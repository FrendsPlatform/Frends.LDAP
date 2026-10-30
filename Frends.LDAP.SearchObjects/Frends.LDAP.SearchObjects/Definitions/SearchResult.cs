using System.Collections.Generic;

namespace Frends.LDAP.SearchObjects.Definitions;

/// <summary>
/// Search result.
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Distinguished name of the entry.
    /// </summary>
    /// <example>CN=Foo Bar,ou=users,dc=wimpi,dc=net</example>
    public string DistinguishedName { get; set; }

    /// <summary>
    /// Search result's attributes.
    /// </summary>
    /// <example>{ Key = "sn", Value = "Bar" }, DistinguishedName = "CN=Foo Bar,ou=users,dc=wimpi,dc=net" }</example>
    public Dictionary<string, dynamic> AttributeSet { get; set; }
}