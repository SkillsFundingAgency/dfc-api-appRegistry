using DFC.Api.AppRegistry.Contracts;
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
    public class LegacyDataLoadHttpTrigger
    {
        private readonly ILogger<LegacyDataLoadHttpTrigger> logger;
        private readonly ILegacyDataLoadService legacyDataLoadService;

        public LegacyDataLoadHttpTrigger(
           ILogger<LegacyDataLoadHttpTrigger> logger,
           ILegacyDataLoadService legacyDataLoadService)
        {
            this.logger = logger;
            this.legacyDataLoadService = legacyDataLoadService;
        }

        [FunctionName("LegacyDataLoad")]
        [Display(Name = "Legacy data load", Description = "Loads all legacy data into app registrations.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "App Registrations loaded", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "appregistry/LegacyDataLoad")] HttpRequest request)
        {
            logger.LogInformation("Loading all legacy data into app registrations");

            await legacyDataLoadService.LoadAsync().ConfigureAwait(false);

            logger.LogInformation("Loaded all legacy data into app registrations");

            return new OkResult();
        }
    }
}