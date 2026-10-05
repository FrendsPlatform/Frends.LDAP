using System;
using System.Linq;
using Frends.LDAP.MoveUser.Definitions;
using Novell.Directory.Ldap.Utilclass;

namespace Frends.LDAP.MoveUser.Helpers;

internal static class DistinguishedNameHelper
{
    internal static (string NewRdn, string NewParent) GetMoveTarget(Input input)
    {
        var source = Parse(input.SourceDistinguishedName, nameof(input.SourceDistinguishedName));
        var destination = Parse(input.DestinationDistinguishedName, nameof(input.DestinationDistinguishedName));
        var sourceDomain = GetDomain(source);
        var destinationDomain = GetDomain(destination);

        if (sourceDomain.Length == 0 || destinationDomain.Length == 0
            || sourceDomain.Length == source.RdNs.Count || destinationDomain.Length == destination.RdNs.Count)
        {
            throw new ArgumentException("Both distinguished names must identify an entry beneath a domain expressed with trailing DC components.");
        }

        if (!sourceDomain.SequenceEqual(destinationDomain, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("Source and destination distinguished names must belong to the same domain.");

        // Preserve the original escaping instead of reserializing the parser's unescaped values.
        var dn = input.DestinationDistinguishedName;
        var quoted = false;
        for (var index = 0; index < dn.Length; index++)
        {
            if (dn[index] == '\\')
                index++;
            else if (dn[index] == '"')
                quoted = !quoted;
            else if (!quoted && (dn[index] == ',' || dn[index] == ';'))
                return (dn[..index], dn[(index + 1)..]);
        }

        throw new ArgumentException("The destination distinguished name must include a parent container.");
    }

    private static Dn Parse(string value, string parameterName)
    {
        try
        {
            return new Dn(value);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException("A valid LDAP distinguished name is required.", parameterName, ex);
        }
    }

    private static string[] GetDomain(Dn dn)
    {
        return dn.RdNs.Reverse()
            .TakeWhile(rdn => !rdn.Multivalued && rdn.Type.Equals("DC", StringComparison.OrdinalIgnoreCase))
            .Select(rdn => rdn.Value)
            .ToArray();
    }
}
