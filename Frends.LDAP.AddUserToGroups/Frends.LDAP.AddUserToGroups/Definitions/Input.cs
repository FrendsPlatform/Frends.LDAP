using System.ComponentModel;

namespace Frends.LDAP.AddUserToGroups.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// User's distinguished name (DN)
    /// </summary>
    /// <example>CN=Tes Tuser,ou=users,dc=wimpi,dc=net</example>
    public string UserDistinguishedName { get; set; }

    /// <summary>
    /// Group distinguished name(s) (DN) to add the user to. Can specify one or multiple groups.
    /// </summary>
    /// <example>new[] { "cn=admin,ou=roles,dc=wimpi,dc=net", "cn=developers,ou=roles,dc=wimpi,dc=net" }</example>
    public string[] GroupDistinguishedNames { get; set; }

    /// <summary>
    /// Handle user exists exception.
    /// </summary>
    /// <example>UserExistsAction.Throw</example>
    [DefaultValue(UserExistsAction.Throw)]
    public UserExistsAction UserExistsAction { get; set; }
}