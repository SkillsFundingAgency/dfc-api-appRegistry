using DFC.Api.AppRegistry.Contracts;
using DFC.Compui.Subscriptions.Pkg.Data.Contracts;
using DFC.Compui.Subscriptions.Pkg.Data.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class PagesWebhookService : IWebhooksService
    {
        private readonly IPagesDataLoadService pagesDataLoadService;
        private readonly ILogger<PagesWebhookService> logger;

        public PagesWebhookService(IPagesDataLoadService pagesDataLoadService, ILogger<PagesWebhookService> logger)
        {
            this.pagesDataLoadService = pagesDataLoadService;
            this.logger = logger;
        }

        public async Task<HttpStatusCode> DeleteContentAsync(Guid contentId)
        {
            logger.LogInformation($"{nameof(DeleteContentAsync)} called in {nameof(PagesWebhookService)}");

            var result = await pagesDataLoadService.RemoveAsync(contentId).ConfigureAwait(false);
            return result;
        }

        public Task<HttpStatusCode> DeleteContentItemAsync(Guid contentItemId)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpStatusCode> ProcessContentAsync(Uri url, Guid contentId)
        {
            logger.LogInformation($"{nameof(ProcessContentAsync)} called in {nameof(PagesWebhookService)}  with Content Id {contentId}");

            var result = await pagesDataLoadService.CreateOrUpdateAsync(contentId).ConfigureAwait(false);
            return result;
        }

        public Task<HttpStatusCode> ProcessContentItemAsync(Uri url, Guid contentItemId)
        {
            throw new NotImplementedException();
        }

        public Task<HttpStatusCode> ProcessMessageAsync(WebhookCacheOperation webhookCacheOperation, Guid eventId, Guid contentId, Uri url)
        {
            throw new NotImplementedException();
        }
    }
}
