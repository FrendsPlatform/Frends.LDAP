using System;
using System.ComponentModel.DataAnnotations;

namespace Frends.LDAP.MoveUser.Definitions;

/// <summary>
/// Validates that a host is a single hostname or IP address without a scheme or port.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class LdapHostAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LdapHostAttribute"/> class.
    /// </summary>
    public LdapHostAttribute()
        : base("Host must be a single LDAP server hostname or IP address, without a scheme or port.")
    {
    }

    /// <inheritdoc/>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null ||
            (value is string host && string.IsNullOrWhiteSpace(host)) ||
            (value is string hostValue && Uri.CheckHostName(hostValue) != UriHostNameType.Unknown))
            return ValidationResult.Success;

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }
}
