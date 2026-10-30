using System.Threading;
using Frends.LDAP.DeleteUser.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novell.Directory.Ldap;

namespace Frends.LDAP.DeleteUser.Tests;

[TestClass]
public class UnitTests
{
    /*
        Create a simple LDAP server to docker
        docker run -d -it --rm -p 10389:10389 dwimberger/ldap-ad-it
    */

    private readonly string? host = "127.0.0.1";
    private readonly int port = 10389;
    private readonly string? user = "uid=admin,ou=system";
    private readonly string? pw = "secret";
    private readonly string path = "ou=users,dc=wimpi,dc=net";
    private readonly string commonName = "Test User";

    private Input? input;
    private Connection? connection;

    [TestMethod]
    public void Create_HandleLDAPError_Test()
    {
        input = new()
        {
            Path = path,
            CommonName = "Fail" + Guid.NewGuid(),
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

        var result = LDAP.DeleteUser(input, connection, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(false) && result.Error.Message.Contains("No Such Object"));
    }

    [TestMethod]
    public void DeleteUser_Test()
    {
        CreateTestUser();

        input = new()
        {
            Path = path,
            CommonName = commonName,
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

        var result = LDAP.DeleteUser(input, connection, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(true));
    }

    public void CreateTestUser()
    {
        var entry = $"CN={commonName},{path}";

        LdapConnection conn = new();
        conn.Connect(host, port);
        conn.Bind(user, pw);

        LdapAttributeSet attributeSet = new();
        attributeSet.Add(new LdapAttribute("objectclass", "inetOrgPerson"));
        attributeSet.Add(new LdapAttribute("cn", commonName));
        attributeSet.Add(new LdapAttribute("givenname", "Test"));
        attributeSet.Add(new LdapAttribute("sn", "User"));

        LdapEntry newEntry = new(entry, attributeSet);
        conn.Add(newEntry);
        conn.Disconnect();
    }
}