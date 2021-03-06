using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Functions
{
    public class GetHttpTrigger
    {
        private readonly ILogger<GetHttpTrigger> logger;
        private readonly IDocumentService<AppRegistrationModel> documentService;

        public GetHttpTrigger(
           ILogger<GetHttpTrigger> logger,
           IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
        }

        [FunctionName("GetByPath")]
        [Display(Name = "Get app registration by Path", Description = "Retrieves a registered application for the specified path.")]
        [ProducesResponseType(typeof(AppRegistrationModel), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App Registration found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Nothing found for parameter", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "appregistry/{path}")] HttpRequest request,
            string path)
        {
            logger.LogInformation($"Get AppRegistration for: {path}");

            var appRegistrationModel = await documentService.GetAsync(d => d.Path == path).ConfigureAwait(false);

            if (appRegistrationModel != null)
            {
                logger.LogInformation($"Returning app registration for: {path}");
            }
            else
            {
                logger.LogWarning($"Failed to get app registration for: {path}");
            }

            return new OkObjectResult(appRegistrationModel);
        }
    }
}
