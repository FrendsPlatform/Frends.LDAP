using Frends.LDAP.SearchObjects.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.LDAP.SearchObjects.Tests.HelpersTests;

[TestClass]
public class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [TestMethod]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.ThrowsException<Exception>(() =>
            LDAP.SearchObjects(new Input(), new Connection(), new Options(), CancellationToken.None));
        Assert.AreEqual("Host is missing.", ex.Message);
    }

    [TestMethod]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var result = LDAP.SearchObjects(new Input(), new Connection(), options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Host is missing.", result.Error.Message);
    }

    [TestMethod]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = new Options { ErrorMessageOnFailure = CustomErrorMessage };
        var ex = Assert.ThrowsException<Exception>(() =>
            LDAP.SearchObjects(new Input(), new Connection(), options, CancellationToken.None));
        Assert.AreEqual(CustomErrorMessage, ex.Message);
    }
}
