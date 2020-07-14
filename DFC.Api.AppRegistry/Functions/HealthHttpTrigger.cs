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
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Functions
{
    public class HealthHttpTrigger
    {
        private const string SuccessMessage = "Document store is available";
        private readonly ILogger<HealthHttpTrigger> logger;
        private readonly IDocumentService<AppRegistrationModel> documentService;
        private readonly string? resourceName;

        public HealthHttpTrigger(ILogger<HealthHttpTrigger> logger, IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
            resourceName = typeof(HealthHttpTrigger)?.Namespace;
        }

        [FunctionName("app-registry-health")]
        [Display(Name = "App registry API Health Check", Description = "App registry API Health Check")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App registry API Health Check.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.ServiceUnavailable, Description = "App registry API Health failed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Health(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest request)
        {
            logger.LogInformation($"{nameof(Health)} has been called");
            try
            {
                var isHealthy = !(documentService is null) && await documentService.PingAsync().ConfigureAwait(false);
                if (isHealthy)
                {
                    logger.LogInformation($"{nameof(Health)} responded with: {resourceName} - {SuccessMessage}");
                    return new OkResult();
                }

                logger.LogError($"{nameof(Health)}: Ping to {resourceName} has failed");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"{nameof(Health)}: {resourceName} exception: {ex.Message}");
            }

            return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
        }
    }
}
