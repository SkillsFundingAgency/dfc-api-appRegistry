using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace DFC.Api.AppRegistry.Functions
{
    public class PingHttpTrigger
    {
        private readonly ILogger<PingHttpTrigger> logger;

        public PingHttpTrigger(ILogger<PingHttpTrigger> logger)
        {
            this.logger = logger;
        }

        [FunctionName("app-registry-ping")]
        [Display(Name = "Ping App registry API", Description = "Pings App registry API")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App registry Ping.", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is invalid.", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public IActionResult Ping([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/ping")] HttpRequest request)
        {
            logger.LogInformation($"{nameof(Ping)} has been called");
            return new OkResult();
        }
    }
}
