using Frends.LDAP.AddUserToGroups.Definitions;
using System.ComponentModel;
using Novell.Directory.Ldap;
using System;

namespace Frends.LDAP.AddUserToGroups;

/// <summary>
/// LDAP task.
/// </summary>
public class LDAP
{
    /// <summary>
    /// Add user to Active Directory groups.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.LDAP.AddUserToGroups)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <returns>Object { bool Success, string Error, string CommonName, string Path }</returns>
    public static Result AddUserToGroups([PropertyTab] Input input, [PropertyTab] Connection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
            throw new Exception("AddUserToGroups error: Connection parameters missing.");

        LdapConnection conn = new();

        try
        {
            var defaultPort = connection.SecureSocketLayer ? 636 : 389;

            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);
            if (connection.TLS) conn.StartTls();
            conn.Bind(connection.User, connection.Password);

			LdapModification[] mods = new LdapModification[1];
			var member = new LdapAttribute("member", input.UserDistinguishedName);
			mods[0] = new LdapModification(LdapModification.Add, member);
			conn.Modify(input.GroupDistinguishedName, mods);

			return new Result(true, null, input.UserDistinguishedName, input.GroupDistinguishedName);
        }
        catch (LdapException ex)
        {
            if (ex.Message.Equals("Entry Already Exists") && input.UserExistsAction.Equals(UserExistsAction.Skip)) 
                return new Result(false, ex.Message, input.UserDistinguishedName, input.GroupDistinguishedName);
            else
                throw new Exception($"AddUserToGroups LDAP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"AddUserToGroups error: {ex}");
        }
        finally
        {
            if (connection.TLS) conn.StopTls();
            conn.Disconnect();
        }
    }
}