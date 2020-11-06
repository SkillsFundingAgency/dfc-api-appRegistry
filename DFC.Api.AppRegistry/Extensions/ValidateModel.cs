using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DFC.Api.AppRegistry.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ValidateModel
    {
        public static bool Validate<TModel>(this TModel model, ILogger logger)
            where TModel : IValidatableObject
        {
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(model, new ValidationContext(model, null, null), validationResults, true))
            {
                if (validationResults.Any())
                {
                    logger.LogWarning($"Validation Failed with {validationResults.Count} errors");
                    foreach (var validationResult in validationResults)
                    {
                        logger.LogWarning($"Validation Failed: {validationResult.ErrorMessage}: {string.Join(",", validationResult.MemberNames)}");
                    }
                }

                return false;
            }

            return true;
        }
    }
}
