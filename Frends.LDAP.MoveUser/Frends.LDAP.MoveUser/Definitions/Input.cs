using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.LDAP.MoveUser.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Full distinguished name (DN) of the existing user to move.
    /// The DN must include the domain components (DC).
    /// </summary>
    /// <example>CN=Jane Doe,OU=Sales,DC=example,DC=com</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    [Required]
    public string SourceDistinguishedName { get; set; } = string.Empty;

    /// <summary>
    /// Full destination DN, including the user's relative distinguished name (RDN).
    /// The destination container must already exist on the same server and in the same domain.
    /// Keep the first RDN unchanged to move without renaming the user.
    /// </summary>
    /// <example>CN=Jane Doe,OU=Support,DC=example,DC=com</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    [Required]
    public string DestinationDistinguishedName { get; set; } = string.Empty;
}
