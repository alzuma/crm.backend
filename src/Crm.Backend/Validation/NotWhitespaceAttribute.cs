using System.ComponentModel.DataAnnotations;

namespace Crm.Backend.Validation;

/// <summary>Custom validation example: rejects values that are empty or
/// whitespace only, which plain [Required] would accept. Custom validation
/// rules for incoming data live in this folder.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NotWhitespaceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        => value is null || value.ToString()!.Trim().Length == 0
            ? new ValidationResult("The value must not be empty or whitespace.")
            : ValidationResult.Success;
}
