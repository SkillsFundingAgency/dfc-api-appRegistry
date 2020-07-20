using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models.ClientOptions;
using DFC.Api.AppRegistry.Models.Pages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class PagesDataLoadService : IPagesDataLoadService
    {
        private readonly ILogger<PagesDataLoadService> logger;
        private readonly HttpClient httpClient;
        private readonly PagesClientOptions pagesClientOptions;
        private readonly IApiDataService apiDataService;
        private readonly ILegacyDataLoadService legacyDataLoadService;

        public PagesDataLoadService(
            ILogger<PagesDataLoadService> logger,
            HttpClient httpClient,
            PagesClientOptions pagesClientOptions,
            IApiDataService apiDataService,
            ILegacyDataLoadService legacyDataLoadService)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.pagesClientOptions = pagesClientOptions;
            this.apiDataService = apiDataService;
            this.legacyDataLoadService = legacyDataLoadService;
        }

        public async Task<HttpStatusCode> CreateOrUpdateAsync(Guid contentId)
        {
            logger.LogInformation($"Load Page {contentId} into App Registration Started");

            var appRegistration = await legacyDataLoadService.GetAppRegistrationByPathAsync("pages").ConfigureAwait(false);

            if (appRegistration == null)
            {
                return HttpStatusCode.NotFound;
            }

            var page = await GetPageAsync(contentId.ToString()).ConfigureAwait(false);

            if (page == null)
            {
                logger.LogError($"Page {contentId} returned null from Pages API");
                return HttpStatusCode.NotFound;
            }

            if (!appRegistration.Locations.Any(x => x == page.Url!.ToString()))
            {
                appRegistration.Locations!.Add(page.Url!.ToString());
                appRegistration.LastModifiedDate = DateTime.UtcNow;
                await legacyDataLoadService.UpdateAppRegistrationAsync(appRegistration).ConfigureAwait(false);
            }

            return HttpStatusCode.OK;
        }

        public async Task LoadAsync()
        {
            logger.LogInformation($"Load Pages into App Registration Started");

            var appRegistration = await legacyDataLoadService.GetAppRegistrationByPathAsync("pages").ConfigureAwait(false);

            if (appRegistration == null)
            {
                return;
            }

            var pages = await GetPagesAsync().ConfigureAwait(false);

            if (pages == null)
            {
                logger.LogInformation($"No pages returned from {nameof(GetPagesAsync)}");
                return;
            }

            appRegistration.Locations = pages.Where(x => x.Url != null).Select(y => y.Url!.ToString()).ToList();
            appRegistration.LastModifiedDate = DateTime.UtcNow;

            await legacyDataLoadService.UpdateAppRegistrationAsync(appRegistration).ConfigureAwait(false);

            logger.LogInformation($"Load Pages into AppRegistration Completed");
        }

        public async Task<HttpStatusCode> RemoveAsync(Guid contentId)
        {
            logger.LogInformation($"Remove page {contentId} from App Registration");

            var appRegistration = await legacyDataLoadService.GetAppRegistrationByPathAsync("pages").ConfigureAwait(false);

            if (appRegistration == null)
            {
                return HttpStatusCode.NotFound;
            }

            var locationToRemove = appRegistration.Locations.FirstOrDefault(x => x.Contains(contentId.ToString(), StringComparison.OrdinalIgnoreCase));

            if (locationToRemove != null)
            {
                appRegistration!.Locations!.Remove(locationToRemove);
                appRegistration.LastModifiedDate = DateTime.UtcNow;

                await legacyDataLoadService.UpdateAppRegistrationAsync(appRegistration).ConfigureAwait(false);
            }

            logger.LogInformation($"Remove page {contentId} from App Registration completed");
            return HttpStatusCode.OK;
        }

        private async Task<PageModel?> GetPageAsync(string id)
        {
            logger.LogInformation($"Retrieving Page {id} data");

            var url = new Uri($"{pagesClientOptions.BaseAddress}{pagesClientOptions.Endpoint}/{id}", UriKind.Absolute);
            var result = await apiDataService.GetAsync<PageModel>(httpClient, url).ConfigureAwait(false);

            logger.LogInformation($"Retrieved Page {id} data");

            return result;
        }

        private async Task<IList<PageModel>?> GetPagesAsync()
        {
            logger.LogInformation($"Retrieving Pages data");

            var url = new Uri($"{pagesClientOptions.BaseAddress}{pagesClientOptions.SummaryEndpoint}", UriKind.Absolute);
            var result = await apiDataService.GetAsync<List<PageModel>>(httpClient, url).ConfigureAwait(false);

            logger.LogInformation($"Retrieved Page data");

            return result;
        }
    }
}
