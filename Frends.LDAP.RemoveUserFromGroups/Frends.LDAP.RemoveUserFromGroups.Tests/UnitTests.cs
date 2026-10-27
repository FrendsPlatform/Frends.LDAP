using Frends.LDAP.RemoveUserFromGroups.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novell.Directory.Ldap;

namespace Frends.LDAP.RemoveUserFromGroups.Tests;

[TestClass]
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
    private readonly string path = "ou=users,dc=wimpi,dc=net";
    private readonly string? groupDn = "cn=admin,ou=roles,dc=wimpi,dc=net";

    private Input? input;
    private Connection? connection;

    [TestMethod]
    public void Update_ThrowErrorOnFailure_True_Test()
    {
        var tuser = "Tes Tuser" + Guid.NewGuid().ToString();
        var dn = $"CN={tuser},{path}";
        CreateTestUsers(tuser);

        input = new()
        {
            UserDistinguishedName = dn,
            GroupDistinguishedName = groupDn,
        };
        connection = new()
        {
            Host = host,
            User = user,
            Password = pw,
            SecureSocketLayer = false,
            Port = port,
            TLS = false,
        };

        var ex = Assert.ThrowsException<Exception>(() => LDAP.RemoveUserFromGroups(input, connection, new Options(), CancellationToken.None));
        Assert.IsTrue(ex.Message != null);
    }

    [TestMethod]
    public void Update_ThrowErrorOnFailure_False_Test()
    {
        var tuser = "Tes Tuser" + Guid.NewGuid().ToString();
        var dn = $"CN={tuser},{path}";
        CreateTestUsers(tuser);

        input = new()
        {
            UserDistinguishedName = dn,
            GroupDistinguishedName = groupDn,
        };
        connection = new()
        {
            Host = host,
            User = user,
            Password = pw,
            SecureSocketLayer = false,
            Port = port,
            TLS = false,
        };

        var result = LDAP.RemoveUserFromGroups(input, connection, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(false) && result.Error != null);
    }

    [TestMethod]
    public void RemoveUserFromGroups_Test()
    {
        var tuser = "Tes Tuser" + Guid.NewGuid().ToString();
        var dn = $"CN={tuser},{path}";
        CreateTestUsers(tuser);
        AddUserToGroup(dn);

        input = new()
        {
            UserDistinguishedName = dn,
            GroupDistinguishedName = groupDn,
        };
        connection = new()
        {
            Host = host,
            User = user,
            Password = pw,
            SecureSocketLayer = false,
            Port = port,
            TLS = false,
        };

        var result = LDAP.RemoveUserFromGroups(input, connection, new Options(), CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(true));
    }

    public void CreateTestUsers(string tuser)
    {
        try
        {
            LdapConnection conn = new();
            conn.Connect(host, port);
            conn.Bind(user, pw);

            var attributeSet = new LdapAttributeSet();
            attributeSet.Add(new LdapAttribute("objectclass", "inetorgperson"));
            attributeSet.Add(new LdapAttribute("cn", tuser));
            attributeSet.Add(new LdapAttribute("givenname", "Tes"));
            attributeSet.Add(new LdapAttribute("sn", tuser.Split(' ', 1)));

            var entry = $"CN={tuser},{path}";
            LdapEntry newEntry = new(entry, attributeSet);
            conn.Add(newEntry);
            conn.Disconnect();
        }
        catch (Exception)
        {
        }
    }

    public void AddUserToGroup(string dn)
    {
        try
        {
            LdapConnection conn = new();
            conn.Connect(host, port);
            conn.Bind(user, pw);

            LdapModification[] mods = new LdapModification[1];
            var member = new LdapAttribute("member", dn);
            mods[0] = new LdapModification(LdapModification.Add, member);
            conn.Modify(groupDn, mods);
            conn.Disconnect();
        }
        catch (Exception)
        {
        }
    }
}