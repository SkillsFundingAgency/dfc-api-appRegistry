using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Functions
{
    public class GetListHttpTrigger
    {
        private readonly ILogger<GetListHttpTrigger> logger;
        private readonly IDocumentService<AppRegistrationModel> documentService;

        public GetListHttpTrigger(
           ILogger<GetListHttpTrigger> logger,
           IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
        }

        [FunctionName("GetList")]
        [Display(Name = "Get all app registrations", Description = "Retrieves a list of all registered applications.")]
        [ProducesResponseType(typeof(AppRegistrationModel), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App Registration found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Nothing found", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "appregistry/")] HttpRequest request)
        {
            logger.LogInformation("Getting all app registrations");

            var appRegistrationModels = await documentService.GetAllAsync().ConfigureAwait(false);

            if (appRegistrationModels != null && appRegistrationModels.Any())
            {
                logger.LogInformation($"Returning {appRegistrationModels.Count()} app registrations");

                return new OkObjectResult(appRegistrationModels.OrderBy(o => o.Path));
            }

            logger.LogWarning("Failed to get any app registrations");

            return new NoContentResult();
        }
    }
}
