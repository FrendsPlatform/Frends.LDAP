using System;
using System.ComponentModel;
namespace Frends.LDAP.CreateUser.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// ObjectClass. 
    /// </summary>
    /// <example>User</example>
    public string ObjectClass { get; set; }

    /// <summary>
    /// Directory path.
    /// </summary>
    /// <example>CN=Users,DC=Example,DC=Com</example>
    [DefaultValue("CN=Users,DC=Example,DC=Com")]
    public string Path { get; set; }

    /// <summary>
    /// Given name.
    /// </summary>
    /// <example>Firstname</example>
    public string GivenName { get; set; }

    /// <summary>
    /// Surname (SN).
    /// </summary>
    /// <example>Lastname</example>
    public string Surname { get; set; }

    /// <summary>
    /// Common name (CN).
    /// </summary>
    /// <example>Firstname Lastname</example>
    public string CommonName { get; set; }

    /// <summary>
    /// Email.
    /// </summary>
    /// <example>firstname.lastname@foo.bar</example>
    public string Email { get; set; }

    /// <summary>
    /// Set password to new user.
    /// </summary>
    /// <example>true</example>
    public bool SetPassword { get; set; }

    /// <summary>
    /// Password is unicoded to new user.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool PasswordIsUnicoded { get; set; }

    /// <summary>
    /// Password.
    /// </summary>
    /// <example>Password123</example>
    [PasswordPropertyText(true)]
    public string Password { get; set; }

    /// <summary>
    /// Custom attribute values. 
    /// </summary>
    /// <example>Telephone, +358123456789</example>
    public Attributes[] Attributes { get; set; } = Array.Empty<Attributes>();
}

/// <summary>
/// Attribute values.
/// </summary>
public class Attributes
{
    /// <summary>
    /// Key.
    /// </summary>
    /// <example>Telephone</example>
    public string Key { get; set; }

    /// <summary>
    /// Value.
    /// </summary>
    /// <example>+358123456789</example>
    public string Value { get; set; }
}