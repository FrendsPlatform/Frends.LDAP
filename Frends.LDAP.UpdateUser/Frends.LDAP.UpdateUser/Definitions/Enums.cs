namespace Frends.LDAP.UpdateUser.Definitions;

/// <summary>
/// Modification methods.
/// </summary>
public enum ModificationMethod
{
    /// <summary>
    /// Adds the listed values to the given attribute, creating the attribute if it does not already exist.
    /// </summary>
    Add,

    /// <summary>
    /// Deletes the listed values from the given attribute, removing the entire attribute (1) if no values are listed or (2) if all current values of the attribute are listed for deletion.
    /// </summary>
    Delete,

    /// <summary>
    /// Replaces all existing values of the given attribute with the new values listed, creating the attribute if it does not already exist. A replace with no value deletes the entire attribute if it exists, and is ignored if the attribute does not exist.
    /// </summary>
    Replace,
}