using DFC.Api.AppRegistry.Enums;
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
        private const string Description = "Ability to update specific values of an existing Region for an AppRegistation. <br>" +
                                           "<br><b>Validation Rules:</b> <br>" +
                                           "<br><b>Path:</b> Is mandatory <br>" +
                                           "<br><b>PageRegion:</b> Is mandatory <br>";

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
        [ProducesResponseType(typeof(AppRegistrationModel), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App Registration found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Nothing found for parameter", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "Region validation error(s)", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        [Display(Name = "Patch", Description = Description)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "appregistry/{path}/regions/{pageRegion}")] HttpRequest? request,
            string path,
            int pageRegion)
        {
            logger.LogInformation($"Validating patch AppRegistration for: {path}/{pageRegion}");

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

            if (!Enum.IsDefined(typeof(PageRegion), pageRegion))
            {
                logger.LogWarning($"Invalid PageRegion '{pageRegion}' received");
                return new BadRequestResult();
            }

            var pageRegionValue = (PageRegion)pageRegion;

            JsonPatchDocument<RegionModel>? regionModelPatch;
            try
            {
                using var streamReader = new StreamReader(request.Body);
                var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                regionModelPatch = JsonConvert.DeserializeObject<JsonPatchDocument<RegionModel>>(requestBody);

                if (regionModelPatch == null)
                {
                    logger.LogWarning("Request body is empty");
                    return new BadRequestResult();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deserialising patch document for: {path}/{pageRegionValue}");
                return new BadRequestResult();
            }

            logger.LogInformation($"Getting AppRegistration for: {path}");

            var appRegistrationModel = await documentService.GetAsync(d => d.Path == path).ConfigureAwait(false);

            if (appRegistrationModel == null)
            {
                logger.LogWarning($"No app registration exists for: {path}");
                return new NoContentResult();
            }

            var regionModel = appRegistrationModel.Regions.FirstOrDefault(f => f.PageRegion == pageRegionValue);

            if (regionModel == null)
            {
                logger.LogWarning($"No Region exists for: {path}/{pageRegionValue}");
                return new NoContentResult();
            }

            logger.LogInformation($"Patching AppRegistration for: {path}/{pageRegionValue}");

            try
            {
                logger.LogInformation($"Attempting to apply patch to: {path}/{pageRegionValue}");
                regionModelPatch?.ApplyTo(regionModel);

                var validationResults = regionModel.Validate(new ValidationContext(regionModel));
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
                logger.LogError(ex, $"Error applying patch to region model for: {path}/{pageRegionValue}");
                return new BadRequestResult();
            }

            try
            {
                logger.LogInformation($"Attempting to update app registration for: {path}/{pageRegionValue}");

                var statusCode = await documentService.UpsertAsync(appRegistrationModel).ConfigureAwait(false);

                logger.LogInformation($"Updated app registration with patch for: {path}/{pageRegionValue}: Status code {statusCode}");
                return new OkResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating app registration with patch for: {path}/{pageRegionValue}");
                return new BadRequestResult();
            }
        }
    }
}
