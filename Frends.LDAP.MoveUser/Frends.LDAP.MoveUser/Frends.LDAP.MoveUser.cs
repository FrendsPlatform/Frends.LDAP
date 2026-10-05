using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Frends.LDAP.MoveUser.Definitions;
using Frends.LDAP.MoveUser.Helpers;
using Novell.Directory.Ldap;

namespace Frends.LDAP.MoveUser;

/// <summary>
/// Task Class for LDAP operations.
/// </summary>
public static class LDAP
{
    /// <summary>
    /// Moves a user to another distinguished name on the same LDAP server and domain.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-LDAP-MoveUser)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string SourceDistinguishedName, string DestinationDistinguishedName, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result MoveUser(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, connection, options);

            var (newRdn, newParent) = DistinguishedNameHelper.GetMoveTarget(input);
            var port = connection.Port == 0
                ? (connection.SecureSocketLayer ? LdapConnection.DefaultSslPort : LdapConnection.DefaultPort)
                : connection.Port;

            using var client = CreateConnection(connection, options);
            cancellationToken.ThrowIfCancellationRequested();
            client.Connect(connection.Host, port);

            if (connection.TLS)
            {
                cancellationToken.ThrowIfCancellationRequested();
                client.StartTls();
            }

            cancellationToken.ThrowIfCancellationRequested();
            client.Bind(connection.User, connection.Password);

            cancellationToken.ThrowIfCancellationRequested();
            var entry = client.Read(input.SourceDistinguishedName, ["objectClass"]);
            var objectClasses = entry?.GetAttribute("objectClass")?.StringValueArray ?? Array.Empty<string>();
            if (objectClasses.Contains("computer", StringComparer.OrdinalIgnoreCase)
                || !objectClasses.Any(value =>
                    value.Equals("person", StringComparison.OrdinalIgnoreCase)
                    || value.Equals("organizationalPerson", StringComparison.OrdinalIgnoreCase)
                    || value.Equals("inetOrgPerson", StringComparison.OrdinalIgnoreCase)
                    || value.Equals("user", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("The source distinguished name does not identify a user.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            client.Rename(input.SourceDistinguishedName, newRdn, newParent, true);

            return new Result
            {
                Success = true,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    internal static LdapConnection CreateConnection(Connection connection, Options options)
    {
        return new LdapConnection
        {
            SecureSocketLayer = connection.SecureSocketLayer,
            ConnectionTimeout = options.TimeoutSeconds * 1000,
            Constraints = new LdapConstraints
            {
                TimeLimit = options.TimeoutSeconds * 1000,
                ReferralFollowing = false,
            },
        };
    }
}
