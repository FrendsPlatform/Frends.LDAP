using System;
using System.ComponentModel;
using System.Threading;
using Frends.LDAP.DeleteUser.Definitions;
using Frends.LDAP.DeleteUser.Helpers;
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
    /// <returns>object { bool Success, string CommonName, string Path, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result DeleteUser([PropertyTab] Input input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        LdapConnection conn = new();

        try
        {
            if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
                throw new ArgumentException("Connection parameters missing.");

            cancellationToken.ThrowIfCancellationRequested();

            var defaultPort = connection.SecureSocketLayer ? 636 : 389;
            var entry = $"CN={input.CommonName},{input.Path}";

            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);
            if (connection.TLS) conn.StartTls();
            conn.Bind(connection.User, connection.Password);

            conn.Delete(entry);

            return new Result { Success = true, CommonName = input.CommonName, Path = input.Path };
        }
        catch (Exception ex)
        {
            var result = ex.Handle(options);
            result.CommonName = input.CommonName;
            result.Path = input.Path;
            return result;
        }
        finally
        {
            if (conn.Connected)
            {
                if (connection.TLS) conn.StopTls();
                conn.Disconnect();
            }
        }
    }
}