using System;
using System.Threading;
using Frends.LDAP.MoveUser.Definitions;
using Novell.Directory.Ldap;
using NSubstitute;
using NUnit.Framework;

namespace Frends.LDAP.MoveUser.Tests;

[TestFixture]
internal class FunctionalTests : TestBase
{
    private ILdapConnection client;

    [SetUp]
    public void SetUp()
    {
        client = Substitute.For<ILdapConnection>();
        client.Read(Arg.Any<string>(), Arg.Any<string[]>()).Returns(UserEntry("person"));
    }

    [Test]
    public void MovesExistingUserWithoutRecreatingIt()
    {
        var input = DefaultInput();
        var result = Run(input);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Error, Is.Null);
        client.Received(1).Rename(input.SourceDistinguishedName, "CN=Jane Doe", "OU=Support,DC=example,DC=com", true);
        client.DidNotReceiveWithAnyArgs().Add(null);
        client.DidNotReceiveWithAnyArgs().Delete(null);
        client.Received(1).Dispose();
    }

    [TestCase(false, false, 0, 389)]
    [TestCase(true, false, 0, 636)]
    [TestCase(false, true, 0, 389)]
    [TestCase(false, false, 10389, 10389)]
    [TestCase(true, false, 10636, 10636)]
    public void UsesConfiguredPort(bool ssl, bool tls, int port, int expectedPort)
    {
        var connection = DefaultConnection();
        connection.SecureSocketLayer = ssl;
        connection.TLS = tls;
        connection.Port = port;

        Assert.That(Run(connection: connection).Success, Is.True);
        client.Received(1).Connect(connection.Host, expectedPort);
    }

    [Test]
    public void StartsTlsBeforeSendingCredentialsAndDisposesWithoutReconnecting()
    {
        var connection = DefaultConnection();
        connection.TLS = true;

        Run(connection: connection);

        Received.InOrder(() =>
        {
            client.Connect(connection.Host, 389);
            client.StartTls();
            client.Bind(connection.User, connection.Password);
            client.Read(Arg.Any<string>(), Arg.Any<string[]>());
            client.Rename(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), true);
            client.Dispose();
        });
        client.DidNotReceive().StopTls();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ConfiguresTimeoutsAndDisablesReferrals(bool ssl)
    {
        var connection = DefaultConnection();
        connection.SecureSocketLayer = ssl;
        var options = new Options { TimeoutSeconds = 17 };

        using var configuredClient = LDAP.CreateConnection(connection, options);

        Assert.That(configuredClient.SecureSocketLayer, Is.EqualTo(ssl));
        Assert.That(configuredClient.ConnectionTimeout, Is.EqualTo(17000));
        Assert.That(configuredClient.Constraints.TimeLimit, Is.EqualTo(17000));
        Assert.That(configuredClient.SearchConstraints.TimeLimit, Is.EqualTo(17000));
        Assert.That(configuredClient.Constraints.ReferralFollowing, Is.False);
        Assert.That(configuredClient.SearchConstraints.ReferralFollowing, Is.False);
    }

    [TestCase("CN=Changed Name", "OU=Support,DC=example,DC=com")]
    [TestCase(@"CN=Doe\, Jane", "OU=Support,DC=example,DC=com")]
    [TestCase(@"CN=Doe\2C Jane", "OU=Support,DC=example,DC=com")]
    [TestCase("CN=\"Doe, Jane\"", "OU=Support,DC=example,DC=com")]
    [TestCase(@"CN=Jane\\Doe", "OU=Support,DC=example,DC=com")]
    [TestCase(@"CN=Jane\+Doe+UID=42", @"OU=Support\, East,DC=example,DC=com")]
    [TestCase(@"CN=Jane\ ", "OU=Support,DC=example,DC=com")]
    public void PreservesEscapedAndMultivaluedDistinguishedNames(string newRdn, string newParent)
    {
        var input = DefaultInput();
        input.DestinationDistinguishedName = $"{newRdn},{newParent}";

        Assert.That(Run(input).Success, Is.True);
        client.Received(1).Rename(input.SourceDistinguishedName, newRdn, newParent, true);
    }

    [TestCase("DC=EXAMPLE,DC=COM")]
    [TestCase(@"DC=\65xample,DC=com")]
    public void ComparesParsedDomainComponents(string domain)
    {
        var input = DefaultInput();
        input.DestinationDistinguishedName = $"CN=Jane Doe,OU=Support,{domain}";

        Assert.That(Run(input).Success, Is.True);
    }

    [Test]
    public void PreservesLegacyDnSeparators()
    {
        var input = DefaultInput();
        input.DestinationDistinguishedName = "CN=Jane Doe;OU=Support;DC=example;DC=com";

        Assert.That(Run(input).Success, Is.True);
        client.Received(1).Rename(input.SourceDistinguishedName, "CN=Jane Doe", "OU=Support;DC=example;DC=com", true);
    }

    [TestCase("not a DN")]
    [TestCase(@"CN=Invalid\q,OU=Support,DC=example,DC=com")]
    [TestCase("CN=Jane Doe,OU=Support")]
    [TestCase("DC=example,DC=com")]
    [TestCase("CN=Jane Doe,OU=Support,DC=other,DC=com")]
    [TestCase("CN=Jane Doe,OU=Support,DC=child,DC=example,DC=com")]
    [TestCase("CN=Jane Doe,OU=Support,DC=com")]
    [TestCase("CN=Jane Doe,OU=Support,DC=notexample,DC=com")]
    public void RejectsInvalidOrCrossDomainDestinationsBeforeConnecting(string destination)
    {
        var input = DefaultInput();
        input.DestinationDistinguishedName = destination;

        var result = Run(input, options: new Options { ThrowErrorOnFailure = false });

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.InstanceOf<ArgumentException>());
        client.DidNotReceiveWithAnyArgs().Connect(null, 0);
    }

    [TestCase("CN=Jane Doe")]
    [TestCase("DC=example,DC=com")]
    [TestCase("invalid")]
    public void RejectsInvalidSourcesBeforeConnecting(string source)
    {
        var input = DefaultInput();
        input.SourceDistinguishedName = source;

        Assert.That(Run(input, options: new Options { ThrowErrorOnFailure = false }).Success, Is.False);
        client.DidNotReceiveWithAnyArgs().Connect(null, 0);
    }

    [TestCase("person")]
    [TestCase("organizationalPerson")]
    [TestCase("inetOrgPerson")]
    [TestCase("USER")]
    public void AcceptsUserObjectClasses(string objectClass)
    {
        client.Read(Arg.Any<string>(), Arg.Any<string[]>()).Returns(UserEntry(objectClass));

        Assert.That(Run().Success, Is.True);
    }

    [TestCase("group")]
    [TestCase("organizationalUnit")]
    [TestCase("computer")]
    public void RejectsNonUserEntries(string objectClass)
    {
        client.Read(Arg.Any<string>(), Arg.Any<string[]>()).Returns(
            objectClass == "computer" ? UserEntry("person", "user", "computer") : UserEntry(objectClass));

        var result = Run(options: new Options { ThrowErrorOnFailure = false });

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("does not identify a user"));
        client.DidNotReceiveWithAnyArgs().Rename(null, null, null, true);
        client.Received(1).Dispose();
    }

    [Test]
    public void RejectsMissingObjectClasses()
    {
        client.Read(Arg.Any<string>(), Arg.Any<string[]>()).Returns(
            new LdapEntry(DefaultInput().SourceDistinguishedName, new LdapAttributeSet()));

        Assert.That(Run(options: new Options { ThrowErrorOnFailure = false }).Success, Is.False);
        client.DidNotReceiveWithAnyArgs().Rename(null, null, null, true);
    }

    [TestCase("connect", 91)]
    [TestCase("tls", 52)]
    [TestCase("bind", 49)]
    [TestCase("read", 32)]
    [TestCase("rename", 68)]
    [TestCase("rename", 50)]
    [TestCase("rename", 10)]
    public void ReturnsLdapFailuresAndDisposesConnection(string stage, int resultCode)
    {
        var failure = new LdapException("Test LDAP failure", resultCode, string.Empty);
        var connection = DefaultConnection();
        connection.TLS = stage == "tls";
        switch (stage)
        {
            case "connect":
                client.When(value => value.Connect(Arg.Any<string>(), Arg.Any<int>())).Do(_ => throw failure);
                break;
            case "tls":
                client.When(value => value.StartTls()).Do(_ => throw failure);
                break;
            case "bind":
                client.When(value => value.Bind(Arg.Any<string>(), Arg.Any<string>())).Do(_ => throw failure);
                break;
            case "read":
                client.Read(Arg.Any<string>(), Arg.Any<string[]>()).Returns(_ => throw failure);
                break;
            case "rename":
                client.When(value => value.Rename(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), true))
                    .Do(_ => throw failure);
                break;
        }

        var result = Run(connection: connection, options: new Options { ThrowErrorOnFailure = false });

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.SameAs(failure));
        client.Received(1).Dispose();
        client.DidNotReceive().StopTls();
        if (stage != "rename")
            client.DidNotReceiveWithAnyArgs().Rename(null, null, null, true);
    }

    [TestCase("connect")]
    [TestCase("tls")]
    [TestCase("bind")]
    [TestCase("read")]
    public void ObservesCancellationBeforeTheNextOperation(string stage)
    {
        var cancellation = new CancellationTokenSource();
        var connection = DefaultConnection();
        connection.TLS = stage == "tls";
        switch (stage)
        {
            case "connect":
                client.When(value => value.Connect(Arg.Any<string>(), Arg.Any<int>())).Do(_ => cancellation.Cancel());
                break;
            case "tls":
                client.When(value => value.StartTls()).Do(_ => cancellation.Cancel());
                break;
            case "bind":
                client.When(value => value.Bind(Arg.Any<string>(), Arg.Any<string>())).Do(_ => cancellation.Cancel());
                break;
            case "read":
                client.Read(Arg.Any<string>(), Arg.Any<string[]>()).Returns(_ =>
                {
                    cancellation.Cancel();
                    return UserEntry("person");
                });
                break;
        }

        Assert.Throws<OperationCanceledException>((Action)(() => Run(
            connection: connection,
            options: new Options { ThrowErrorOnFailure = false },
            cancellationToken: cancellation.Token)));
        client.DidNotReceiveWithAnyArgs().Rename(null, null, null, true);
        client.Received(1).Dispose();
    }

    [Test]
    public void DoesNotHideThrownLdapErrors()
    {
        var failure = new LdapException("Destination already exists", 68, string.Empty);
        client.When(value => value.Rename(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), true))
            .Do(_ => throw failure);

        Assert.That(Assert.Throws<LdapException>((Action)(() => Run())), Is.SameAs(failure));
        client.Received(1).Dispose();
    }

    private Result Run(
        Input input = null,
        Connection connection = null,
        Options options = null,
        CancellationToken cancellationToken = default) =>
        LDAP.MoveUser(
            input ?? DefaultInput(),
            connection ?? DefaultConnection(),
            options ?? DefaultOptions(),
            cancellationToken);
}
