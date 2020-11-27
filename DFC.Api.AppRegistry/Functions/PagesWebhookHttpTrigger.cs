using DFC.Compui.Subscriptions.Pkg.Data.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace DFC.Api.AppRegistry.Functions
{
    public class PagesWebhookHttpTrigger
    {
        private readonly ILogger<PagesWebhookHttpTrigger> logger;
        private readonly IWebhookReceiver webhookReceiver;

        public PagesWebhookHttpTrigger(
           ILogger<PagesWebhookHttpTrigger> logger,
           IWebhookReceiver webhookReceiver)
        {
            this.logger = logger;
            this.webhookReceiver = webhookReceiver;
        }

        [FunctionName("PagesWebhook")]
        [Display(Name = "Pages Webhook", Description = "Receives webhook Post requests for Pages.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Page processed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "pages/webhook")] HttpRequest? request)
        {
            try
            {
                logger.LogInformation("Received webhook request");

                if (request == null)
                {
                    logger.LogError($"{nameof(request)} is null");
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                using var streamReader = new StreamReader(request.Body);
                var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(requestBody))
                {
                    logger.LogError($"{nameof(request)} body is null");
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                var result = await webhookReceiver.ReceiveEvents(requestBody).ConfigureAwait(false);

                logger.LogInformation("Webhook request completed");

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}