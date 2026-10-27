using Microsoft.VisualStudio.TestTools.UnitTesting;
using Frends.LDAP.SearchObjects.Definitions;
using Novell.Directory.Ldap;
using System.Text;

namespace Frends.LDAP.SearchObjects.Tests;

[TestClass]
public class UnitTests
{

    /*
        Create a simple LDAP server to docker
        docker run -d -it --rm -p 10389:10389 dwimberger/ldap-ad-it
    */

    private readonly string? _host = "127.0.0.1";
    private readonly int _port = 10389;
    private readonly string? _user = "uid=admin,ou=system";
    private readonly string? _pw = "secret";
    private readonly string _path = "ou=users,dc=wimpi,dc=net";
    private readonly List<string> _cns = new() { "Tes Tuser", "Qwe Rty", "Foo Bar" };
    private readonly byte[] _photo = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../TestData/test.png"));

    Input? input;
    Connection? connection;

    [TestInitialize]
    public void Setup()
    {
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
            LDAPProtocolVersion = LDAPVersion.V3
        };

        try
        {
            CreateTestUsers();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [TestMethod]
    public void Search_ScopeSub_Test()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefNever,
            MaxResults = default,
            BatchSize = default,
            TypesOnly = default,
            Attributes = null,
            ContentEncoding = ContentEncoding.UTF8,
            EnableBom = false,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);
        Assert.IsTrue(result.Success.Equals(true));

        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("givenname")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Te")) &&
            x.AttributeSet.Any(y => y.Key.Equals("title")) &&
            x.AttributeSet.Any(y => y.Value.Equals("engineer"))
        ));

        //Others
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("uid=test,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Qwe Rty,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Foo Bar,ou=users,dc=wimpi,dc=net")));
    }

    [TestMethod]
    public void Search_ScopeOne_Test()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeOne,
            Filter = null,
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefNever,
            MaxResults = default,
            BatchSize = default,
            TypesOnly = default,
            Attributes = null,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("givenname")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Te")) &&
            x.AttributeSet.Any(y => y.Key.Equals("title")) &&
            x.AttributeSet.Any(y => y.Value.Equals("engineer"))
        ));

        //Others
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("uid=test,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Qwe Rty,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Foo Bar,ou=users,dc=wimpi,dc=net")));
    }

    [TestMethod]
    public void Search_DerefSearching_Test()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefSearching,
            MaxResults = default,
            BatchSize = default,
            TypesOnly = default,
            Attributes = null,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("givenname")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Te")) &&
            x.AttributeSet.Any(y => y.Key.Equals("title")) &&
            x.AttributeSet.Any(y => y.Value.Equals("engineer"))
        ));

        //Others
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("uid=test,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Qwe Rty,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Foo Bar,ou=users,dc=wimpi,dc=net")));
    }

    [TestMethod]
    public void Search_DerefAlways_Test()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefAlways,
            MaxResults = default,
            BatchSize = default,
            TypesOnly = default,
            Attributes = null,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("givenname")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Te")) &&
            x.AttributeSet.Any(y => y.Key.Equals("title")) &&
            x.AttributeSet.Any(y => y.Value.Equals("engineer"))
        ));

        //Others
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("uid=test,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Qwe Rty,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Foo Bar,ou=users,dc=wimpi,dc=net")));
    }

    [TestMethod]
    public void Search_DerefFinding_Test()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefFinding,
            MaxResults = default,
            BatchSize = default,
            TypesOnly = default,
            Attributes = null,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("givenname")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Te")) &&
            x.AttributeSet.Any(y => y.Key.Equals("title")) &&
            x.AttributeSet.Any(y => y.Value.Equals("engineer"))
        ));

        //Others
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("uid=test,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Qwe Rty,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Foo Bar,ou=users,dc=wimpi,dc=net")));
    }

    [TestMethod]
    public void Search_BatchSize_Test()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefNever,
            MaxResults = default,
            BatchSize = 0,
            TypesOnly = default,
            Attributes = null,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("givenname")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Te")) &&
            x.AttributeSet.Any(y => y.Key.Equals("title")) &&
            x.AttributeSet.Any(y => y.Value.Equals("engineer"))
        ));

        //Others
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("uid=test,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Qwe Rty,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Foo Bar,ou=users,dc=wimpi,dc=net")));
    }

    [TestMethod]
    public void Search_MaxResults_Test()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefNever,
            MaxResults = 2,
            BatchSize = default,
            TypesOnly = default,
            Attributes = null,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);
        Assert.IsTrue(result.Success.Equals(true) && result.SearchResult.Count == 2);
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName is not null &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value is not null) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value is not null) &&
            x.AttributeSet.Any(y => y.Key.Equals("objectclass")) &&
            x.AttributeSet.Any(y => y.Value is not null) &&
            x.AttributeSet.Any(y => y.Key.Equals("givenname")) &&
            x.AttributeSet.Any(y => y.Value is not null) &&
            x.AttributeSet.Any(y => y.Key.Equals("title")) &&
            x.AttributeSet.Any(y => y.Value is not null))
        );
    }

    [TestMethod]
    public void Search_TypesOnly_Test()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefNever,
            MaxResults = default,
            BatchSize = default,
            TypesOnly = true,
            Attributes = null,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value is null) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value is null) &&
            x.AttributeSet.Any(y => y.Key.Equals("objectclass")) &&
            x.AttributeSet.Any(y => y.Value is null) &&
            x.AttributeSet.Any(y => y.Key.Equals("givenname")) &&
            x.AttributeSet.Any(y => y.Value is null) &&
            x.AttributeSet.Any(y => y.Key.Equals("title")) &&
            x.AttributeSet.Any(y => y.Value is null)
        ));

        //Others
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("uid=test,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Qwe Rty,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Foo Bar,ou=users,dc=wimpi,dc=net")));
    }

    [TestMethod]
    public void Search_Filter_Test()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = "(title=engineer)",
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefNever,
            MaxResults = default,
            BatchSize = default,
            PageSize = 500,
            TypesOnly = default,
            Attributes = null,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);
        Assert.IsTrue(result.Success.Equals(true) && result.SearchResult.Count == 2);
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("givenname")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Te")) &&
            x.AttributeSet.Any(y => y.Key.Equals("title")) &&
            x.AttributeSet.Any(y => y.Value.Equals("engineer"))
        ));

        //Others
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Foo Bar,ou=users,dc=wimpi,dc=net")));

        Assert.IsFalse(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("uid=test,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Qwe Rty,ou=users,dc=wimpi,dc=net")));
    }

    [TestMethod]
    public void Search_Attributes_Test()
    {
        var atr = new List<Attributes>
        {
            new Attributes() { Key = "photo", ReturnType = ReturnType.ByteArray },
            new Attributes() { Key = "cn", ReturnType = ReturnType.String },
        };

        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefNever,
            MaxResults = default,
            BatchSize = default,
            TypesOnly = default,
            SearchOnlySpecifiedAttributes = true,
            Attributes = atr.ToArray(),
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);
        Assert.IsTrue(result.Success.Equals(true));

        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")))
        );
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("photo")))
        );

        Assert.AreEqual(Convert.ToBase64String(result.SearchResult.First(x => x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net")).AttributeSet.First(x => x.Key.Equals("photo")).Value), Convert.ToBase64String(_photo));

        Assert.IsFalse(result.SearchResult.Any(x =>
            x.AttributeSet.Any(y => y.Key.Equals("sn")) ||
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("givenname")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Te")) &&
            x.AttributeSet.Any(y => y.Key.Equals("title")) &&
            x.AttributeSet.Any(y => y.Value.Equals("engineer"))
        ));

        //Others
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Foo Bar,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("uid=test,ou=users,dc=wimpi,dc=net") ||
            x.DistinguishedName.Equals("CN=Qwe Rty,ou=users,dc=wimpi,dc=net")));
    }

    [TestMethod]
    public void Search_TestMissingParameters()
    {
        var conns = new Connection[]
        {
            new()
            {
                Host = "",
                User = "",
                Password = "",
                SecureSocketLayer = false,
                Port = 0,
                TLS = false,
                LDAPProtocolVersion = LDAPVersion.V3
            },
            new()
            {
                Host = _host,
                User = "",
                Password = "",
                SecureSocketLayer = false,
                Port = 0,
                TLS = false,
                LDAPProtocolVersion = LDAPVersion.V3
            },
            new()
            {
                Host = _host,
                User = _user,
                Password = "",
                SecureSocketLayer = false,
                Port = 0,
                TLS = false,
                LDAPProtocolVersion = LDAPVersion.V3
            }
        };

        var errors = new string[] { "Host is missing.", "Username is missing.", "Password is missing." };

        var index = 0;

        foreach (var conn in conns)
        {
            var ex = Assert.ThrowsException<Exception>(() => LDAP.SearchObjects(input, conn, new Options(), default));
            Assert.AreEqual(errors[index], ex.Message);
            index++;
        }
    }

    [TestMethod]
    public void Search_InvalidCredentials()
    {
        connection = new()
        {
            Host = _host,
            User = "invalidUser",
            Password = "invalisPass",
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
            LDAPProtocolVersion = LDAPVersion.V3,
        };

        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MsLimit = default,
            ServerTimeLimit = default,
            SearchDereference = SearchDereference.DerefNever,
            MaxResults = default,
            BatchSize = default,
            TypesOnly = default,
            Attributes = null,
            ContentEncoding = ContentEncoding.UTF8,
            EnableBom = false,
        };

        var ex = Assert.ThrowsException<LdapException>(() => LDAP.SearchObjects(input, connection, new Options(), default));
        Assert.AreEqual("Invalid DN Syntax", ex.Message);
        Console.WriteLine(ex.Message);
    }

    public void CreateTestUsers()
    {
        LdapConnection conn = new();
        conn.Connect(_host, _port);
        conn.Bind(_user, _pw);

        foreach (var i in _cns)
        {
            var title = i.Contains("Qwe Rty") ? "Coffee maker" : "engineer";
            LdapAttributeSet attributeSet = new();
            attributeSet.Add(new LdapAttribute("objectclass", "inetOrgPerson"));
            attributeSet.Add(new LdapAttribute("cn", i));
            attributeSet.Add(new LdapAttribute("givenname", i[..2]));
            attributeSet.Add(new LdapAttribute("sn", i[4..]));
            attributeSet.Add(new LdapAttribute("title", title));
            attributeSet.Add(new LdapAttribute("photo", _photo));

            var entry = $"CN={i},{_path}";
            LdapEntry newEntry = new(entry, attributeSet);
            conn.Add(newEntry);
        }

        conn.Disconnect();
    }

    [TestMethod]
    public void Search_MaxResults_LessThanAvailable_ReturnsLimitedResults()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MaxResults = 2,
            PageSize = 3,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);

        Assert.IsTrue(result.Success, "Search failed.");
        Assert.AreEqual(2, result.SearchResult.Count, "Should return only 2 results.");
    }

    [TestMethod]
    public void Search_PageSizeSmallerThanTotal_ShouldReturnAll()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MaxResults = 5,
            PageSize = 2,
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);

        Assert.IsTrue(result.Success, "Search failed.");
        Assert.AreEqual(5, result.SearchResult.Count, "Should return all 5 results using paging.");
    }

    [TestMethod]
    public void Search_PageSizeZero_DisablesPaging()
    {
        input = new()
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = null,
            MaxResults = 5,
            PageSize = 0, // No paging
        };

        var result = LDAP.SearchObjects(input, connection, new Options(), default);

        Assert.IsTrue(result.Success, "Search failed.");
        Assert.AreEqual(5, result.SearchResult.Count, "Should return all 5 results without paging.");
    }
}