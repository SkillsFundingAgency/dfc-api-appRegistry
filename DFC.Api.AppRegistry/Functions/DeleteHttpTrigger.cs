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
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Functions
{
    public class DeleteHttpTrigger
    {
        private readonly ILogger<DeleteHttpTrigger> logger;
        private readonly IDocumentService<AppRegistrationModel> documentService;

        public DeleteHttpTrigger(
           ILogger<DeleteHttpTrigger> logger,
           IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
        }

        [FunctionName("Delete")]
        [Display(Name = "Delete an app registration", Description = "Deletes a resource of type 'AppRegistry'.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App Registration deleted", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Path does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "appregistry/{path}")] HttpRequest? request, string path)
        {
            logger.LogInformation("Validating Delete AppRegistration for");

            if (string.IsNullOrWhiteSpace(path))
            {
                logger.LogWarning($"Missing value in request for '{nameof(path)}'");
                return new BadRequestResult();
            }

            logger.LogInformation($"Attempting to get app registration for: {path}");
            var appRegistrationModel = await documentService.GetAsync(p => p.Path == path).ConfigureAwait(false);
            if (appRegistrationModel == null)
            {
                logger.LogWarning($"No app registration exists for path: {path}");
                return new NoContentResult();
            }

            try
            {
                logger.LogInformation($"Attempting to delete app registration for: {appRegistrationModel.Path}");

                var result = await documentService.DeleteAsync(appRegistrationModel.Id).ConfigureAwait(false);

                if (result)
                {
                    logger.LogInformation($"Deleted app registration for: {appRegistrationModel.Path}");
                    return new OkResult();
                }

                logger.LogError($"Error deleting app registration for: {appRegistrationModel.Path}");
                return new UnprocessableEntityResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting app registration with Delete for: {appRegistrationModel.Path}");
                return new UnprocessableEntityResult();
            }
        }
    }
}
