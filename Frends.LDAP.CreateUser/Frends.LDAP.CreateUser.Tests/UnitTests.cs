using Microsoft.VisualStudio.TestTools.UnitTesting;
using Frends.LDAP.CreateUser.Definitions;
using Novell.Directory.Ldap;
namespace Frends.LDAP.CreateUser.Tests;

[TestClass]
public class UnitTests
{

    /*
        Create a simple LDAP server to docker
        docker run -it --rm -p 10389:10389 dwimberger/ldap-ad-it
    */

    private readonly string? _host = "127.0.0.1";
    private readonly int _port = 10389;
    private readonly string? _user = "uid=admin,ou=system";
    private readonly string? _pw = "secret";

    Input? input;
    Connection? connection;

    [TestMethod]
    public void Create_HandleLDAPError_Test()
    {
        input = new()
        {
            Path = "CN=qw,DC=er,DC=ty",
            CommonName = "FAil" + new Guid(),
            ObjectClass = "inetOrgPerson",
            GivenName = "Ail",
            Surname = "F",
            SetPassword = true,
            Password = "Qwerty123!",
        };
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.CreateUser(input, connection);
        Assert.IsTrue(result.Success.Equals(false) && result.Error.Contains("No Such Object"));
    }

    [TestMethod]
    public void UnicodedPassword_Test()
    {
        input = new()
        {
            Path = "CN=qw,DC=er,DC=ty",
            CommonName = "FAil" + new Guid(),
            ObjectClass = "inetOrgPerson",
            GivenName = "Ail",
            Surname = "F",
            SetPassword = true,
            SetPasswordInUnicode = true,
            Password = "Qwerty123!",
        };
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.CreateUser(input, connection);
        Assert.IsTrue(result.Error.Contains("No Such Attribute"));
        Assert.IsFalse(result.Success);
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
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.CreateUser(input, connection);
        Assert.IsTrue(result.Success.Equals(true));
        DeleteUser(input.CommonName, input.Path);
    }

    public void DeleteUser(string cn, string path)
    {
        LdapConnection ldapConn = new();
        ldapConn.Connect(_host, _port);
        ldapConn.Bind(_user, _pw);
        ldapConn.Delete($"CN={cn},{path}");
    }
}