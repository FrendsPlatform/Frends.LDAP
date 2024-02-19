using Frends.LDAP.DeleteUser.Definitions;
using System.ComponentModel;
using Novell.Directory.Ldap;
using System;
namespace Frends.LDAP.DeleteUser;

/// <summary>
/// LDAP task.
/// </summary>
public class LDAP
{
    /// <summary>
    /// Delete a user from Active Directory.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.LDAP.DeleteUser)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <returns>Object { bool Success, string Error, string CommonName, string Path }</returns>
    public static Result DeleteUser([PropertyTab] Input input, [PropertyTab] Connection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
            throw new Exception("Connection parameters missing.");

        LdapConnectionOptions ldco = new LdapConnectionOptions();

        if (connection.IgnoreCertificates) {
            ldco.ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);
        }
            
        LdapConnection conn = new LdapConnection(ldco);

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
            return new Result(false, ex.Message, input.CommonName, input.Path);
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