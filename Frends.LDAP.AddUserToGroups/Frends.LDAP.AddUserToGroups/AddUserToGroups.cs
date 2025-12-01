using Frends.LDAP.AddUserToGroups.Definitions;
using System.ComponentModel;
using Novell.Directory.Ldap;
using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

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
    /// <param name="cancellationToken">Cancellation token given by Frends.</param>
    /// <returns>Object { bool Success, string Error, string UserDistinguishedName, string GroupDistinguishedName }</returns>
    public static Result AddUserToGroups([PropertyTab] Input input, [PropertyTab] Connection connection, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
            throw new Exception("AddUserToGroups error: Connection parameters missing.");

        using LdapConnection conn = new();

        try
        {
            var defaultPort = connection.SecureSocketLayer ? 636 : 389;

            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);
            if (connection.TLS)
                conn.StartTls();
            conn.Bind(connection.User, connection.Password);

            LdapModification[] mods = new LdapModification[1];
            var member = new LdapAttribute("member", input.UserDistinguishedName);
            mods[0] = new LdapModification(LdapModification.Add, member);

            if (UserExistsInGroup(conn, input.UserDistinguishedName, input.GroupDistinguishedName, cancellationToken) && input.UserExistsAction.Equals(UserExistsAction.Skip))
                return new Result(false, "AddUserToGroups LDAP error: User already exists in the group.", input.UserDistinguishedName, input.GroupDistinguishedName);

            conn.Modify(input.GroupDistinguishedName, mods);

            return new Result(true, null, input.UserDistinguishedName, input.GroupDistinguishedName);
        }
        catch (LdapException ex)
        {
            if (ex.ResultCode == LdapException.AttributeOrValueExists && input.UserExistsAction == UserExistsAction.Skip)
            {
                return new Result(false, "User already exists in the group, skipped as requested.", input.UserDistinguishedName, input.GroupDistinguishedName);
            }
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

    private static bool UserExistsInGroup(LdapConnection connection, string userDn, string groupDn, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ILdapSearchResults searchResults = connection.Search(
            groupDn,
            LdapConnection.ScopeSub,
            "(objectClass=*)",
            null,
            false);

        if (searchResults.HasMore())
        {
            try
            {
                var entry = searchResults.Next();
                var memberAttr = entry.GetAttribute("member");
                return memberAttr.StringValueArray.Contains(userDn, StringComparer.OrdinalIgnoreCase);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        return false;
    }
}