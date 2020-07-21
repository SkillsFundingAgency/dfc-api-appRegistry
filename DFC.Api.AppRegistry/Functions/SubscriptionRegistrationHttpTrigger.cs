using DFC.Compui.Subscriptions.Pkg.NetStandard.Data.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace DFC.Api.AppRegistry.Functions
{
    public class SubscriptionRegistrationHttpTrigger
    {
        private readonly ILogger<SubscriptionRegistrationHttpTrigger> logger;
        private readonly ISubscriptionRegistrationService subscriptionRegistrationService;

        public SubscriptionRegistrationHttpTrigger(
           ILogger<SubscriptionRegistrationHttpTrigger> logger,
           ISubscriptionRegistrationService subscriptionRegistrationService)
        {
            this.logger = logger;
            this.subscriptionRegistrationService = subscriptionRegistrationService;
        }

        [FunctionName("SubscriptionRegistration")]
        [Display(Name = "Register Event Grid Subscription", Description = "Registered an Event Grid Subscription to an Event Grid Topic")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscription Registration updated", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "SubscriptionRegistration error(s)", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscription/")] HttpRequest? request)
        {
            logger.LogInformation("Request received for SubscriptionRegistration");

            try
            {
                await subscriptionRegistrationService.RegisterSubscription("DFC-Api-AppRegistry").ConfigureAwait(false);
                return new OkResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error creating SubscriptionRegistration");
                return new InternalServerErrorResult();
            }
        }
    }
}
