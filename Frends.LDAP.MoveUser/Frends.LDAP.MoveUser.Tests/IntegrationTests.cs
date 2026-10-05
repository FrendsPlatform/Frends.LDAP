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

    private readonly List<string> cleanupDns = new();
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

    [Test]
    public void CollisionDoesNotOverwriteOrDeleteEitherEntry()
    {
        AddUser(input.DestinationDistinguishedName);

        var result = LDAP.MoveUser(input, connection, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(((LdapException)result.Error.AdditionalInfo).ResultCode, Is.EqualTo(LdapException.EntryAlreadyExists));
        Assert.That(admin.Read(input.SourceDistinguishedName), Is.Not.Null);
        Assert.That(admin.Read(input.DestinationDistinguishedName), Is.Not.Null);
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

    private void AddUser(string dn)
    {
        admin.Add(new LdapEntry(dn, new LdapAttributeSet
        {
            new LdapAttribute("objectClass", new[] { "top", "person", "organizationalPerson", "inetOrgPerson" }),
            new LdapAttribute("cn", "Move, User"),
            new LdapAttribute("sn", "User"),
            new LdapAttribute("description", "MoveUser integration test"),
        }));
        cleanupDns.Add(dn);
    }
}
