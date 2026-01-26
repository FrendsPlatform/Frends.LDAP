using Frends.LDAP.RemoveUserFromGroups.Definitions;
using System.ComponentModel;
using Novell.Directory.Ldap;
using System;

namespace Frends.LDAP.RemoveUserFromGroups;

/// <summary>
/// LDAP task.
/// </summary>
public class LDAP
{
    /// <summary>
    /// Remove a user from Active Directory groups.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.LDAP.RemoveUserFromGroups)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <returns>Object { bool Success, string Error, string CommonName, string Path }</returns>
    public static Result RemoveUserFromGroups([PropertyTab] Input input, [PropertyTab] Connection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
            throw new Exception("RemoveUserFromGroups error: Connection parameters missing.");

        LdapConnectionOptions ldco = new LdapConnectionOptions();

        if (connection.IgnoreCertificates)
            ldco.ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

        using var conn = new LdapConnection(ldco);

        try
        {
            var defaultPort = connection.SecureSocketLayer ? 636 : 389;

            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);
            if (connection.TLS) conn.StartTls();
            conn.Bind(connection.User, connection.Password);

			LdapModification[] mods = new LdapModification[1];
			var member = new LdapAttribute("member", input.UserDistinguishedName);
			mods[0] = new LdapModification(LdapModification.Delete, member);
			conn.Modify(input.GroupDistinguishedName, mods);

			return new Result(true, null, input.UserDistinguishedName, input.GroupDistinguishedName);
        }
        catch (LdapException ex)
        {
            if (input.HandleLDAPError.Equals(HandleLDAPError.Skip)) 
                return new Result(false, ex.Message, input.UserDistinguishedName, input.GroupDistinguishedName);
            else
                throw new Exception($"RemoveUserFromGroups LDAP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"RemoveUserFromGroups error: {ex}");
        }
        finally
        {
            if (connection.TLS) conn.StopTls();
            conn.Disconnect();
        }
    }
}