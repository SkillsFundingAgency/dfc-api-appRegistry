using DFC.Api.AppRegistry.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Functions
{
    public class PagesDataLoadHttpTrigger
    {
        private readonly ILogger<PagesDataLoadHttpTrigger> logger;
        private readonly IPagesDataLoadService pagesDataLoadService;

        public PagesDataLoadHttpTrigger(
           ILogger<PagesDataLoadHttpTrigger> logger,
           IPagesDataLoadService pagesDataLoadService)
        {
            this.logger = logger;
            this.pagesDataLoadService = pagesDataLoadService;
        }

        [FunctionName("PagesDataLoad")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Pages loaded", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 429, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        [Display(Name = "PagesDataLoad", Description = "Loads pages data into the pages app registration.")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "pages/")] HttpRequest request)
        {
            logger.LogInformation("Loading all pages data into app registrations");

            await pagesDataLoadService.LoadAsync().ConfigureAwait(false);

            logger.LogInformation("Loaded all pages data into app registrations");

            return new OkResult();
        }
    }
}

