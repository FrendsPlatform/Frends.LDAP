using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Frends.LDAP.SearchObjects.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// LDAP server host.
    /// This can be a domain name, LDAP alias, or a Domain Controller FQDN.
    /// </summary>
    /// <example>emea.company.com</example>
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
    /// Ignore server certificates
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool IgnoreCertificates { get; set; }

    /// <summary>
    /// Connection is protected by TLS.
    /// </summary>
    /// <example>true</example>
    public bool TLS { get; set; }

    /// <summary>
    /// Used LDAP protocol version.
    /// Warning: LDAPv2 is deprecated and has security vulnerabilities. Use LDAPv3 unless absolutely required.
    /// </summary>
    /// <example>LDAPVersion.V3</example>
    [DefaultValue(LDAPVersion.V3)]
    public LDAPVersion LDAPProtocolVersion { get; set; }

    /// <summary>
    /// If enabled credentials are not used to create a bind to the LDAP server.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool AnonymousBind { get; set; }

    /// <summary>
    /// User.
    /// </summary>
    /// <example>Foo</example>
    [UIHint(nameof(AnonymousBind), "", false)]
    public string User { get; set; }

    /// <summary>
    /// Password.
    /// </summary>
    /// <example>Bar123</example>
    [UIHint(nameof(AnonymousBind), "", false)]
    [PasswordPropertyText]
    public string Password { get; set; }

    /// <summary>
    /// If enabled Task throws an exception when LDAP error happens.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool ThrowExceptionOnError { get; set; }
}