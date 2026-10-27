namespace Frends.LDAP.DeleteUser.Definitions;

/// <summary>
/// Task's result.
/// </summary>
public class Result
{
    internal Result(bool success, string error, string commonName, string path)
    {
        Success = success;
        Error = error;
        CommonName = commonName;
        Path = path;
    }

    /// <summary>
    /// User deleted.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// LDAP Error message.
    /// </summary>
    /// <example>Entry Already Exists</example>
    public string Error { get; private set; }

    /// <summary>
    /// Common name.
    /// </summary>
    /// <example>Firstname Lastname</example>
    public string CommonName { get; private set; }

    /// <summary>
    /// Path.
    /// </summary>
    /// <example>CN=Users,DC=Example,DC=Com</example>
    public string Path { get; private set; }
}