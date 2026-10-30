using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.LDAP.UpdateUser.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Modification method.
    /// </summary>
    /// <example>ModificationMethod.Add</example>
    [DefaultValue(ModificationMethod.Add)]
    public ModificationMethod ModificationMethod { get; set; }

    /// <summary>
    /// Set true to create distinguished name (DN) from Input.CommonName and Input.Path instead of using Input CreateDN.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool CreateDN { get; set; }

    /// <summary>
    /// Distinguished name (DN).
    /// </summary>
    /// <example>CN=Tes TUser,ou=users,dc=wimpi,dc=net</example>
    [UIHint(nameof(CreateDN), "", false)]
    public string DistinguishedName { get; set; }

    /// <summary>
    /// Common name (CN).
    /// </summary>
    /// <example>Firstname Lastname</example>
    [UIHint(nameof(CreateDN), "", true)]
    public string CommonName { get; set; }

    /// <summary>
    /// Directory path.
    /// </summary>
    /// <example>CN=Users,DC=Example,DC=Com</example>
    [UIHint(nameof(CreateDN), "", true)]
    [DefaultValue("CN=Users,DC=Example,DC=Com")]
    public string Path { get; set; }

    /// <summary>
    /// Attributes to be updated.
    /// </summary>
    /// <example>Telephone, +358123456789</example>
    public Attributes[] Attributes { get; set; } = Array.Empty<Attributes>();
}
