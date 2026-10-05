using System;
using System.ComponentModel.DataAnnotations;

namespace Frends.LDAP.MoveUser.Definitions;

/// <summary>
/// Validates that LDAPs and StartTLS are not both enabled.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class ExclusiveEncryptionAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExclusiveEncryptionAttribute"/> class.
    /// </summary>
    public ExclusiveEncryptionAttribute()
        : base("SecureSocketLayer and TLS cannot both be enabled.")
    {
    }

    /// <inheritdoc/>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        return value is Connection { SecureSocketLayer: true, TLS: true }
            ? new ValidationResult(FormatErrorMessage(validationContext.DisplayName))
            : ValidationResult.Success;
    }
}
