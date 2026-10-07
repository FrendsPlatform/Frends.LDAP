using System;

namespace Frends.LDAP.MoveUser.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>The source distinguished name does not identify a user.</example>
    public string Message { get; set; }

    /// <summary>
    /// Original exception, including the LDAP result code when the server rejects an operation.
    /// </summary>
    /// <example>object { Exception AdditionalInfo }</example>
    public Exception AdditionalInfo { get; set; }
}
