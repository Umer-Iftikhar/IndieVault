using System.ComponentModel.DataAnnotations;

namespace IndieVault.Tests.TestHelpers
{
    // Runs DataAnnotations validation the same way MVC model binding does.
    public static class ModelValidator
    {
        public static IList<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }
    }
}
