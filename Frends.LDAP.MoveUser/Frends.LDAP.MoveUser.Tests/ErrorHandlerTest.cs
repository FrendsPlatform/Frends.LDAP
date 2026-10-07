using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Frends.LDAP.MoveUser.Definitions;
using Frends.LDAP.MoveUser.Helpers;
using Novell.Directory.Ldap;
using NUnit.Framework;

namespace Frends.LDAP.MoveUser.Tests;

[TestFixture]
internal class ErrorHandlerTest : TestBase
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void ThrowsOriginalValidationErrorByDefault()
    {
        Assert.Throws<ValidationException>((Action)(() =>
            LDAP.MoveUser(new Input(), DefaultConnection(), DefaultOptions(), CancellationToken.None)));
    }

    [Test]
    public void ReturnsStructuredFailureWhenThrowingIsDisabled()
    {
        var result = LDAP.MoveUser(new Input(), DefaultConnection(), new Options { ThrowErrorOnFailure = false }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain(nameof(Input.SourceDistinguishedName)));
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<ValidationException>());
    }

    [Test]
    public void PreservesCauseWithCustomThrownMessage()
    {
        var options = new Options { ErrorMessageOnFailure = CustomErrorMessage };

        var exception = Assert.Throws<Exception>((Action)(() =>
            LDAP.MoveUser(new Input(), DefaultConnection(), options, CancellationToken.None)));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo(CustomErrorMessage));
        Assert.That(exception.InnerException, Is.TypeOf<ValidationException>());
    }

    [Test]
    public void PrefixesReturnedErrorWithCustomMessage()
    {
        var options = new Options { ThrowErrorOnFailure = false, ErrorMessageOnFailure = CustomErrorMessage };

        var result = LDAP.MoveUser(new Input(), DefaultConnection(), options, CancellationToken.None);

        Assert.That(result.Error.Message, Does.StartWith($"{CustomErrorMessage}: "));
        Assert.That(result.Error.Message, Does.Contain(nameof(Input.SourceDistinguishedName)));
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<ValidationException>());
    }

    [TestCase(true)]
    [TestCase(false)]
    public void NeverConvertsCancellationIntoFailure(bool throwError)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var options = new Options { ThrowErrorOnFailure = throwError, ErrorMessageOnFailure = CustomErrorMessage };

        var exception = Assert.Throws<OperationCanceledException>((Action)(() =>
            LDAP.MoveUser(DefaultInput(), DefaultConnection(), options, cancellation.Token)));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.CancellationToken, Is.EqualTo(cancellation.Token));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public void RequiresBothDistinguishedNames(string value)
    {
        var inputSourceOnly = DefaultInput();
        inputSourceOnly.SourceDistinguishedName = value;
        Assert.Throws<ValidationException>((Action)(() =>
            LDAP.MoveUser(inputSourceOnly, DefaultConnection(), DefaultOptions(), CancellationToken.None)));

        var inputDestinationOnly = DefaultInput();
        inputDestinationOnly.DestinationDistinguishedName = value;
        Assert.Throws<ValidationException>((Action)(() =>
            LDAP.MoveUser(inputDestinationOnly, DefaultConnection(), DefaultOptions(), CancellationToken.None)));
    }

    [TestCase("Host")]
    [TestCase("User")]
    [TestCase("Password")]
    public void RequiresConnectionParameters(string property)
    {
        var connection = DefaultConnection();
        switch (property)
        {
            case "Host":
                connection.Host = string.Empty;
                break;
            case "User":
                connection.User = string.Empty;
                break;
            case "Password":
                connection.Password = string.Empty;
                break;
        }

        Assert.Throws<ValidationException>((Action)(() =>
            LDAP.MoveUser(DefaultInput(), connection, DefaultOptions(), CancellationToken.None)));
    }

    [TestCase("ldap://dc1.example.com")]
    [TestCase("dc1.example.com dc2.example.com")]
    [TestCase("dc1.example.com:389")]
    public void RejectsUrlsHostListsAndEmbeddedPorts(string host)
    {
        var connection = DefaultConnection();
        connection.Host = host;

        var exception = Assert.Throws<ValidationException>((Action)(() =>
            LDAP.MoveUser(DefaultInput(), connection, DefaultOptions(), CancellationToken.None)));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("Host must be a single LDAP server hostname or IP address, without a scheme or port."));
    }

    [TestCase(-1)]
    [TestCase(65536)]
    public void RejectsInvalidPorts(int port)
    {
        var connection = DefaultConnection();
        connection.Port = port;

        Assert.Throws<ValidationException>((Action)(() =>
            LDAP.MoveUser(DefaultInput(), connection, DefaultOptions(), CancellationToken.None)));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(2147484)]
    public void RejectsInvalidTimeouts(int timeout)
    {
        Assert.Throws<ValidationException>((Action)(() =>
            LDAP.MoveUser(DefaultInput(), DefaultConnection(), new Options { TimeoutSeconds = timeout }, CancellationToken.None)));
    }

    [Test]
    public void RejectsConflictingEncryptionSettings()
    {
        var connection = DefaultConnection();
        connection.SecureSocketLayer = true;
        connection.TLS = true;

        var exception = Assert.Throws<ValidationException>((Action)(() =>
            LDAP.MoveUser(DefaultInput(), connection, DefaultOptions(), CancellationToken.None)));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("SecureSocketLayer and TLS cannot both be enabled."));
    }

    [Test]
    public void HandlesNullInputsAndConnections()
    {
        var options = new Options { ThrowErrorOnFailure = false };

        var nullInput = LDAP.MoveUser(null, DefaultConnection(), options, CancellationToken.None);
        var nullConnection = LDAP.MoveUser(DefaultInput(), null, options, CancellationToken.None);

        Assert.That(nullInput.Success, Is.False);
        Assert.That(nullInput.Error.AdditionalInfo, Is.TypeOf<ValidationException>());
        Assert.That(nullConnection.Success, Is.False);
        Assert.That(nullConnection.Error.AdditionalInfo, Is.TypeOf<ValidationException>());
    }

    [Test]
    public void ThrowsWhenOptionsAreNull()
    {
        Assert.Throws<NullReferenceException>((Action)(() =>
            LDAP.MoveUser(DefaultInput(), DefaultConnection(), null, CancellationToken.None)));
    }

    [TestCase(LdapException.ConnectError)]
    [TestCase(LdapException.Unavailable)]
    [TestCase(LdapException.InvalidCredentials)]
    [TestCase(LdapException.NoSuchObject)]
    [TestCase(LdapException.EntryAlreadyExists)]
    [TestCase(LdapException.InsufficientAccessRights)]
    [TestCase(LdapException.Referral)]
    public void ReturnsLdapFailuresWithoutLosingCause(int resultCode)
    {
        var failure = new LdapException("Test LDAP failure", resultCode, string.Empty);

        var result = failure.Handle(new Options { ThrowErrorOnFailure = false });

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Is.EqualTo(failure.Message));
        Assert.That(result.Error.AdditionalInfo, Is.SameAs(failure));
    }

    [Test]
    public void DoesNotHideThrownLdapErrors()
    {
        var failure = new LdapException("Destination already exists", LdapException.EntryAlreadyExists, string.Empty);

        var exception = Assert.Throws<LdapException>((Action)(() => failure.Handle(DefaultOptions())));

        Assert.That(exception, Is.SameAs(failure));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RethrowsCancellationUnchanged(bool throwError)
    {
        var cancellation = new OperationCanceledException(new CancellationToken(true));
        var options = new Options { ThrowErrorOnFailure = throwError, ErrorMessageOnFailure = CustomErrorMessage };

        var exception = Assert.Throws<OperationCanceledException>((Action)(() => cancellation.Handle(options)));

        Assert.That(exception, Is.SameAs(cancellation));
    }

    [Test]
    public void ValidationRequiresObjects()
    {
        Assert.Throws<ValidationException>((Action)(() => ValidationHandler.Run()));
        Assert.Throws<ValidationException>((Action)(() => ValidationHandler.Run(null)));
    }
}
