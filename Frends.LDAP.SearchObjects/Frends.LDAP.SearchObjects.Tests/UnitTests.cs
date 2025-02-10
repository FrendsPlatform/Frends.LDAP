using Microsoft.VisualStudio.TestTools.UnitTesting;
using Frends.LDAP.SearchObjects.Definitions;
using Novell.Directory.Ldap;
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

    Input? input;
    Connection? connection;

    [TestInitialize]
    public void Setup()
    {
        try
        {
            CreateTestUsers();
        }
        catch (Exception)
        {
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

        var result = LDAP.SearchObjects(input, connection, default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("objectclass")) &&
            x.AttributeSet.Any(y => y.Value.Equals("top")) &&
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
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.SearchObjects(input, connection, default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("objectclass")) &&
            x.AttributeSet.Any(y => y.Value.Equals("top")) &&
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
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.SearchObjects(input, connection, default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("objectclass")) &&
            x.AttributeSet.Any(y => y.Value.Equals("top")) &&
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
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.SearchObjects(input, connection, default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("objectclass")) &&
            x.AttributeSet.Any(y => y.Value.Equals("top")) &&
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
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.SearchObjects(input, connection, default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("objectclass")) &&
            x.AttributeSet.Any(y => y.Value.Equals("top")) &&
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
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.SearchObjects(input, connection, default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("objectclass")) &&
            x.AttributeSet.Any(y => y.Value.Equals("top")) &&
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
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.SearchObjects(input, connection, default);
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
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.SearchObjects(input, connection, default);
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
        connection = new()
        {
            Host = _host,
            User = _user,
            Password = _pw,
            SecureSocketLayer = false,
            Port = _port,
            TLS = false,
        };

        var result = LDAP.SearchObjects(input, connection, default);
        Assert.IsTrue(result.Success.Equals(true) && result.SearchResult.Count == 2);
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("sn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")) &&
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("objectclass")) &&
            x.AttributeSet.Any(y => y.Value.Equals("top")) &&
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
            new Attributes() { Key = "cn" }
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
            Attributes = atr.ToArray(),
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

        var result = LDAP.SearchObjects(input, connection, default);
        Assert.IsTrue(result.Success.Equals(true));
        Assert.IsTrue(result.SearchResult.Any(x =>
            x.DistinguishedName.Equals("CN=Tes Tuser,ou=users,dc=wimpi,dc=net") &&
            x.AttributeSet.Any(y => y.Key.Equals("cn")))
        );

        Assert.IsFalse(result.SearchResult.Any(x =>
            x.AttributeSet.Any(y => y.Key.Equals("sn")) ||
            x.AttributeSet.Any(y => y.Value.Equals("Tes Tuser")) &&
            x.AttributeSet.Any(y => y.Key.Equals("objectclass")) &&
            x.AttributeSet.Any(y => y.Value.Equals("top")) &&
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

    public void CreateTestUsers()
    {
        LdapConnection conn = new();
        conn.Connect(_host, _port);
        conn.Bind(_user, _pw);

        foreach(var i in _cns)
        {
            var title = i.Contains("Qwe Rty") ? "Coffee maker" : "engineer";
            LdapAttributeSet attributeSet = new();
            attributeSet.Add(new LdapAttribute("objectclass", "inetOrgPerson"));
            attributeSet.Add(new LdapAttribute("cn", i));
            attributeSet.Add(new LdapAttribute("givenname", i[..2]));
            attributeSet.Add(new LdapAttribute("sn", i[4..]));
            attributeSet.Add(new LdapAttribute("title", title));

            var entry = $"CN={i},{_path}";
            LdapEntry newEntry = new(entry, attributeSet);
            conn.Add(newEntry);
        }
        conn.Disconnect();
    }
}