namespace Frends.LDAP.RemoveUserFromGroups.Definitions;

/// <summary>
/// Task's result.
/// </summary>
public class Result
{
    /// <summary>
    /// User has been removed from group(s).
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// User DN.
    /// </summary>
    /// <example>CN=Tes Tuser,ou=users,dc=wimpi,dc=net</example>
    public string UserDistinguishedName { get; set; }

    /// <summary>
    /// Group DN.
    /// </summary>
    /// <example>cn=admin,ou=roles,dc=wimpi,dc=net</example>
    public string GroupDistinguishedName { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
