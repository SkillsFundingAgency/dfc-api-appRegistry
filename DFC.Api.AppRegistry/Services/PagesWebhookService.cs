using DFC.Compui.Subscriptions.Pkg.Data.Contracts;
using DFC.Compui.Subscriptions.Pkg.Data.Enums;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class PagesWebhookService : IWebhooksService
    {
        public Task<HttpStatusCode> DeleteContentAsync(Guid contentId)
        {
            throw new NotImplementedException();
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

        public Task<HttpStatusCode> ProcessMessageAsync(WebhookCacheOperation webhookCacheOperation, Guid eventId, Guid contentId, Uri url)
        {
            throw new NotImplementedException();
        }
    }
}
