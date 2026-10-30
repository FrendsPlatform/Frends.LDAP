using System;
using System.Threading;
using Frends.LDAP.DeleteUser.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.LDAP.DeleteUser.Tests.HelpersTests;

[TestClass]
public class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [TestMethod]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            LDAP.DeleteUser(DefaultInput(), InvalidConnection(), new Options(), CancellationToken.None));
    }

    [TestMethod]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var result = LDAP.DeleteUser(DefaultInput(), InvalidConnection(), options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.AreEqual("Firstname Lastname", result.CommonName);
    }

    [TestMethod]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = new Options { ErrorMessageOnFailure = CustomErrorMessage };
        var ex = Assert.ThrowsException<Exception>(() =>
            LDAP.DeleteUser(DefaultInput(), InvalidConnection(), options, CancellationToken.None));
        StringAssert.Contains(ex.Message, CustomErrorMessage);
    }

    private static Input DefaultInput() => new() { CommonName = "Firstname Lastname", Path = "CN=Users,DC=Example,DC=Com" };

    private static Connection InvalidConnection() => new() { Host = string.Empty, User = "admin", Password = "secret" };
}
