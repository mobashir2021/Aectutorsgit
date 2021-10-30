using FluentValidation;

namespace AECMIS.DAL.Validation
{
    public static class ValidationExtensions
    {
        public static void ValidateAndThrow(this IValidator validator, object instance)
        {
            //string ruleSet
            var result = validator.Validate(instance);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        public static bool ValidateOrThrow(this IValidator validator, object instance)
        {
            //string ruleSet
            var result = validator.Validate(instance);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }

            return result.IsValid;
        }

    }
}