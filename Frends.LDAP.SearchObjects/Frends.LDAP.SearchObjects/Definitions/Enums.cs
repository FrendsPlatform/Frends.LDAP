namespace Frends.LDAP.SearchObjects.Definitions;

/// <summary>
/// Search scopes.
/// </summary>
public enum Scopes
{
    /// <summary>
    /// Searches only the base DN.
    /// </summary>
    ScopeBase,

    /// <summary>
    /// Searches only entries under the base DN.
    /// </summary>
    ScopeOne,

    /// <summary>
    /// Searches the base DN and all entries within its subtree.
    /// </summary>
    ScopeSub
}

/// <summary>
/// Search dereferences
/// </summary>
public enum SearchDereference
{
    /// <summary>
    /// Indicates that aliases are never dereferenced.
    /// </summary>
    DerefNever,

    /// <summary>
    /// Indicates that aliases are are derefrenced when searching the entries beneath the starting point of the search, but not when finding the starting entry..
    /// </summary>
    DerefSearching,

    /// <summary>
    /// Indicates that aliases are dereferenced when finding the starting point for the search, but not when searching under that starting entry.
    /// </summary>
    DerefFinding,

    /// <summary>
    /// Indicates that aliases are always dereferenced, both when finding the starting point for the search, and also when searching the entries beneath the starting entry.
    /// </summary>
    DerefAlways,
}

/// <summary>
/// LDAP protocol versions.
/// </summary>
public enum LDAPVersion
{
    /// <summary>
    /// LDAP Version 2
    /// </summary>
    V2,
    /// <summary>
    /// LDAP Version 3
    /// </summary>
    V3,
}

/// <summary>
/// Enumeration of file encoding options.
/// </summary>
public enum ContentEncoding
{
    // Pragma for self-explanatory enum attributes.
#pragma warning disable 1591, SA1602
    UTF8,
    Default,
    ASCII,
    WINDOWS1252,
#pragma warning restore 1591, SA1602
    /// <summary>
    /// Other enables users to add other encoding options as string.
    /// </summary>
    Other,
}

/// <summary>
/// Enumeration of return types.
/// </summary>
public enum ReturnType
{
    // Pragma for self-explanatory enum attributes.
#pragma warning disable 1591, SA1602
    String,
    Byte,
#pragma warning restore 1591, SA1602
}
