namespace Frends.LDAP.AddUserToGroups.Tests;

using Frends.LDAP.AddUserToGroups.Definitions;
using Novell.Directory.Ldap;
using NUnit.Framework;

[TestFixture]
public class UnitTests
{
    /*
        LDAP server to docker.
        docker run -d -it --rm -p 10389:10389 dwimberger/ldap-ad-it
    */
    private readonly string? host = "127.0.0.1";
    private readonly int port = 10389;
    private readonly string? user = "uid=admin,ou=system";
    private readonly string? pw = "secret";
    private readonly string groupDn = "cn=admin,ou=roles,dc=wimpi,dc=net";
    private readonly string groupDn2 = "cn=developers,ou=roles,dc=wimpi,dc=net";
    private readonly string testUserDn = "CN=Test User,ou=users,dc=wimpi,dc=net";

    private Input? input;
    private Connection? connection;

    [SetUp]
    public void SetUp()
    {
        connection = new()
        {
            Host = host,
            User = user,
            Password = pw,
            SecureSocketLayer = false,
            Port = port,
            TLS = false,
        };

        CreateTestUser(testUserDn);
        CreateTestGroups("admin", groupDn);
        CreateTestGroups("developers", groupDn2);
    }

    [TearDown]
    public void Teardown()
    {
        DeleteTestUsers(testUserDn, new[] { groupDn, groupDn2 });
        DeleteTestGroups(groupDn);
        DeleteTestGroups(groupDn2);
    }

    [Test]
    public void Update_HandleLDAPError_Test()
    {
        input = new()
        {
            UserDistinguishedName = "CN=Common Name,CN=Users,DC=Example,DC=Com",
            GroupDistinguishedNames = new[] { "CN=Admins,DC=Example,DC=Com" },
            UserExistsAction = UserExistsAction.Throw,
        };

        var ex = Assert.Throws<Exception>(() => LDAP.AddUserToGroups(input, connection, new Options(), default));
        Assert.IsTrue(ex.Message.Contains("No Such Object"));
    }

    [Test]
    public void AddUserToGroups_SingleGroup_Test()
    {
        input = new()
        {
            UserDistinguishedName = testUserDn,
            GroupDistinguishedNames = new[] { groupDn },
            UserExistsAction = UserExistsAction.Throw,
        };

        var result = LDAP.AddUserToGroups(input, connection, new Options(), default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Error);
        Assert.IsNull(result.Details);
    }

    [Test]
    public void AddUserToGroups_MultipleGroups_Test()
    {
        input = new()
        {
            UserDistinguishedName = testUserDn,
            GroupDistinguishedNames = new[] { groupDn, groupDn2 },
            UserExistsAction = UserExistsAction.Throw,
        };

        var result = LDAP.AddUserToGroups(input, connection, new Options(), default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Error);
        Assert.IsNull(result.Details);

        Assert.IsTrue(VerifyUserInGroup(testUserDn, groupDn));
        Assert.IsTrue(VerifyUserInGroup(testUserDn, groupDn2));
    }

    [Test]
    public void AddUserToGroups_TestWithUserExisting()
    {
        input = new()
        {
            UserDistinguishedName = testUserDn,
            GroupDistinguishedNames = new[] { groupDn },
            UserExistsAction = UserExistsAction.Throw,
        };

        var result = LDAP.AddUserToGroups(input, connection, new Options(), default);
        Assert.IsTrue(result.Success);

        var ex = Assert.Throws<Exception>(() => LDAP.AddUserToGroups(input, connection, new Options(), default));
        Assert.IsTrue(ex.Message.Contains("User already exists in the group"));
    }

    [Test]
    public void AddUserToGroups_TestWithUserExistingWithSkip()
    {
        input = new()
        {
            UserDistinguishedName = testUserDn,
            GroupDistinguishedNames = new[] { groupDn },
            UserExistsAction = UserExistsAction.Skip,
        };

        var result = LDAP.AddUserToGroups(input, connection, new Options(), default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Error);
        Assert.IsNull(result.Details);

        result = LDAP.AddUserToGroups(input, connection, new Options(), default);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Details);
        Assert.IsTrue(result.Details.Contains("User already exists"));
    }

    [Test]
    public void AddUserToGroups_MultipleGroups_OneAlreadyExists_Skip_Test()
    {
        input = new()
        {
            UserDistinguishedName = testUserDn,
            GroupDistinguishedNames = new[] { groupDn },
            UserExistsAction = UserExistsAction.Throw,
        };
        LDAP.AddUserToGroups(input, connection, new Options(), default);

        input = new()
        {
            UserDistinguishedName = testUserDn,
            GroupDistinguishedNames = new[] { groupDn, groupDn2 },
            UserExistsAction = UserExistsAction.Skip,
        };

        var result = LDAP.AddUserToGroups(input, connection, new Options(), default);

        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Error);
        Assert.IsNotNull(result.Details);
        Assert.IsTrue(result.Details.Contains("Added to 1 group(s)"));
        Assert.IsTrue(result.Details.Contains("Skipped 1 group(s)"));

        Assert.IsTrue(VerifyUserInGroup(testUserDn, groupDn));
        Assert.IsTrue(VerifyUserInGroup(testUserDn, groupDn2));
    }

    [Test]
    public void AddUserToGroups_MultipleGroups_AllAlreadyExist_Skip_Test()
    {
        input = new()
        {
            UserDistinguishedName = testUserDn,
            GroupDistinguishedNames = new[] { groupDn, groupDn2 },
            UserExistsAction = UserExistsAction.Throw,
        };
        LDAP.AddUserToGroups(input, connection, new Options(), default);

        input = new()
        {
            UserDistinguishedName = testUserDn,
            GroupDistinguishedNames = new[] { groupDn, groupDn2 },
            UserExistsAction = UserExistsAction.Skip,
        };

        var result = LDAP.AddUserToGroups(input, connection, new Options(), default);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Details);
        Assert.IsTrue(result.Details.Contains("User already exists"));
    }

    private bool VerifyUserInGroup(string userDn, string groupDn)
    {
        using LdapConnection conn = new();
        conn.Connect(host, port);
        conn.Bind(user, pw);

        ILdapSearchResults searchResults = conn.Search(
            groupDn,
            LdapConnection.ScopeSub,
            "(objectClass=*)",
            null,
            false);

        if (searchResults.HasMore())
        {
            var entry = searchResults.Next();
            var memberAttr = entry.GetAttribute("member");
            conn.Disconnect();
            return memberAttr.StringValueArray.Contains(userDn, StringComparer.OrdinalIgnoreCase);
        }

        conn.Disconnect();
        return false;
    }

    private void CreateTestUser(string userDn)
    {
        using LdapConnection conn = new()
        {
            SecureSocketLayer = false,
        };
        conn.Connect(host, port);
        conn.Bind(user, pw);

        try
        {
            conn.Delete(userDn);
        }
        catch
        {
            // User doesn't exist, that's fine
        }

        var attributeSet = new LdapAttributeSet
        {
        new LdapAttribute("objectclass", "inetOrgPerson"),
        new LdapAttribute("cn", "Test User"),
        new LdapAttribute("givenname", "Test"),
        new LdapAttribute("sn", "User"),
        };

        LdapEntry newEntry = new(userDn, attributeSet);
        conn.Add(newEntry);
        conn.Disconnect();
    }

    private void DeleteTestUsers(string userDn, string[] groupDns)
    {
        using LdapConnection conn = new();
        conn.Connect(host, port);
        conn.Bind(user, pw);

        foreach (var groupDn in groupDns)
        {
            try
            {
                ILdapSearchResults searchResults = conn.Search(
                    groupDn,
                    LdapConnection.ScopeSub,
                    "(objectClass=*)",
                    null,
                    false);

                LdapEntry groupEntry = searchResults.Next();
                LdapAttribute memberAttr = groupEntry.GetAttribute("member");
                var currentMembers = memberAttr.StringValueArray;

                if (currentMembers.Any(e => e == userDn))
                {
                    var mod = new LdapModification(LdapModification.Delete, new LdapAttribute("member", userDn));
                    conn.Modify(groupDn, mod);
                }
            }
            catch
            {
                // Group might not exist, continue
            }
        }

        try
        {
            conn.Delete(userDn);
        }
        catch
        {
            // User might not exist
        }

        conn.Disconnect();
    }

    private void DeleteTestGroups(string dn)
    {
        using LdapConnection conn = new();
        conn.Connect(host, port);
        conn.Bind(user, pw);

        try
        {
            conn.Delete(dn);
        }
        catch (LdapException ex) when (ex.ResultCode == LdapException.NoSuchObject)
        {
        }

        conn.Disconnect();
    }

    private void CreateTestGroups(string cn, string dn)
    {
        using LdapConnection conn = new();
        conn.Connect(host, port);
        conn.Bind(user, pw);

        try
        {
            var groupAttr = new LdapAttributeSet
        {
            new LdapAttribute("objectClass", new[] { "top", "groupOfNames" }),
            new LdapAttribute("cn", cn),
            new LdapAttribute("member", "uid=admin,ou=system"),
        };
            LdapEntry group = new(dn, groupAttr);
            conn.Add(group);
        }
        catch (LdapException ex) when (ex.ResultCode == LdapException.EntryAlreadyExists)
        {
            // Group already exists, ignore
        }

        conn.Disconnect();
    }
}