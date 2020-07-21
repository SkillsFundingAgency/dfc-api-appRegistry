using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Functions
{
    public class PatchHttpTrigger
    {
        private const string Description = "Ability to update specific values of an AppRegistation. <br>" +
                                           "<br><b>Validation Rules:</b> <br>" +
                                           "<br><b>Path:</b> Is mandatory <br>";

        private readonly ILogger<PatchHttpTrigger> logger;
        private readonly IDocumentService<AppRegistrationModel> documentService;

        public PatchHttpTrigger(
           ILogger<PatchHttpTrigger> logger,
           IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
        }

        [FunctionName("Patch")]
        [Display(Name = "Patch an app registration", Description = Description)]
        [ProducesResponseType(typeof(AppRegistrationModel), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App Registration patched", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Nothing found for parameter", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "App Registration validation error(s)", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "appregistry/{path}")] HttpRequest? request,
            string path)
        {
            logger.LogInformation($"Validating patch AppRegistration for: {path}");

            if (request?.Body == null)
            {
                logger.LogWarning($"Missing request.Body in request");
                return new BadRequestResult();
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                logger.LogWarning($"Missing value in request for '{nameof(path)}'");
                return new BadRequestResult();
            }

            JsonPatchDocument<AppRegistrationModel>? appRegistrationModelPatch;
            try
            {
                using var streamReader = new StreamReader(request.Body);
                var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                appRegistrationModelPatch = JsonConvert.DeserializeObject<JsonPatchDocument<AppRegistrationModel>>(requestBody);

                if (appRegistrationModelPatch == null)
                {
                    logger.LogWarning("Request body is empty");
                    return new BadRequestResult();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deserialising patch document for: {path}");
                return new BadRequestResult();
            }

            logger.LogInformation($"Getting AppRegistration for: {path}");

            var appRegistrationModels = await documentService.GetAsync(d => d.Path == path).ConfigureAwait(false);

            if (appRegistrationModels == null)
            {
                logger.LogWarning($"No app registration exists for: {path}");
                return new NoContentResult();
            }

            var appRegistrationModel = appRegistrationModels.FirstOrDefault();

            logger.LogInformation($"Patching AppRegistration for: {path}");

            try
            {
                logger.LogInformation($"Attempting to apply patch to: {path}");
                appRegistrationModelPatch?.ApplyTo(appRegistrationModel);

                var validationResults = appRegistrationModel.Validate(new ValidationContext(appRegistrationModel));
                if (validationResults != null && validationResults.Any())
                {
                    logger.LogWarning($"Validation Failed with {validationResults.Count()} errors");
                    foreach (var validationResult in validationResults)
                    {
                        logger.LogWarning($"Validation Failed: {validationResult.ErrorMessage}: {string.Join(",", validationResult.MemberNames)}");
                    }

                    return new UnprocessableEntityResult();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error applying patch to app registration model for: {path}");
                return new BadRequestResult();
            }

            try
            {
                logger.LogInformation($"Attempting to update app registration for: {path}");

                appRegistrationModel.LastModifiedDate = DateTime.UtcNow;

                var statusCode = await documentService.UpsertAsync(appRegistrationModel).ConfigureAwait(false);

                logger.LogInformation($"Updated app registration with patch for: {path}: Status code {statusCode}");
                return new OkObjectResult(appRegistrationModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating app registration with patch for: {path}");
                return new UnprocessableEntityResult();
            }
        }
    }
}
