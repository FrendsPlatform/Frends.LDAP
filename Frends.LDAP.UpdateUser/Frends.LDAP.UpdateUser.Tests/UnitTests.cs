using Frends.LDAP.UpdateUser.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novell.Directory.Ldap;

namespace Frends.LDAP.UpdateUser.Tests;

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
    private readonly Options options = new() { ThrowErrorOnFailure = false };

    private Input? input;
    private Connection? connection;

    [TestCleanup]
    public void CleanUp()
    {
        DeleteUser();
    }

    [TestMethod]
    public void Update_HandleLDAPError_Test()
    {
        CreateTestUsers(false);

        input = new()
        {
            Path = "CN=qw,DC=er,DC=ty",
            CommonName = "FAil" + Guid.Empty,
            ModificationMethod = ModificationMethod.Add,
            CreateDN = true,
            Attributes = new[] { new Attributes { Key = "title", Value = "senior coffee maker" } },
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

        var result = LDAP.UpdateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(false) && result.Error.Message.Contains("No Such Object"));
    }

    [TestMethod]
    public void UpdateUser_Add_New_CreateDNFalse_Test()
    {
        CreateTestUsers(false);

        input = new()
        {
            DistinguishedName = "CN=Tes TUser,ou=users,dc=wimpi,dc=net",
            ModificationMethod = ModificationMethod.Add,
            CreateDN = false,
            Attributes = new[] { new Attributes { Key = "title", Value = "senior coffee maker" } },
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

        var result = LDAP.UpdateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(true));
    }

    [TestMethod]
    public void UpdateUser_Add_New_Test()
    {
        CreateTestUsers(false);

        input = new()
        {
            Path = "ou=users,dc=wimpi,dc=net",
            CommonName = "Tes TUser",
            ModificationMethod = ModificationMethod.Add,
            CreateDN = true,
            Attributes = new[] { new Attributes { Key = "title", Value = "senior coffee maker" } },
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

        var result = LDAP.UpdateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(true));
    }

    [TestMethod]
    public void UpdateUser_Add_AlreadyHaveValue_Test()
    {
        CreateTestUsers(true);

        input = new()
        {
            Path = "ou=users,dc=wimpi,dc=net",
            CommonName = "Tes TUser",
            ModificationMethod = ModificationMethod.Add,
            CreateDN = true,
            Attributes = new[] { new Attributes { Key = "title", Value = "senior coffee maker" } },
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

        var result = LDAP.UpdateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(true));
    }

    [TestMethod]
    public void UpdateUser_Delete_WithValue_DoesntExistsFail_Test()
    {
        CreateTestUsers(false);

        input = new()
        {
            Path = "ou=users,dc=wimpi,dc=net",
            CommonName = "Tes TUser",
            ModificationMethod = ModificationMethod.Delete,
            CreateDN = true,
            Attributes = new[] { new Attributes { Key = "title", Value = "coffee maker" } },
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

        var result = LDAP.UpdateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(false) && result.Error.Message.Contains("No Such Attribute"));
    }

    [TestMethod]
    public void UpdateUser_Delete_WithValue_Success_Test()
    {
        CreateTestUsers(true);

        input = new()
        {
            Path = "ou=users,dc=wimpi,dc=net",
            CommonName = "Tes TUser",
            ModificationMethod = ModificationMethod.Delete,
            CreateDN = true,
            Attributes = new[] { new Attributes { Key = "title", Value = "coffee maker" } },
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

        var result = LDAP.UpdateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(true));
    }

    [TestMethod]
    public void UpdateUser_Delete_WithoutValue_Test()
    {
        CreateTestUsers(true);

        input = new()
        {
            Path = "ou=users,dc=wimpi,dc=net",
            CommonName = "Tes TUser",
            ModificationMethod = ModificationMethod.Add,
            CreateDN = true,
            Attributes = new[] { new Attributes { Key = "title", Value = string.Empty } },
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

        var result = LDAP.UpdateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(true));
    }

    [TestMethod]
    public void UpdateUser_Replace_WithValue_Test()
    {
        CreateTestUsers(false);

        input = new()
        {
            Path = "ou=users,dc=wimpi,dc=net",
            CommonName = "Tes TUser",
            ModificationMethod = ModificationMethod.Replace,
            CreateDN = true,
            Attributes = new[] { new Attributes { Key = "title", Value = "senior coffee maker" } },
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

        var result = LDAP.UpdateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(true));
    }

    [TestMethod]
    public void UpdateUser_Replace_WithoutValue_Test()
    {
        CreateTestUsers(true);

        input = new()
        {
            Path = "ou=users,dc=wimpi,dc=net",
            CommonName = "Tes TUser",
            ModificationMethod = ModificationMethod.Replace,
            CreateDN = true,
            Attributes = new[] { new Attributes { Key = "title", Value = string.Empty } },
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

        var result = LDAP.UpdateUser(input, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success.Equals(true));
    }

    public void CreateTestUsers(bool setTitle)
    {
        LdapConnection conn = new();
        conn.Connect(host, port);
        conn.Bind(user, pw);

        var attributeSet = new LdapAttributeSet
        {
            new LdapAttribute("objectclass", "inetOrgPerson"),
            new LdapAttribute("cn", "Tes Tuser"),
            new LdapAttribute("givenname", "Tes"),
            new LdapAttribute("sn", "Tuser"),
        };

        if (setTitle)
            attributeSet.Add(new LdapAttribute("title", "coffee maker"));

        var entry = $"CN=Tes Tuser,{path}";
        LdapEntry newEntry = new(entry, attributeSet);
        conn.Add(newEntry);
        conn.Disconnect();
    }

    public void DeleteUser()
    {
        LdapConnection conn = new();
        conn.Connect(host, port);
        conn.Bind(user, pw);
        conn.Delete($"CN=Tes Tuser,{path}");
        conn.Disconnect();
    }
}
