using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.LDAP.SearchObjects.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// The search base parameter specifies the DN of the entry where you want to begin the search.
    /// If you want the search to begin at the tree root pass an empty string.
    /// </summary>
    /// <example>"ou=users,dc=wimpi,dc=net"</example>
    public string SearchBase { get; set; }

    /// <summary>
    /// The search scope parameter specifies the depth of the search.
    /// </summary>
    /// <example>Scopes.ScopeBase</example>
    public Scopes Scope { get; set; }

    /// <summary>
    /// The search filter defines the entries that will be returned by the search. The LDAP search filter grammar is specified in RFC 2254 and 2251. The grammar uses ABNF notation. If you are looking for all employees with a title of engineer, the search filter would be (title=engineer). 
    /// </summary>
    /// <example>(title=engineer)</example>
    public string Filter { get; set; }

    /// <summary>
    /// The maximum time in milliseconds to wait for results. The default is 0, which means that there is no maximum time limit.
    /// </summary>
    /// <example>0</example>
    [DefaultValue(0)]
    public int MsLimit { get; set; }

    /// <summary>
    /// The maximum time in seconds that the server should spend returning search results. This is a server-enforced limit. The default of 0 means no time limit.
    /// </summary>
    /// <example>0</example>
    [DefaultValue(0)]
    public int ServerTimeLimit { get; set; }

    /// <summary>
    /// Specifies when aliases should be dereferenced.
    /// DerefNever, DerefFinding, DerefAlways
    /// </summary>
    /// <example>SearchConstraints.DerefNever</example>
    [DefaultValue(SearchDereference.DerefNever)]
    public SearchDereference SearchDereference { get; set; }

    /// <summary>
    /// The maximum number of search results to return for a search request.
    /// </summary>
    /// <example>1000</example>
    [DefaultValue(1000)]
    public int MaxResults { get; set; }

    /// <summary>
    /// The number of results to return in a batch. Specifying 0 means to block until all results are received. Specifying 1 means to return results one result at a time.
    /// </summary>
    /// <example>1</example>
    [DefaultValue(1)]
    public int BatchSize { get; set; }

    /// <summary>
    /// If true, returns the names but not the values of the attributes found. 
    /// If false, returns the names and values for attributes found.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool TypesOnly { get; set; }

    /// <summary>
    /// Specifies the encoding of the content.
    /// </summary>
    /// <example>ContentEncoding.UTF8</example>
    public ContentEncoding ContentEncoding { get; set; }

    /// <summary>
    /// Enable BOM in UTF-8 encoding.
    /// </summary>
    /// <example>true</example>
    [UIHint(nameof(ContentEncoding), "", ContentEncoding.UTF8)]
    public bool EnableBom { get; set; }

    /// <summary>
    /// Content encoding as string.
    /// </summary>
    /// <example>windows-1251</example>
    [UIHint(nameof(ContentEncoding), "", ContentEncoding.Other)]
    public string ContentEncodingString { get; set; }

    /// <summary>
    /// Determine if only specified attributes should be returned.
    /// This allows user to specify encoding and type for certain attributes.
    /// </summary>
    [DefaultValue(true)]
    public bool SearchOnlySpecifiedAttributes { get; set; }

    /// <summary>
    /// The names of attributes to retrieve.
    /// </summary>
    /// <example>cn</example>
    public Attributes[] Attributes { get; set; } = Array.Empty<Attributes>();
}

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
    /// Enables the return value to be returned as a byte array.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool ReturnAsByteArray { get; set; }
}