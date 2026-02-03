using System.ComponentModel;

namespace Frends.LDAP.RemoveUserFromGroups.Definitions;

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
    /// Group's distinguished name (DN)
    /// </summary>
    /// <example>cn=admin,ou=roles,dc=wimpi,dc=net</example>
    public string GroupDistinguishedName { get; set; }

    /// <summary>
    /// How to handle LDAP errors.
    /// </summary>
    /// <example>HandleLDAPError.Throw</example>
    [DefaultValue(HandleLDAPError.Throw)]
    public HandleLDAPError HandleLDAPError { get; set; }
}