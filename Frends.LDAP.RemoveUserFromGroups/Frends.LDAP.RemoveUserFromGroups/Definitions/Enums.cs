namespace Frends.LDAP.RemoveUserFromGroups.Definitions;

/// <summary>
/// How to handle LDAP errors.
/// </summary>
public enum HandleLDAPError
{
    /// <summary>
    /// Throw an error.
    /// </summary>
    Throw,

    /// <summary>
    /// Do nothing and add LDAP error message to the task's result.
    /// </summary>
    Skip,
}