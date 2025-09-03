using Frends.LDAP.CreateUser.Definitions;
using System.ComponentModel;
using Novell.Directory.Ldap;
using System;
using System.Text;

namespace Frends.LDAP.CreateUser;

/// <summary>
/// LDAP task.
/// </summary>
public class LDAP
{
    /// <summary>
    /// Create a user to Active Directory.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.LDAP.CreateUser)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <returns>Object { bool Success, string Error, string CommonName, bool PasswordSet, string Path }</returns>
    public static Result CreateUser([PropertyTab] Input input, [PropertyTab] Connection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
            throw new Exception("Connection parameters missing.");

        LdapConnection conn = new();

        try
        {
            var defaultPort = connection.SecureSocketLayer ? 636 : 389;
            var entry = $"CN={input.CommonName},{input.Path}";

            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);
            if (connection.TLS)
                throw new Exception("Active Directory password changes require SSL. TLS is not supported for unicodePwd.");

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

                if (input.PasswordIsUnicoded)
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
            return new Result(true, null, input.CommonName, input.SetPassword, input.Path);
        }
        catch (LdapException ex)
        {
            return new Result(false, ex.Message, input.CommonName, false, input.Path);
        }
        catch (Exception ex)
        {
            throw new Exception($"CreateUser error: {ex}");
        }
        finally
        {
            if (connection.TLS) conn.StopTls();
            conn.Disconnect();
        }
    }
}







