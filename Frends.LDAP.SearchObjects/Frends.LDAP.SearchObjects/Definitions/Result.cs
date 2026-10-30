using System.Collections.Generic;

namespace Frends.LDAP.SearchObjects.Definitions;

/// <summary>
/// Task's results.
/// </summary>
public class Result
{
    /// <summary>
    /// Search completed.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }

    /// <summary>
    /// Results.
    /// </summary>
    /// <example>[ { AttributeSet { Key = "sn", Value = "Bar" }, DistinguishedName = "CN=Foo Bar,ou=users,dc=wimpi,dc=net" } ]</example>
    public List<SearchResult> SearchResult { get; set; }
}