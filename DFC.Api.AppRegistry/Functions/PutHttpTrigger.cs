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

        public PutHttpTrigger(
           ILogger<PutHttpTrigger> logger,
           IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
        }

        [FunctionName("Put")]
        [Display(Name = "Put an app registration", Description = "Updates a resource of type 'AppRegistry'.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App Registration updated", ShowSchema = true)]
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

            var body = await request.GetBodyAsync<AppRegistrationModel>().ConfigureAwait(false);

            if (body?.Value == null)
            {
                logger.LogWarning($"Request.Body is malformed");
                return new BadRequestResult();
            }

            if (!body.IsValid)
            {
                if (body.ValidationResults != null && body.ValidationResults.Any())
                {
                    logger.LogWarning($"Validation Failed with {body.ValidationResults.Count()} errors");
                    foreach (var validationResult in body.ValidationResults)
                    {
                        logger.LogWarning($"Validation Failed: {validationResult.ErrorMessage}: {string.Join(",", validationResult.MemberNames)}");
                    }
                }

                return new BadRequestResult();
            }

            if (!string.Equals(path, body.Value.Path, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning($"Path parameter ({path}) does not match request value Path ({body.Value.Path})");
                return new BadRequestResult();
            }

            logger.LogInformation($"Attempting to get app registration for: {path}");
            var currentAppRegistration = await documentService.GetAsync(p => p.Path == path).ConfigureAwait(false);
            if (currentAppRegistration == null)
            {
                logger.LogWarning($"No app registration exists for path: {path}");
                return new NoContentResult();
            }

            try
            {
                logger.LogInformation($"Attempting to update app registration for: {body.Value.Path}");

                body.Value.Regions?.ForEach(f => f.LastModifiedDate = DateTime.UtcNow);
                body.Value.LastModifiedDate = DateTime.UtcNow;

                var statusCode = await documentService.UpsertAsync(body.Value).ConfigureAwait(false);

                if (statusCode == HttpStatusCode.OK)
                {
                    logger.LogInformation($"Upserted app registration with Put for: {body.Value.Path}: Status code {statusCode}");
                    return new OkResult();
                }

                logger.LogError($"Error updating app registration with Put for: {body.Value.Path}: Status code {statusCode}");
                return new UnprocessableEntityResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating app registration with Put for: {body.Value.Path}");
                return new BadRequestResult();
            }
        }
    }
}
