using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Extensions;
using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Functions
{
    public class CdnUpdatePostHttpTrigger
    {
        private readonly ILogger<CdnUpdatePostHttpTrigger> logger;
        private readonly IDocumentService<AppRegistrationModel> documentService;
        private readonly IUpdateScriptHashCodeService updateScriptHashCodes;

        public CdnUpdatePostHttpTrigger(
           ILogger<CdnUpdatePostHttpTrigger> logger,
           IDocumentService<AppRegistrationModel> documentService,
           IUpdateScriptHashCodeService updateScriptHashCodes)
        {
            this.logger = logger;
            this.documentService = documentService;
            this.updateScriptHashCodes = updateScriptHashCodes;
        }

        [FunctionName("PostCdn")]
        [Display(Name = "Post CDN update to the Shell's app registration", Description = "Updates a resource of type 'AppRegistry'.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App Registration updated", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Path does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "AppRegistry validation error(s)", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "appregistry/shell/cdn")] HttpRequest? request)
        {
            const string path = "shell";

            logger.LogInformation("Validating Post CDN update");

            if (request == null)
            {
                logger.LogWarning($"Missing request.Body");
                return new BadRequestResult();
            }

            var cdnPostModel = await request.GetModelFromBodyAsync<CdnPostModel>().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(cdnPostModel?.Cdn))
            {
                logger.LogWarning($"Request.Body is malformed");
                return new BadRequestResult();
            }

            logger.LogInformation($"Attempting to get app registration for: {path}");
            var existingAppRegistrations = await documentService.GetAsync(p => p.Path == path).ConfigureAwait(false);
            if (existingAppRegistrations == null || !existingAppRegistrations.Any())
            {
                logger.LogWarning($"No app registration exists for path: {path}");
                return new NoContentResult();
            }

            try
            {
                logger.LogInformation($"Attempting to update app registration for: {path}");

                var appRegistrationModel = existingAppRegistrations.First();
                appRegistrationModel.CdnLocation = cdnPostModel.Cdn;
                appRegistrationModel.LastModifiedDate = DateTime.UtcNow;

                var statusCode = await documentService.UpsertAsync(appRegistrationModel).ConfigureAwait(false);

                if (statusCode == HttpStatusCode.OK)
                {
                    logger.LogInformation($"Upserted app registration with Post for: {appRegistrationModel.Path}: Status code {statusCode}");
 
                    updateScriptHashCodes.CdnLocation = appRegistrationModel.CdnLocation;

                    var statusCodeHashcodeUpdate = await updateScriptHashCodes.UpdateAllAsync().ConfigureAwait(false);

                    if (statusCodeHashcodeUpdate == HttpStatusCode.OK || statusCodeHashcodeUpdate == HttpStatusCode.NoContent)
                    {
                        logger.LogInformation($"Updated app registration script hash codes with Post for: {appRegistrationModel.Path}: Status code {statusCodeHashcodeUpdate}");

                        return new OkResult();
                    }
                }

                logger.LogError($"Error updating app registration with Post for: {appRegistrationModel.Path}: Status code {statusCode}");
                return new UnprocessableEntityResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating app registration with Post for: {path}");
                return new UnprocessableEntityResult();
            }
        }
    }
}
