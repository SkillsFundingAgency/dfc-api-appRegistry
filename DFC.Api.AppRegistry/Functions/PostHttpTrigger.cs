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
        [ProducesResponseType(typeof(AppRegistrationModel), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "App Registration created", ShowSchema = true)]
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

            try
            {
                logger.LogInformation($"Attempting to create app registration for: {body.Value.Path}");

                body.Value.Regions?.ForEach(f => f.DateOfRegistration = DateTime.UtcNow);
                body.Value.DateOfRegistration = DateTime.UtcNow;

                var statusCode = await documentService.UpsertAsync(body.Value).ConfigureAwait(false);

                switch (statusCode)
                {
                    case HttpStatusCode.Created:
                        logger.LogInformation($"Created app registration with Post for: {body.Value.Path}: Status code {statusCode}");
                        var appRegistrationModel = await documentService.GetAsync(p => p.Path == body.Value.Path).ConfigureAwait(false);
                        return new CreatedResult(appRegistrationModel!.Path, appRegistrationModel);
                    case HttpStatusCode.OK:
                        logger.LogInformation($"Upserted app registration with Post for: {body.Value.Path}: Status code {statusCode}");
                        return new OkResult();
                }

                logger.LogError($"Error creating app registration with Post for: {body.Value.Path}: Status code {statusCode}");
                return new UnprocessableEntityResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error creating app registration with Post for: {body.Value.Path}");
                return new BadRequestResult();
            }
        }
    }
}
