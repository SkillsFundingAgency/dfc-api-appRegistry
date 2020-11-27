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
    public class PatchAjaxRequestHttpTrigger
    {
        private const string Description = "Ability to update specific values of an existing AjaxRequest for an AppRegistation. <br>" +
                                           "<br><b>Validation Rules:</b> <br>" +
                                           "<br><b>Path:</b> Is mandatory <br>" +
                                           "<br><b>Name:</b> Is mandatory <br>";

        private readonly ILogger<PatchAjaxRequestHttpTrigger> logger;
        private readonly IDocumentService<AppRegistrationModel> documentService;

        public PatchAjaxRequestHttpTrigger(
           ILogger<PatchAjaxRequestHttpTrigger> logger,
           IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
        }

        [FunctionName("PatchAjaxRequest")]
        [Display(Name = "Patch an AjaxRequest", Description = Description)]
        [ProducesResponseType(typeof(AjaxRequestModel), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "AjaxRequest patched", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Nothing found for parameter", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "AjaxRequest validation error(s)", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "appregistry/{path}/ajaxrequests/{name}")] HttpRequest? request,
            string path,
            string name)
        {
            logger.LogInformation($"Validating patch AppRegistration for: {path}/{name}");

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

            if (string.IsNullOrWhiteSpace(name))
            {
                logger.LogWarning($"Missing value in request for '{nameof(name)}'");
                return new BadRequestResult();
            }

            JsonPatchDocument<AjaxRequestModel>? ajaxRequestModelPatch;
            try
            {
                using var streamReader = new StreamReader(request.Body);
                var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                ajaxRequestModelPatch = JsonConvert.DeserializeObject<JsonPatchDocument<AjaxRequestModel>>(requestBody);

                if (ajaxRequestModelPatch == null)
                {
                    logger.LogWarning("Request body is empty");
                    return new BadRequestResult();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deserialising patch document for: {path}/{name}");
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
            var ajaxRequestModel = appRegistrationModel.AjaxRequests.FirstOrDefault(f => string.Compare(f.Name, name, StringComparison.OrdinalIgnoreCase) == 0);

            if (ajaxRequestModel == null)
            {
                logger.LogWarning($"No AjaxRequest exists for: {path}/{name}");
                return new NoContentResult();
            }

            logger.LogInformation($"Patching AppRegistration for: {path}/{name}");

            try
            {
                logger.LogInformation($"Attempting to apply patch to: {path}/{name}");
                ajaxRequestModelPatch?.ApplyTo(ajaxRequestModel);

                var validationResults = ajaxRequestModel.Validate(new ValidationContext(ajaxRequestModel));
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
                logger.LogError(ex, $"Error applying patch to ajaxRequest model for: {path}/{name}");
                return new BadRequestResult();
            }

            try
            {
                logger.LogInformation($"Attempting to update app registration for: {path}/{name}");

                ajaxRequestModel.LastModifiedDate = DateTime.UtcNow;
                appRegistrationModel.LastModifiedDate = DateTime.UtcNow;

                var statusCode = await documentService.UpsertAsync(appRegistrationModel).ConfigureAwait(false);

                logger.LogInformation($"Updated app registration with patch for: {path}/{name}: Status code {statusCode}");
                return new OkObjectResult(ajaxRequestModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating app registration with patch for: {path}/{name}");
                return new UnprocessableEntityResult();
            }
        }
    }
}
