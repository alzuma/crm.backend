using System.ComponentModel.DataAnnotations;

namespace Crm.Backend.Validation;

/// <summary>Runs the data annotation validation on an incoming DTO. This is
/// the designated place where incoming data is validated; the endpoint layer
/// calls it before handing the data to the application layer.</summary>
public static class DtoValidator
{
    public static bool TryValidate<T>(T value, out IReadOnlyDictionary<string, string[]> errors)
        where T : class
    {
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true))
        {
            errors = new Dictionary<string, string[]>();
            return true;
        }

        errors = results
            .GroupBy(static result => result.MemberNames.FirstOrDefault() ?? "model")
            .ToDictionary(
                group => group.Key,
                group => group.Select(static result => result.ErrorMessage ?? "Invalid value.").ToArray());
        return false;
    }
}
