using Frends.LDAP.MoveUser.Definitions;
using Novell.Directory.Ldap;

namespace Frends.LDAP.MoveUser.Tests;

internal abstract class TestBase
{
    protected static Input DefaultInput() => new()
    {
        SourceDistinguishedName = "CN=Jane Doe,OU=Sales,DC=example,DC=com",
        DestinationDistinguishedName = "CN=Jane Doe,OU=Support,DC=example,DC=com",
    };

    protected static Connection DefaultConnection() => new()
    {
        Host = "dc1.example.com",
        User = "CN=Service Account,DC=example,DC=com",
        Password = "unused-test-password",
    };

    protected static Options DefaultOptions() => new();

    protected static LdapEntry UserEntry(params string[] objectClasses) => new(
        DefaultInput().SourceDistinguishedName,
        new LdapAttributeSet { new LdapAttribute("objectClass", objectClasses) });
}
