using Frends.LDAP.SearchObjects.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.LDAP.SearchObjects.Tests;

[TestClass]
public class UnitTestAnonymousBind
{
    /*
        Create a simple LDAP server to docker for anonymous bind testing
        docker run --rm -p 20389:389 -e LDAP_ORGANISATION="Test Org" -e LDAP_DOMAIN="example.org" -e LDAP_ADMIN_PASSWORD="admin" osixia/openldap:1.5.0
    */

    private readonly string? _host = "127.0.0.1";
    private readonly int _port = 20389;
    private readonly string? _user = "cn=admin,dc=example,dc=org";
    private readonly string? _pw = "admin";
    private readonly string _path = "dc=example,dc=org";

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
    }

    [TestMethod]
    public void Search_AnonymousBind_ShouldFailAndReturnError()
    {
        var badConnection = new Connection
        {
            Host = _host,
            Port = _port,
            SecureSocketLayer = false,
            TLS = false,
            LDAPProtocolVersion = LDAPVersion.V3,
            AnonymousBind = true,
            ThrowExceptionOnError = false
        };

        var badInput = new Input
        {
            SearchBase = _path,
            Scope = Scopes.ScopeSub,
            Filter = "(objectClass=*)",
            PageSize = 2,
            MaxResults = 5
        };

        var result = LDAP.SearchObjects(badInput, badConnection, CancellationToken.None);

        Assert.IsFalse(result.Success, "Expected Success to be false when anonymous bind fails.");
        Assert.IsTrue(!string.IsNullOrEmpty(result.Error), "Expected an error message.");
        Assert.IsTrue(result.Error.Contains("bind"), "Expected error message to mention bind.");
    }
}
