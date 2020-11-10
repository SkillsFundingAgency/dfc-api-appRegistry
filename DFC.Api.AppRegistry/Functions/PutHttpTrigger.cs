using DFC.Api.AppRegistry.Common;
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
    public class PutHttpTrigger
    {
        private readonly ILogger<PutHttpTrigger> logger;
        private readonly IDocumentService<AppRegistrationModel> documentService;
        private readonly IUpdateScriptHashCodeService updateScriptHashCodeService;

        public PutHttpTrigger(
           ILogger<PutHttpTrigger> logger,
           IDocumentService<AppRegistrationModel> documentService,
           IUpdateScriptHashCodeService updateScriptHashCodeService)
        {
            this.logger = logger;
            this.documentService = documentService;
            this.updateScriptHashCodeService = updateScriptHashCodeService;
        }

        [FunctionName("Put")]
        [Display(Name = "Put an app registration", Description = "Updates a resource of type 'AppRegistry'.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App Registration updated", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Path does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "AppRegistry validation error(s)", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "appregistry/{path}")] HttpRequest? request, string path)
        {
            logger.LogInformation("Validating Put AppRegistration for");

            if (request == null)
            {
                logger.LogWarning($"Missing request.Body");
                return new BadRequestResult();
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                logger.LogWarning($"Missing value in request for '{nameof(path)}'");
                return new BadRequestResult();
            }

            var appRegistrationModel = await request.GetModelFromBodyAsync<AppRegistrationModel>().ConfigureAwait(false);

            if (appRegistrationModel == null)
            {
                logger.LogWarning($"Request.Body is malformed");
                return new BadRequestResult();
            }

            if (!string.Equals(path, appRegistrationModel.Path, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning($"Path parameter ({path}) does not match request value Path ({appRegistrationModel.Path})");
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
                logger.LogInformation($"Attempting to update app registration for: {appRegistrationModel.Path}");

                var existingAppRegistration = existingAppRegistrations.First();
                appRegistrationModel.Id = existingAppRegistration.Id;
                appRegistrationModel.Etag = existingAppRegistration.Etag;
                appRegistrationModel.CdnLocation = existingAppRegistration.CdnLocation;
                appRegistrationModel.DateOfRegistration = existingAppRegistration.DateOfRegistration;
                appRegistrationModel.PageLocations = existingAppRegistration.PageLocations;
                appRegistrationModel.Regions?.ForEach(f => f.LastModifiedDate = DateTime.UtcNow);
                appRegistrationModel.Regions?.ForEach(f => f.DateOfRegistration = existingAppRegistration.Regions.FirstOrDefault(r => r.PageRegion == f.PageRegion)?.DateOfRegistration);
                appRegistrationModel.AjaxRequests?.ForEach(f => f.LastModifiedDate = DateTime.UtcNow);
                appRegistrationModel.AjaxRequests?.ForEach(f => f.DateOfRegistration = existingAppRegistration.AjaxRequests.FirstOrDefault(r => r.Name == f.Name)?.DateOfRegistration);
                appRegistrationModel.LastModifiedDate = DateTime.UtcNow;

                if (!appRegistrationModel.Validate(logger))
                {
                    return new UnprocessableEntityResult();
                }

                var shellAppRegistrations = await documentService.GetAsync(p => p.Path == Constants.PathNameForShell).ConfigureAwait(false);
                if (shellAppRegistrations != null && shellAppRegistrations.Any() && !string.IsNullOrWhiteSpace(shellAppRegistrations.FirstOrDefault()?.CdnLocation))
                {
                    await updateScriptHashCodeService.RefreshHashcodesAsync(appRegistrationModel, shellAppRegistrations.First().CdnLocation).ConfigureAwait(false);
                }

                var statusCode = await documentService.UpsertAsync(appRegistrationModel).ConfigureAwait(false);

                if (statusCode == HttpStatusCode.OK)
                {
                    logger.LogInformation($"Upserted app registration with Put for: {appRegistrationModel.Path}: Status code {statusCode}");
                    return new OkResult();
                }

                logger.LogError($"Error updating app registration with Put for: {appRegistrationModel.Path}: Status code {statusCode}");
                return new UnprocessableEntityResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating app registration with Put for: {appRegistrationModel.Path}");
                return new UnprocessableEntityResult();
            }
        }
    }
}
