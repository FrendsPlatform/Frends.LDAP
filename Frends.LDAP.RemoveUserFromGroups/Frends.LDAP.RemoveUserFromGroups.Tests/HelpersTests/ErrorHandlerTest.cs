using Frends.LDAP.RemoveUserFromGroups.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.LDAP.RemoveUserFromGroups.Tests.HelpersTests;

[TestClass]
public class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [TestMethod]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            LDAP.RemoveUserFromGroups(DefaultInput(), DefaultConnection(), new Options(), CancellationToken.None));
        Assert.IsNotNull(ex);
    }

    [TestMethod]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var result = LDAP.RemoveUserFromGroups(DefaultInput(), DefaultConnection(), options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.AreEqual(DefaultInput().UserDistinguishedName, result.UserDistinguishedName);
    }

    [TestMethod]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = new Options { ErrorMessageOnFailure = CustomErrorMessage };
        var ex = Assert.ThrowsException<Exception>(() =>
            LDAP.RemoveUserFromGroups(DefaultInput(), DefaultConnection(), options, CancellationToken.None));
        StringAssert.Contains(ex.Message, CustomErrorMessage);
    }

    private static Input DefaultInput() => new()
    {
        UserDistinguishedName = "CN=Tes Tuser,ou=users,dc=wimpi,dc=net",
        GroupDistinguishedName = "cn=admin,ou=roles,dc=wimpi,dc=net",
    };

    private static Connection DefaultConnection() => new() { Host = string.Empty, User = string.Empty, Password = string.Empty };
}
