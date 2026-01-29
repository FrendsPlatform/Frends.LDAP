namespace Frends.LDAP.AddUserToGroups.Tests;

using NUnit.Framework;
using Frends.LDAP.AddUserToGroups.Definitions;
using Novell.Directory.Ldap;

[TestFixture]
public class UnitTests
{
    /*
        LDAP server to docker.
        docker run -d -it --rm -p 10389:10389 dwimberger/ldap-ad-it
    */
    private readonly string? _host = "127.0.0.1";
    private readonly int _port = 10389;
    private readonly string? _user = "uid=admin,ou=system";
    private readonly string? _pw = "secret";
    private readonly string _groupDn = "cn=admin,ou=roles,dc=wimpi,dc=net";
    private readonly string _groupDn2 = "cn=developers,ou=roles,dc=wimpi,dc=net";
    private readonly string _testUserDn = "CN=Test User,ou=users,dc=wimpi,dc=net";

    private Input? input;
    private Connection? connection;

    [SetUp]
    public void SetUp()
    {
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        CreateTestUser(_testUserDn);
        CreateTestGroups();
    }

    [TearDown]
    public void Teardown()
    {
        DeleteTestUsers(_testUserDn, new[] { _groupDn, _groupDn2 });
        DeleteTestGroups();
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

        var ex = Assert.Throws<Exception>(() => LDAP.AddUserToGroups(input, connection, default));
        Assert.IsTrue(ex.Message.Contains("No Such Object"));
    }

    [Test]
    public void AddUserToGroups_SingleGroup_Test()
    {
        input = new()
        {
            UserDistinguishedName = _testUserDn,
            GroupDistinguishedNames = new[] { _groupDn },
            UserExistsAction = UserExistsAction.Throw,
        };

        var result = LDAP.AddUserToGroups(input, connection, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Error);
        Assert.IsNull(result.Details);
    }

    [Test]
    public void AddUserToGroups_MultipleGroups_Test()
    {
        input = new()
        {
            UserDistinguishedName = _testUserDn,
            GroupDistinguishedNames = new[] { _groupDn, _groupDn2 },
            UserExistsAction = UserExistsAction.Throw,
        };

        var result = LDAP.AddUserToGroups(input, connection, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Error);
        Assert.IsNull(result.Details);

        Assert.IsTrue(VerifyUserInGroup(_testUserDn, _groupDn));
        Assert.IsTrue(VerifyUserInGroup(_testUserDn, _groupDn2));
    }

    [Test]
    public void AddUserToGroups_TestWithUserExisting()
    {
        input = new()
        {
            UserDistinguishedName = _testUserDn,
            GroupDistinguishedNames = new[] { _groupDn },
            UserExistsAction = UserExistsAction.Throw,
        };

        var result = LDAP.AddUserToGroups(input, connection, default);
        Assert.IsTrue(result.Success);

        var ex = Assert.Throws<Exception>(() => LDAP.AddUserToGroups(input, connection, default));
        Assert.IsTrue(ex.Message.Contains("AddUserToGroups LDAP error: Attribute Or Value Exists"));
    }

    [Test]
    public void AddUserToGroups_TestWithUserExistingWithSkip()
    {
        input = new()
        {
            UserDistinguishedName = _testUserDn,
            GroupDistinguishedNames = new[] { _groupDn },
            UserExistsAction = UserExistsAction.Skip,
        };

        var result = LDAP.AddUserToGroups(input, connection, default);
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Error);
        Assert.IsNull(result.Details);

        result = LDAP.AddUserToGroups(input, connection, default);
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Details);
        Assert.IsTrue(result.Error.Contains("User already exists in the group"));
    }

    [Test]
    public void AddUserToGroups_MultipleGroups_OneAlreadyExists_Skip_Test()
    {
        input = new()
        {
            UserDistinguishedName = _testUserDn,
            GroupDistinguishedNames = new[] { _groupDn },
            UserExistsAction = UserExistsAction.Throw,
        };
        LDAP.AddUserToGroups(input, connection, default);

        input = new()
        {
            UserDistinguishedName = _testUserDn,
            GroupDistinguishedNames = new[] { _groupDn, _groupDn2 },
            UserExistsAction = UserExistsAction.Skip,
        };

        var result = LDAP.AddUserToGroups(input, connection, default);

        Assert.IsTrue(result.Success);
        Assert.IsNull(result.Error);
        Assert.IsNotNull(result.Details);
        Assert.IsTrue(result.Details.Contains("Added to 1 group(s)"));
        Assert.IsTrue(result.Details.Contains("Skipped 1 group(s)"));

        Assert.IsTrue(VerifyUserInGroup(_testUserDn, _groupDn));
        Assert.IsTrue(VerifyUserInGroup(_testUserDn, _groupDn2));
    }

    [Test]
    public void AddUserToGroups_MultipleGroups_AllAlreadyExist_Skip_Test()
    {
        input = new()
        {
            UserDistinguishedName = _testUserDn,
            GroupDistinguishedNames = new[] { _groupDn, _groupDn2 },
            UserExistsAction = UserExistsAction.Throw,
        };
        LDAP.AddUserToGroups(input, connection, default);

        input = new()
        {
            UserDistinguishedName = _testUserDn,
            GroupDistinguishedNames = new[] { _groupDn, _groupDn2 },
            UserExistsAction = UserExistsAction.Skip,
        };

        var result = LDAP.AddUserToGroups(input, connection, default);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNull(result.Details);
        Assert.IsTrue(result.Error.Contains("User already exists in all groups"));
    }

    private bool VerifyUserInGroup(string userDn, string groupDn)
    {
        using LdapConnection conn = new();
        conn.Connect(_host, _port);
        conn.Bind(_user, _pw);

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
        conn.Connect(_host, _port);
        conn.Bind(_user, _pw);

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
        conn.Connect(_host, _port);
        conn.Bind(_user, _pw);

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

    private void DeleteTestGroups()
    {
        using LdapConnection conn = new();
        conn.Connect(_host, _port);
        conn.Bind(_user, _pw);

        conn.Delete(_groupDn);
        conn.Delete(_groupDn2);

        conn.Disconnect();
    }

    private void CreateTestGroups()
    {
        using LdapConnection conn = new();
        conn.Connect(_host, _port);
        conn.Bind(_user, _pw);

        try
        {
            var adminGroupAttr = new LdapAttributeSet
        {
            new LdapAttribute("objectClass", new[] { "top", "groupOfNames" }),
            new LdapAttribute("cn", "admin"),
            new LdapAttribute("member", "uid=admin,ou=system"),
        };
            LdapEntry adminGroup = new(_groupDn, adminGroupAttr);
            conn.Add(adminGroup);
        }
        catch (LdapException ex) when (ex.ResultCode == LdapException.EntryAlreadyExists)
        {
            // Group already exists, ignore
        }

        try
        {
            var devGroupAttr = new LdapAttributeSet
        {
            new LdapAttribute("objectClass", new[] { "top", "groupOfNames" }),
            new LdapAttribute("cn", "developers"),
            new LdapAttribute("member", "uid=admin,ou=system"),
        };
            LdapEntry devGroup = new(_groupDn2, devGroupAttr);
            conn.Add(devGroup);
        }
        catch (LdapException ex) when (ex.ResultCode == LdapException.EntryAlreadyExists)
        {
            // Group already exists, ignore
        }

        conn.Disconnect();
    }
}