using System;
using System.ComponentModel;
using System.Threading;
using Frends.LDAP.DeleteUser.Definitions;
using Novell.Directory.Ldap;

namespace Frends.LDAP.DeleteUser;

/// <summary>
/// LDAP task.
/// </summary>
public static class LDAP
{
    /// <summary>
    /// Delete a user from Active Directory.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.LDAP.DeleteUser)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Object { bool Success, string Error, string CommonName, string Path }</returns>
    public static Result DeleteUser([PropertyTab] Input input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
            throw new Exception("Connection parameters missing.");

        cancellationToken.ThrowIfCancellationRequested();

        LdapConnection conn = new();

        try
        {
            var defaultPort = connection.SecureSocketLayer ? 636 : 389;
            var entry = $"CN={input.CommonName},{input.Path}";

            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);
            if (connection.TLS) conn.StartTls();
            conn.Bind(connection.User, connection.Password);

            conn.Delete(entry);

            return new Result(true, null, input.CommonName, input.Path);
        }
        catch (LdapException ex)
        {
            if (options.ThrowErrorOnFailure)
                throw new Exception(string.IsNullOrEmpty(options.ErrorMessageOnFailure) ? ex.Message : options.ErrorMessageOnFailure, ex);

            var message = string.IsNullOrEmpty(options.ErrorMessageOnFailure) ? ex.Message : $"{options.ErrorMessageOnFailure}: {ex.Message}";
            return new Result(false, message, input.CommonName, input.Path);
        }
        catch (Exception ex)
        {
            throw new Exception($"DeleteUser error: {ex}");
        }
        finally
        {
            if (connection.TLS) conn.StopTls();
            conn.Disconnect();
        }
    }
}