using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DFC.Api.AppRegistry.Services
{
    public class ModelValidationService : IModelValidationService
    {
        private readonly ILogger<ModelValidationService> logger;

        public ModelValidationService(ILogger<ModelValidationService> logger)
        {
            this.logger = logger;
        }

        public bool ValidateModel(AppRegistrationModel? appRegistrationModel)
        {
            _ = appRegistrationModel ?? throw new ArgumentNullException(nameof(appRegistrationModel));

            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(appRegistrationModel, null, null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(appRegistrationModel, validationContext, validationResults, true);

            if (!isValid && validationResults.Any())
            {
                foreach (var validationResult in validationResults)
                {
                    logger.LogError($"Error validating {appRegistrationModel.Path}: {string.Join(",", validationResult.MemberNames)} - {validationResult.ErrorMessage}");
                }
            }

            return isValid;
        }
    }
}
