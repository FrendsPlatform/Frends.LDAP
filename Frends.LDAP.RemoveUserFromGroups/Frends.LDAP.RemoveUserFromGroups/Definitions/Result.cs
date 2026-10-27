namespace Frends.LDAP.RemoveUserFromGroups.Definitions;

/// <summary>
/// Task's result.
/// </summary>
public class Result
{
    internal Result(bool success, string error, string userDistinguishedName, string groupDistinguishedName)
    {
        Success = success;
        Error = error;
        UserDistinguishedName = userDistinguishedName;
        GroupDistinguishedName = groupDistinguishedName;
    }

    /// <summary>
    /// User has been removed from group(s).
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// LDAP Error message.
    /// </summary>
    /// <example>No Such Attribute</example>
    public string Error { get; private set; }

    /// <summary>
    /// User DN.
    /// </summary>
    /// <example>CN=Tes Tuser,ou=users,dc=wimpi,dc=net</example>
    public string UserDistinguishedName { get; private set; }

    /// <summary>
    /// Group DN.
    /// </summary>
    /// <example>cn=admin,ou=roles,dc=wimpi,dc=net</example>
    public string GroupDistinguishedName { get; private set; }
}