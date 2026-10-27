using Frends.LDAP.CreateUser.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novell.Directory.Ldap;

namespace Frends.LDAP.CreateUser.Tests;

[TestClass]
public class UnitTests
{
    /*
        Create a simple LDAP server to docker
        docker run -it --rm -p 10389:10389 dwimberger/ldap-ad-it
    */

    private readonly string? host = "127.0.0.1";
    private readonly int port = 10389;
    private readonly string? user = "uid=admin,ou=system";
    private readonly string? pw = "secret";

    private Input? input;
    private Connection? connection;
    private Options options = new() { ThrowErrorOnFailure = false };

    [TestMethod]
    public void Create_HandleLDAPError_Test()
    {
        input = new()
        {
            Path = "CN=qw,DC=er,DC=ty",
            CommonName = "FAil" + Guid.NewGuid(),
            ObjectClass = "inetOrgPerson",
            GivenName = "Ail",
            Surname = "F",
            SetPassword = true,
            Password = "Qwerty123!",
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

        var result = LDAP.CreateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(false) && result.Error.Contains("No Such Object"));
    }

    [TestMethod]
    public void UnicodedPassword_Test()
    {
        input = new()
        {
            Path = "CN=qw,DC=er,DC=ty",
            CommonName = "FAil" + Guid.NewGuid(),
            ObjectClass = "inetOrgPerson",
            GivenName = "Ail",
            Surname = "F",
            SetPassword = true,
            SetPasswordInUnicode = true,
            Password = "Qwerty123!",
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

        var ex = Assert.ThrowsException<Exception>(() => LDAP.CreateUser(input, connection, options, CancellationToken.None));
        StringAssert.Contains(ex.Message, "require SSL or TLS");
    }

    [TestMethod]
    public void CreateUser_Test()
    {
        input = new()
        {
            Path = "ou=users,dc=wimpi,dc=net",
            CommonName = "Test User",
            ObjectClass = "inetOrgPerson",
            GivenName = "Test",
            Surname = "User",
            SetPassword = true,
            Password = "Qwerty123!",
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

        var result = LDAP.CreateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(true));
        DeleteUser(input.CommonName, input.Path);
    }

    public void DeleteUser(string cn, string path)
    {
        LdapConnection ldapConn = new();
        ldapConn.Connect(host, port);
        ldapConn.Bind(user, pw);
        ldapConn.Delete($"CN={cn},{path}");
    }
}