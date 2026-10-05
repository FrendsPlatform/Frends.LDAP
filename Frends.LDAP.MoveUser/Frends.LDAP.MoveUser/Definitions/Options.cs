using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.LDAP.MoveUser.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Timeout in seconds for connecting and for each LDAP operation.
    /// Cancellation is checked between synchronous LDAP operations.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    [Range(1, int.MaxValue / 1000)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
