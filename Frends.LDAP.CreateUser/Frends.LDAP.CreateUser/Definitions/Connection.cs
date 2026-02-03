using System.ComponentModel;
namespace Frends.LDAP.CreateUser.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// LDAP server host.
    /// This must resolve to a Domain Controller (FQDN) in the same domain
    /// where the new object (user) will be created.
    /// </summary>
    /// <example>dc1.emea.company.com</example>
    public string Host { get; set; }

    /// <summary>
    /// Port. Value 0 = use LDAP/LDAPS default port which is 389 or 636 depending on (SecureSocketLayer) and (TLS).
    /// </summary>
    /// <example>389</example>
    [DefaultValue(0)]
    public int Port { get; set; }

    /// <summary>
    /// Perform secure operation.
    /// </summary>
    /// <example>true</example>
    public bool SecureSocketLayer { get; set; }

    /// <summary>
    /// Connection is protected by TLS.
    /// </summary>
    /// <example>true</example>
    public bool TLS { get; set; }

    /// <summary>
    /// User.
    /// </summary>
    /// <example>Foo</example>
    public string User { get; set; }

    /// <summary>
    /// Password.
    /// </summary>
    /// <example>Bar123</example>
    [PasswordPropertyText]
    public string Password { get; set; }
}