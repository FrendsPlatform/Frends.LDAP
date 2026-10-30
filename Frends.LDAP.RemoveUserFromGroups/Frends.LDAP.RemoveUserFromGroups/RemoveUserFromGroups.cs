using System;
using System.ComponentModel;
using System.Threading;
using Frends.LDAP.RemoveUserFromGroups.Definitions;
using Frends.LDAP.RemoveUserFromGroups.Helpers;
using Novell.Directory.Ldap;

namespace Frends.LDAP.RemoveUserFromGroups;

/// <summary>
/// LDAP task.
/// </summary>
public static class LDAP
{
    /// <summary>
    /// Remove a user from Active Directory groups.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.LDAP.RemoveUserFromGroups)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string UserDistinguishedName, string GroupDistinguishedName, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result RemoveUserFromGroups([PropertyTab] Input input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        LdapConnection conn = new();

        try
        {
            if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
                throw new ArgumentException("Connection parameters missing.");

            cancellationToken.ThrowIfCancellationRequested();

            var defaultPort = connection.SecureSocketLayer ? 636 : 389;

            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);
            if (connection.TLS) conn.StartTls();
            conn.Bind(connection.User, connection.Password);

            LdapModification[] mods = new LdapModification[1];
            var member = new LdapAttribute("member", input.UserDistinguishedName);
            mods[0] = new LdapModification(LdapModification.Delete, member);
            conn.Modify(input.GroupDistinguishedName, mods);

            return new Result
            {
                Success = true,
                UserDistinguishedName = input.UserDistinguishedName,
                GroupDistinguishedName = input.GroupDistinguishedName,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options, input);
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