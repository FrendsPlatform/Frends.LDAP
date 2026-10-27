using System.ComponentModel;

namespace Frends.LDAP.SearchObjects.Definitions;

/// <summary>
/// Attribute values.
/// </summary>
public class Attributes
{
    /// <summary>
    /// Key of the attribute.
    /// </summary>
    /// <example>title</example>
    public string Key { get; set; }

    /// <summary>
    /// Determines the return type of the attribute.
    /// </summary>
    /// <example>ReturnType.Guid</example>
    [DefaultValue(ReturnType.String)]
    public ReturnType ReturnType { get; set; }
}
