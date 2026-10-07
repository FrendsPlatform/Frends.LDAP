using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.LDAP.MoveUser.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
[ExclusiveEncryption]
public class Connection
{
    /// <summary>
    /// Single LDAP server hostname or IP address, without a scheme or port.
    /// For Active Directory, use a writable domain controller in the user's domain.
    /// </summary>
    /// <example>dc1.example.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    [Required]
    [LdapHost]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// LDAP server port. Zero selects 636 for SecureSocketLayer or 389 otherwise, including StartTLS.
    /// </summary>
    /// <example>389</example>
    [DefaultValue(0)]
    [Range(0, 65535)]
    public int Port { get; set; }

    /// <summary>
    /// Use LDAPS. Cannot be combined with TLS. The server certificate must be trusted by the Agent.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool SecureSocketLayer { get; set; }

    /// <summary>
    /// Upgrade the LDAP connection with StartTLS before sending credentials.
    /// Cannot be combined with SecureSocketLayer. The server certificate must be trusted by the Agent.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool TLS { get; set; }

    /// <summary>
    /// Bind user DN or a server-supported username with permission to read and move the user.
    /// </summary>
    /// <example>CN=Service Account,OU=Services,DC=example,DC=com</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    [Required]
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Bind user's password. Use a secret environment variable in the Process.
    /// </summary>
    /// <example>#env.LdapPassword</example>
    [PasswordPropertyText]
    [DefaultValue("")]
    [Required]
    public string Password { get; set; } = string.Empty;
}
