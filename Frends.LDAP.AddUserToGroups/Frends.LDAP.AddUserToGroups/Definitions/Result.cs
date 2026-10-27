namespace Frends.LDAP.AddUserToGroups.Definitions;

/// <summary>
/// Task's result.
/// </summary>
public class Result
{
    /// <summary>
    /// Update completed.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }

    /// <summary>
    /// Operation details. Contains information about added and skipped groups when applicable.
    /// </summary>
    /// <example>Added to 2 group(s): cn=admin,ou=roles,dc=wimpi,dc=net, cn=managers,ou=roles,dc=wimpi,dc=net. Skipped 1 group(s): cn=developers,ou=roles,dc=wimpi,dc=net: User already exists in the group</example>
    public string Details { get; set; }

    /// <summary>
    /// User DN.
    /// </summary>
    /// <example>CN=Tes Tuser,ou=users,dc=wimpi,dc=net</example>
    public string UserDistinguishedName { get; set; }

    /// <summary>
    /// Group DN(s).
    /// </summary>
    /// <example>new[] { "cn=admin,ou=roles,dc=wimpi,dc=net" }</example>
    public string[] GroupDistinguishedName { get; set; }
}
