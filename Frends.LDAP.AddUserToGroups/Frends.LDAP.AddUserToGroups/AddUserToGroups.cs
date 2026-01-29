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
    /// <returns>Object { bool Success, string Error, string Details, string UserDistinguishedName, string GroupDistinguishedName }</returns>
    public static Result AddUserToGroups([PropertyTab] Input input, [PropertyTab] Connection connection, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
            throw new Exception("AddUserToGroups error: Connection parameters missing.");

        if (input?.GroupDistinguishedNames == null || input.GroupDistinguishedNames.Length == 0)
            throw new Exception("AddUserToGroups error: GroupDistinguishedNames is required.");

        using LdapConnection conn = new();
        try
        {
            var defaultPort = connection.SecureSocketLayer ? 636 : 389;
            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);
            if (connection.TLS)
                conn.StartTls();
            conn.Bind(connection.User, connection.Password);

            var addedGroups = new List<string>();
            var skippedGroups = new Dictionary<string, string>();

            foreach (var groupDn in input.GroupDistinguishedNames)
            {
                cancellationToken.ThrowIfCancellationRequested();

                LdapModification[] mods = new LdapModification[1];
                var member = new LdapAttribute("member", input.UserDistinguishedName);
                mods[0] = new LdapModification(LdapModification.Add, member);

                if (UserExistsInGroup(conn, input.UserDistinguishedName, groupDn, cancellationToken)
                    && input.UserExistsAction == UserExistsAction.Skip)
                {
                    skippedGroups.Add(groupDn, "User already exists in the group");
                    continue;
                }

                try
                {
                    conn.Modify(groupDn, mods);
                    addedGroups.Add(groupDn);
                }
                catch (LdapException ex)
                {
                    if (ex.ResultCode == LdapException.AttributeOrValueExists && input.UserExistsAction == UserExistsAction.Skip)
                    {
                        skippedGroups.Add(groupDn, "User already exists in the group");
                        continue;
                    }
                    throw new Exception($"AddUserToGroups LDAP error: {ex.Message}");
                }
            }

            var success = addedGroups.Count > 0;
            string error = null;
            string details = null;

            if (addedGroups.Count == 0 && skippedGroups.Count > 0)
            {
                if (input.GroupDistinguishedNames.Length == 1)
                {
                    error = "AddUserToGroups LDAP error: User already exists in the group.";
                }
                else
                {
                    var skipDetails = string.Join("; ", skippedGroups.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                    error = $"User already exists in all groups, skipped as requested. Details: {skipDetails}";
                }
                success = false;
            }
            else if (skippedGroups.Count > 0)
            {
                var skipDetails = string.Join("; ", skippedGroups.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                details = $"Added to {addedGroups.Count} group(s): {string.Join(", ", addedGroups)}. Skipped {skippedGroups.Count} group(s): {skipDetails}";
            }

            return new Result(success, error, details, input.UserDistinguishedName, string.Join(", ", input.GroupDistinguishedNames));
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