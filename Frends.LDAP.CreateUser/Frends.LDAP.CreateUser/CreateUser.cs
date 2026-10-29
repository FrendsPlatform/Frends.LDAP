using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Frends.LDAP.CreateUser.Definitions;
using Frends.LDAP.CreateUser.Helpers;
using Novell.Directory.Ldap;

namespace Frends.LDAP.CreateUser;

/// <summary>
/// LDAP task.
/// </summary>
public static class LDAP
{
    /// <summary>
    /// Create a user to Active Directory.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.LDAP.CreateUser)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string CommonName, bool PasswordSet, string Path, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result CreateUser([PropertyTab] Input input, [PropertyTab] Connection connection, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        LdapConnection conn = new();

        try
        {
            if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
                throw new Exception("Connection parameters missing.");

            if ((connection.SecureSocketLayer == false && connection.TLS == false) && input.SetPasswordInUnicode)
                throw new Exception("Active Directory password changes require SSL or TLS or both.");

            cancellationToken.ThrowIfCancellationRequested();
            var defaultPort = connection.SecureSocketLayer ? 636 : 389;
            var entry = $"CN={input.CommonName},{input.Path}";

            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);

            if (connection.TLS)
                conn.StartTls();

            conn.Bind(connection.User, connection.Password);

            LdapAttributeSet attributeSet = new();
            attributeSet.Add(new LdapAttribute("objectclass", input.ObjectClass));
            attributeSet.Add(new LdapAttribute("cn", input.CommonName));
            if (!string.IsNullOrWhiteSpace(input.GivenName)) attributeSet.Add(new LdapAttribute("givenname", input.GivenName));
            if (!string.IsNullOrWhiteSpace(input.Surname)) attributeSet.Add(new LdapAttribute("sn", input.Surname));
            if (!string.IsNullOrWhiteSpace(input.Email)) attributeSet.Add(new LdapAttribute("mail", input.Email.ToLower()));

            if (input.SetPassword && !string.IsNullOrWhiteSpace(input.Password))
            {
                string quotedPassword = $"\"{input.Password}\"";

                if (input.SetPasswordInUnicode)
                {
                    byte[] passwordBytes = Encoding.Unicode.GetBytes(quotedPassword);
                    attributeSet.Add(new LdapAttribute("unicodePwd", passwordBytes));
                }
                else
                {
                    attributeSet.Add(new LdapAttribute("userpassword", input.Password));
                }
            }

            // Manually set LdapAttributes.
            if (input.Attributes.Length != 0)
                foreach (var i in input.Attributes) attributeSet.Add(new LdapAttribute(i.Key, i.Value));

            LdapEntry newEntry = new(entry, attributeSet);
            conn.Add(newEntry);
            return new Result
            {
                Success = true,
                CommonName = input.CommonName,
                PasswordSet = input.SetPassword,
                Path = input.Path,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
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
