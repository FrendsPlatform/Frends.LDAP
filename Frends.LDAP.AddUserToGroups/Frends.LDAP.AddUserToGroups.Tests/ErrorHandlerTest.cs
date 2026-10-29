using System;
using System.Threading;
using Frends.LDAP.AddUserToGroups.Definitions;
using NUnit.Framework;

namespace Frends.LDAP.AddUserToGroups.Tests;

[TestFixture]
internal class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.Catch<Exception>(() =>
            LDAP.AddUserToGroups(InvalidInput(), DefaultConnection(), new Options(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var result = LDAP.AddUserToGroups(InvalidInput(), DefaultConnection(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("GroupDistinguishedNames is required"));
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = new Options { ErrorMessageOnFailure = CustomErrorMessage };
        var ex = Assert.Throws<Exception>(() =>
            LDAP.AddUserToGroups(InvalidInput(), DefaultConnection(), options, CancellationToken.None));
        Assert.That(ex.Message, Does.Contain(CustomErrorMessage));
    }

    private static Input InvalidInput() => new()
    {
        UserDistinguishedName = "CN=Test User,ou=users,dc=wimpi,dc=net",
        GroupDistinguishedNames = Array.Empty<string>(),
    };

    private static Connection DefaultConnection() => new()
    {
        Host = "127.0.0.1",
        User = "uid=admin,ou=system",
        Password = "secret",
        Port = 10389,
    };
}
