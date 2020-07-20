using DFC.Api.AppRegistry.Contracts;
using DFC.Compui.Subscriptions.Pkg.Data.Contracts;
using DFC.Compui.Subscriptions.Pkg.Data.Enums;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class PagesWebhookService : IWebhooksService
    {
        private readonly IPagesDataLoadService pagesDataLoadService;

        public PagesWebhookService(IPagesDataLoadService pagesDataLoadService)
        {
            this.pagesDataLoadService = pagesDataLoadService;
        }

        public async Task<HttpStatusCode> DeleteContentAsync(Guid contentId)
        {
            var result = await pagesDataLoadService.RemoveAsync(contentId).ConfigureAwait(false);
            return result;
        }

        public Task<HttpStatusCode> DeleteContentItemAsync(Guid contentItemId)
        {
            throw new NotImplementedException();
        }

        public Task<HttpStatusCode> ProcessContentAsync(Uri url, Guid contentId)
        {
            throw new NotImplementedException();
        }

        public Task<HttpStatusCode> ProcessContentItemAsync(Uri url, Guid contentItemId)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpStatusCode> ProcessMessageAsync(WebhookCacheOperation webhookCacheOperation, Guid eventId, Guid contentId, Uri url)
        {
            //Process the message here
            if (webhookCacheOperation == WebhookCacheOperation.CreateOrUpdate)
            {
                var result = await pagesDataLoadService.CreateOrUpdateAsync(contentId).ConfigureAwait(false);
                return result;
            }

            return HttpStatusCode.OK;
        }
    }
}
