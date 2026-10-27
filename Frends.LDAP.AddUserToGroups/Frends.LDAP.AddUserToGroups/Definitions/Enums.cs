namespace Frends.LDAP.AddUserToGroups.Definitions;

/// <summary>
/// Options if user is already in target group.
/// </summary>
public enum UserExistsAction
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