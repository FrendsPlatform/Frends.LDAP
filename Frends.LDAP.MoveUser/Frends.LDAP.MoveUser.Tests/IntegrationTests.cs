using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Frends.LDAP.MoveUser.Definitions;
using Novell.Directory.Ldap;
using NUnit.Framework;

namespace Frends.LDAP.MoveUser.Tests;

[TestFixture]
[Category("Integration")]
[NonParallelizable]
internal class IntegrationTests
{
    private const int LdapPort = 389;
    private const string BaseDn = "dc=example,dc=com";

    private readonly List<string> cleanupDns = [];
    private readonly string adminPassword = Guid.NewGuid().ToString("N");
    private IContainer container;
    private Connection connection;
    private LdapConnection admin;
    private string rootDn;
    private string destinationContainer;
    private Input input;

    [OneTimeSetUp]
    public async Task StartLdapContainer()
    {
        container = new ContainerBuilder("osixia/openldap:1.5.0")
            .WithEnvironment("LDAP_DOMAIN", "example.com")
            .WithEnvironment("LDAP_ADMIN_PASSWORD", adminPassword)
            .WithEnvironment("LDAP_CONFIG_PASSWORD", adminPassword)
            .WithEnvironment("LDAP_TLS", "false")
            .WithPortBinding(LdapPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("slapd starting")
                .UntilExternalTcpPortIsAvailable(LdapPort))
            .Build();

        using var startupTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        await container.StartAsync(startupTimeout.Token);

        connection = new Connection
        {
            Host = container.Hostname,
            Port = container.GetMappedPublicPort(LdapPort),
            User = $"cn=admin,{BaseDn}",
            Password = adminPassword,
        };
        admin = LDAP.CreateConnection(connection, new Options { TimeoutSeconds = 5 });
        admin.Connect(connection.Host, connection.Port);
        admin.Bind("cn=admin,cn=config", adminPassword);

        // Model AD-specific object classes in the disposable OpenLDAP server.
        admin.Add(new LdapEntry("cn=moveuser-tests,cn=schema,cn=config", new LdapAttributeSet
        {
            new LdapAttribute("objectClass", "olcSchemaConfig"),
            new LdapAttribute("cn", "moveuser-tests"),
            new LdapAttribute("olcObjectClasses", new[]
            {
                "( 1.3.6.1.4.1.4203.666.11.999.1 NAME 'user' SUP person STRUCTURAL )",
                "( 1.3.6.1.4.1.4203.666.11.999.2 NAME 'computer' SUP user STRUCTURAL )",
            }),
        }));

        admin.Bind(connection.User, connection.Password);
    }

    [SetUp]
    public void CreateTestEntries()
    {
        var rootName = $"MoveUser-{Guid.NewGuid():N}";
        rootDn = $"OU={rootName},{BaseDn}";
        AddContainer(rootDn, rootName);
        var sourceContainer = $"OU=Source,{rootDn}";
        destinationContainer = $"OU=Destination,{rootDn}";
        AddContainer(sourceContainer, "Source");
        AddContainer(destinationContainer, "Destination");
        input = new Input
        {
            SourceDistinguishedName = $@"CN=Move\, User,{sourceContainer}",
            DestinationDistinguishedName = $@"CN=Move\, User,{destinationContainer}",
        };
        AddUser(input.SourceDistinguishedName);
        cleanupDns.Add(input.DestinationDistinguishedName);
    }

    [TearDown]
    public void DeleteTestEntries()
    {
        foreach (var dn in cleanupDns.AsEnumerable().Reverse().Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                admin.Delete(dn);
            }
            catch (LdapException ex) when (ex.ResultCode == LdapException.NoSuchObject)
            {
                TestContext.Progress.WriteLine($"Cleanup: test entry is already absent: {dn}");
            }
        }

        cleanupDns.Clear();
    }

    [OneTimeTearDown]
    public async Task StopLdapContainer()
    {
        try
        {
            admin?.Dispose();
        }
        finally
        {
            if (container != null)
                await container.DisposeAsync();
        }
    }

    [TestCase("Move, User")]
    [TestCase("Moved User")]
    public void MovesUserAndPreservesIdentityAndOtherAttributes(string destinationName)
    {
        input.DestinationDistinguishedName = $"{LdapDn.EscapeRdn($"CN={destinationName}")},{destinationContainer}";
        cleanupDns.Add(input.DestinationDistinguishedName);
        var before = admin.Read(input.SourceDistinguishedName, new[] { "entryUUID", "objectGUID" });
        var identifier = before.GetAttribute("entryUUID") ?? before.GetAttribute("objectGUID");
        Assert.That(identifier, Is.Not.Null, "The test server must expose entryUUID or objectGUID.");

        var result = LDAP.MoveUser(input, connection, new Options(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Error, Is.Null);
        var moved = admin.Read(input.DestinationDistinguishedName, new[] { identifier.Name, "cn", "sn", "description" });
        Assert.That(moved.GetAttribute(identifier.Name).ByteValue, Is.EqualTo(identifier.ByteValue));
        Assert.That(moved.GetAttribute("cn").StringValueArray, Is.EquivalentTo(new[] { destinationName }));
        Assert.That(moved.GetAttribute("sn").StringValue, Is.EqualTo("User"));
        Assert.That(moved.GetAttribute("description").StringValue, Is.EqualTo("MoveUser integration test"));
        var missingSource = Assert.Throws<LdapException>((Action)(() => admin.Read(input.SourceDistinguishedName)));
        Assert.That(missingSource.ResultCode, Is.EqualTo(LdapException.NoSuchObject));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CollisionDoesNotOverwriteOrDeleteEitherEntry(bool throwError)
    {
        AddUser(input.DestinationDistinguishedName);
        var sourceId = admin.Read(input.SourceDistinguishedName, new[] { "entryUUID" }).GetAttribute("entryUUID").StringValue;
        var destinationId = admin.Read(input.DestinationDistinguishedName, new[] { "entryUUID" }).GetAttribute("entryUUID").StringValue;
        var options = new Options { ThrowErrorOnFailure = throwError };

        LdapException exception;
        if (throwError)
        {
            exception = Assert.Throws<LdapException>((Action)(() =>
                LDAP.MoveUser(input, connection, options, CancellationToken.None)));
        }
        else
        {
            var result = LDAP.MoveUser(input, connection, options, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.AdditionalInfo, Is.TypeOf<LdapException>());
            exception = (LdapException)result.Error.AdditionalInfo;
        }

        Assert.That(exception.ResultCode, Is.EqualTo(LdapException.EntryAlreadyExists));
        Assert.That(admin.Read(input.SourceDistinguishedName, new[] { "entryUUID" }).GetAttribute("entryUUID").StringValue, Is.EqualTo(sourceId));
        Assert.That(admin.Read(input.DestinationDistinguishedName, new[] { "entryUUID" }).GetAttribute("entryUUID").StringValue, Is.EqualTo(destinationId));
    }

    [TestCase("person")]
    [TestCase("organizationalPerson")]
    [TestCase("inetOrgPerson")]
    [TestCase("USER")]
    public void AcceptsUserObjectClasses(string objectClass)
    {
        admin.Delete(input.SourceDistinguishedName);
        AddUser(input.SourceDistinguishedName, objectClass);
        var identifier = admin.Read(input.SourceDistinguishedName, new[] { "entryUUID" }).GetAttribute("entryUUID").StringValue;

        var result = LDAP.MoveUser(input, connection, new Options(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Error, Is.Null);
        Assert.That(admin.Read(input.DestinationDistinguishedName, new[] { "entryUUID" }).GetAttribute("entryUUID").StringValue, Is.EqualTo(identifier));
        var missingSource = Assert.Throws<LdapException>((Action)(() => admin.Read(input.SourceDistinguishedName)));
        Assert.That(missingSource.ResultCode, Is.EqualTo(LdapException.NoSuchObject));
    }

    [TestCase("groupOfNames", "CN")]
    [TestCase("organizationalUnit", "OU")]
    [TestCase("computer", "CN")]
    public void RejectsNonUserEntries(string objectClass, string rdnAttribute)
    {
        var objectClasses = objectClass == "computer"
            ? new[] { "top", "person", "user", "computer" }
            : new[] { "top", objectClass };
        var attributes = new LdapAttributeSet
        {
            new LdapAttribute("objectClass", objectClasses),
            new LdapAttribute(rdnAttribute, "Not a user"),
        };
        if (objectClass == "groupOfNames")
            attributes.Add(new LdapAttribute("member", input.SourceDistinguishedName));
        if (objectClass == "computer")
            attributes.Add(new LdapAttribute("sn", "User"));

        input.SourceDistinguishedName = $"{rdnAttribute}=Not a user,{rootDn}";
        input.DestinationDistinguishedName = $"{rdnAttribute}=Not a user,{destinationContainer}";
        admin.Add(new LdapEntry(input.SourceDistinguishedName, attributes));
        cleanupDns.Add(input.SourceDistinguishedName);
        cleanupDns.Add(input.DestinationDistinguishedName);

        var result = LDAP.MoveUser(input, connection, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<InvalidOperationException>());
        Assert.That(result.Error.Message, Does.Contain("does not identify a user"));
        Assert.That(admin.Read(input.SourceDistinguishedName), Is.Not.Null);
        var missingDestination = Assert.Throws<LdapException>((Action)(() => admin.Read(input.DestinationDistinguishedName)));
        Assert.That(missingDestination.ResultCode, Is.EqualTo(LdapException.NoSuchObject));
    }

    [TestCase(false, LdapException.InvalidCredentials)]
    [TestCase(true, LdapException.ProtocolError)]
    public void BindOrStartTlsFailureLeavesSourceUnchanged(bool tls, int expectedCode)
    {
        var invalidConnection = new Connection
        {
            Host = connection.Host,
            Port = connection.Port,
            User = connection.User,
            Password = "invalid-test-password",
            TLS = tls,
        };

        var result = LDAP.MoveUser(input, invalidConnection, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<LdapException>());
        Assert.That(((LdapException)result.Error.AdditionalInfo).ResultCode, Is.EqualTo(expectedCode));
        Assert.That(admin.Read(input.SourceDistinguishedName), Is.Not.Null);
        var missingDestination = Assert.Throws<LdapException>((Action)(() => admin.Read(input.DestinationDistinguishedName)));
        Assert.That(missingDestination.ResultCode, Is.EqualTo(LdapException.NoSuchObject));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CancellationLeavesSourceUnchanged(bool throwError)
    {
        var cancellationToken = new CancellationToken(true);
        var options = new Options { ThrowErrorOnFailure = throwError };

        var exception = Assert.Throws<OperationCanceledException>((Action)(() =>
            LDAP.MoveUser(input, connection, options, cancellationToken)));

        Assert.That(exception.CancellationToken, Is.EqualTo(cancellationToken));
        Assert.That(admin.Read(input.SourceDistinguishedName), Is.Not.Null);
        var missingDestination = Assert.Throws<LdapException>((Action)(() => admin.Read(input.DestinationDistinguishedName)));
        Assert.That(missingDestination.ResultCode, Is.EqualTo(LdapException.NoSuchObject));
    }

    [Test]
    public void MissingDestinationContainerLeavesSourceUnchanged()
    {
        input.DestinationDistinguishedName = $@"CN=Move\, User,OU=Missing,{rootDn}";

        var result = LDAP.MoveUser(input, connection, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(admin.Read(input.SourceDistinguishedName), Is.Not.Null);
    }

    [Test]
    public void MissingUserReturnsLdapError()
    {
        admin.Delete(input.SourceDistinguishedName);

        var result = LDAP.MoveUser(input, connection, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(((LdapException)result.Error.AdditionalInfo).ResultCode, Is.EqualTo(LdapException.NoSuchObject));
    }

    [Test]
    public void CrossDomainMoveLeavesSourceUnchanged()
    {
        input.DestinationDistinguishedName = "CN=Move User,OU=Users,DC=other,DC=invalid";

        var result = LDAP.MoveUser(input, connection, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<ArgumentException>());
        Assert.That(admin.Read(input.SourceDistinguishedName), Is.Not.Null);
    }

    private void AddContainer(string dn, string name)
    {
        admin.Add(new LdapEntry(dn, new LdapAttributeSet
        {
            new LdapAttribute("objectClass", new[] { "top", "organizationalUnit" }),
            new LdapAttribute("ou", name),
        }));
        cleanupDns.Add(dn);
    }

    private void AddUser(string dn, string objectClass = "inetOrgPerson")
    {
        admin.Add(new LdapEntry(dn, new LdapAttributeSet
        {
            new LdapAttribute("objectClass", new[] { "top", objectClass }),
            new LdapAttribute("cn", "Move, User"),
            new LdapAttribute("sn", "User"),
            new LdapAttribute("description", "MoveUser integration test"),
        }));
        cleanupDns.Add(dn);
    }
}
