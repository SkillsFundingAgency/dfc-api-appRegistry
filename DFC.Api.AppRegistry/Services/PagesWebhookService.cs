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

        public async Task<HttpStatusCode> ProcessMessageAsync(WebhookCacheOperation webhookCacheOperation, Guid eventId, Guid contentId, Uri url)
        {
            logger.LogInformation($"{nameof(ProcessMessageAsync)} called in {nameof(PagesWebhookService)}");

            switch (webhookCacheOperation)
            {
                case WebhookCacheOperation.CreateOrUpdate:
                    logger.LogInformation($"{nameof(WebhookCacheOperation.CreateOrUpdate)} called in {nameof(PagesWebhookService)}  with Content Id {contentId}");
                    return await pagesDataLoadService.CreateOrUpdateAsync(contentId).ConfigureAwait(false);
                case WebhookCacheOperation.Delete:
                    logger.LogInformation($"{nameof(WebhookCacheOperation.Delete)} called in {nameof(PagesWebhookService)} with Content Id {contentId}");
                    return await pagesDataLoadService.RemoveAsync(contentId).ConfigureAwait(false);
                default:
                    return HttpStatusCode.OK;
            }
        }
    }
}
