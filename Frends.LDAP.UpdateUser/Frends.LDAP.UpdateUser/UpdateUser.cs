using Frends.LDAP.UpdateUser.Definitions;
using System.ComponentModel;
using Novell.Directory.Ldap;
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Loader;

namespace Frends.LDAP.UpdateUser;

/// <summary>
/// LDAP task.
/// </summary>
public class LDAP
{
    /// For mem cleanup.
    static LDAP()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        var currentContext = AssemblyLoadContext.GetLoadContext(currentAssembly);
        if (currentContext != null)
            currentContext.Unloading += OnPluginUnloadingRequested;
    }

    /// <summary>
    /// Update Active Directory user.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.LDAP.UpdateUser)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <returns>Object { bool Success, string Error, string CommonName, string Path }</returns>
    public static Result UpdateUser([PropertyTab] Input input, [PropertyTab] Connection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
            throw new Exception("UpdateUser error: Connection parameters missing.");
        if (input.Attributes.Length == 0)
            throw new Exception("UpdateUser error: Attributes missing.");

        LdapConnection conn = new();

        try
        {
            var defaultPort = connection.SecureSocketLayer ? 636 : 389;
            var entry = input.CreateDN ? $"CN={input.CommonName},{input.Path}" : input.DistinguishedName;

            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);
            if (connection.TLS) conn.StartTls();
            conn.Bind(connection.User, connection.Password);

            var modList = new ArrayList();
            var modMethod = LdapModification.Add;

            switch (input.ModificationMethod)
            {
                case ModificationMethod.Add:
                    modMethod = LdapModification.Add;
                    break;
                case ModificationMethod.Delete:
                    modMethod = LdapModification.Delete;
                    break;
                case ModificationMethod.Replace:
                    modMethod = LdapModification.Replace;
                    break;
            }

            foreach (var item in input.Attributes)
                modList.Add(new LdapModification(modMethod, new LdapAttribute(item.Key, string.IsNullOrWhiteSpace(item.Value) ? " " : item.Value)));

            var mods = new LdapModification[modList.Count];
            var mtype = Type.GetType("Novell.Directory.LdapModification");
            mods = (LdapModification[])modList.ToArray(typeof(LdapModification));
            conn.Modify(entry, mods);

            return new Result(true, null, input.CommonName, input.Path);
        }
        catch (LdapException ex)
        {
            return new Result(false, ex.Message, input.CommonName, input.Path);
        }
        catch (Exception ex)
        {
            throw new Exception($"UpdateUser error: {ex}");
        }
        finally
        {
            if (connection.TLS) conn.StopTls();
            conn.Disconnect();
        }
    }

    private static void OnPluginUnloadingRequested(AssemblyLoadContext obj)
    {
        obj.Unloading -= OnPluginUnloadingRequested;
    }
}