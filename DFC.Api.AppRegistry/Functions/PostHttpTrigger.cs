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
    public class PostHttpTrigger
    {
        private readonly ILogger<PostHttpTrigger> logger;
        private readonly IDocumentService<AppRegistrationModel> documentService;

        public PostHttpTrigger(
           ILogger<PostHttpTrigger> logger,
           IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
        }

        [FunctionName("Post")]
        [Display(Name = "Post an app registration", Description = "Creates a new resource of type 'AppRegistry'.")]
        [ProducesResponseType(typeof(AppRegistrationModel), (int)HttpStatusCode.Created)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "App Registration created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App Registration updated", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.UnprocessableEntity, Description = "AppRegistry validation error(s)", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "appregistry/")] HttpRequest? request)
        {
            logger.LogInformation("Validating Post AppRegistration for");

            if (request == null)
            {
                logger.LogWarning($"Missing request.Body");
                return new BadRequestResult();
            }

            var appRegistrationModel = await request.GetModelFromBodyAsync<AppRegistrationModel>(logger).ConfigureAwait(false);

            if (appRegistrationModel == null)
            {
                logger.LogWarning($"Request.Body is malformed");
                return new BadRequestResult();
            }

            try
            {
                logger.LogInformation($"Attempting to create app registration for: {appRegistrationModel.Path}");

                appRegistrationModel.Id = Guid.NewGuid();
                appRegistrationModel.Regions?.ForEach(f => f.DateOfRegistration = DateTime.UtcNow);
                appRegistrationModel.DateOfRegistration = DateTime.UtcNow;

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

                var statusCode = await documentService.UpsertAsync(appRegistrationModel).ConfigureAwait(false);

                switch (statusCode)
                {
                    case HttpStatusCode.Created:
                        logger.LogInformation($"Created app registration with Post for: {appRegistrationModel.Path}: Status code {statusCode}");
                        var resultModel = await documentService.GetAsync(p => p.Path == appRegistrationModel.Path).ConfigureAwait(false);
                        return new CreatedResult(appRegistrationModel.Path, resultModel.FirstOrDefault());
                    case HttpStatusCode.OK:
                        logger.LogInformation($"Upserted app registration with Post for: {appRegistrationModel.Path}: Status code {statusCode}");
                        return new OkResult();
                }

                logger.LogError($"Error creating app registration with Post for: {appRegistrationModel.Path}: Status code {statusCode}");
                return new UnprocessableEntityResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error creating app registration with Post for: {appRegistrationModel.Path}");
                return new UnprocessableEntityResult();
            }
        }
    }
}
