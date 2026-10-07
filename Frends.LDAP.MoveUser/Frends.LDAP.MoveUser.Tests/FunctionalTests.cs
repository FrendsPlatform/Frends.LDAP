using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Frends.LDAP.MoveUser.Definitions;
using Frends.LDAP.MoveUser.Helpers;
using Novell.Directory.Ldap;
using NUnit.Framework;

namespace Frends.LDAP.MoveUser.Tests;

[TestFixture]
internal class FunctionalTests : TestBase
{
    [TestCase(false, false, 0)]
    [TestCase(true, false, 0)]
    [TestCase(false, true, 0)]
    [TestCase(false, false, 10389)]
    [TestCase(true, false, 10636)]
    public void AcceptsConnectionPortAndEncryptionSettings(bool ssl, bool tls, int port)
    {
        var connection = DefaultConnection();
        connection.SecureSocketLayer = ssl;
        connection.TLS = tls;
        connection.Port = port;

        Assert.DoesNotThrow((Action)(() => ValidationHandler.Run(connection)));
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

        Assert.That(DistinguishedNameHelper.GetMoveTarget(input), Is.EqualTo((newRdn, newParent)));
    }

    [TestCase("DC=EXAMPLE,DC=COM")]
    [TestCase(@"DC=\65xample,DC=com")]
    public void ComparesParsedDomainComponents(string domain)
    {
        var input = DefaultInput();
        input.DestinationDistinguishedName = $"CN=Jane Doe,OU=Support,{domain}";

        Assert.That(
            DistinguishedNameHelper.GetMoveTarget(input),
            Is.EqualTo(("CN=Jane Doe", $"OU=Support,{domain}")));
    }

    [Test]
    public void PreservesLegacyDnSeparators()
    {
        var input = DefaultInput();
        input.DestinationDistinguishedName = "CN=Jane Doe;OU=Support;DC=example;DC=com";

        Assert.That(
            DistinguishedNameHelper.GetMoveTarget(input),
            Is.EqualTo(("CN=Jane Doe", "OU=Support;DC=example;DC=com")));
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

        // Validating an invalid DN must fail before cancellation can reach the connection path.
        var result = LDAP.MoveUser(
            input,
            DefaultConnection(),
            new Options { ThrowErrorOnFailure = false },
            new CancellationToken(true));

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.InstanceOf<ArgumentException>());
    }

    [TestCase("CN=Jane Doe")]
    [TestCase("DC=example,DC=com")]
    [TestCase("invalid")]
    public void RejectsInvalidSourcesBeforeConnecting(string source)
    {
        var input = DefaultInput();
        input.SourceDistinguishedName = source;

        var result = LDAP.MoveUser(
            input,
            DefaultConnection(),
            new Options { ThrowErrorOnFailure = false },
            new CancellationToken(true));

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.InstanceOf<ArgumentException>());
    }

    [Test]
    public void ReturnsConnectionFailure()
    {
        // Reserve a local port without listening so no LDAP server can accept the connection.
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.ExclusiveAddressUse = true;
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        var connection = DefaultConnection();
        connection.Host = IPAddress.Loopback.ToString();
        connection.Port = ((IPEndPoint)socket.LocalEndPoint).Port;

        var result = LDAP.MoveUser(
            DefaultInput(),
            connection,
            new Options { ThrowErrorOnFailure = false, TimeoutSeconds = 1 },
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<LdapException>());
        Assert.That(((LdapException)result.Error.AdditionalInfo).ResultCode, Is.EqualTo(LdapException.ConnectError));
    }
}
