using Frends.LDAP.CreateUser.Definitions;
using NUnit.Framework;

namespace Frends.LDAP.CreateUser.Tests;

[TestFixture]
internal class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.Catch<Exception>(() =>
            LDAP.CreateUser(DefaultInput(), DefaultConnection(), DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = LDAP.CreateUser(DefaultInput(), DefaultConnection(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("Connection parameters missing"));
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var ex = Assert.Throws<Exception>(() =>
            LDAP.CreateUser(DefaultInput(), DefaultConnection(), options, CancellationToken.None));
        Assert.That(ex!.Message, Contains.Substring(CustomErrorMessage));
    }

    private static Input DefaultInput() => new()
    {
        Path = "ou=users,dc=wimpi,dc=net",
        CommonName = "Test User",
        ObjectClass = "inetOrgPerson",
    };

    // Empty host makes the connection parameters invalid.
    private static Connection DefaultConnection() => new()
    {
        Host = string.Empty,
        User = "uid=admin,ou=system",
        Password = "secret",
    };

    private static Options DefaultOptions() => new();
}
