namespace Frends.LDAP.CreateUser.Definitions;

/// <summary>
/// Task's result.
/// </summary>
public class Result
{
    /// <summary>
    /// User created.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Common name.
    /// </summary>
    /// <example>Firstname Lastname</example>
    public string CommonName { get; set; }

    /// <summary>
    /// Password set.
    /// </summary>
    /// <example>true</example>
    public bool PasswordSet { get; set; }

    /// <summary>
    /// Path.
    /// </summary>
    /// <example>CN=Users,DC=Example,DC=Com</example>
    public string Path { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
