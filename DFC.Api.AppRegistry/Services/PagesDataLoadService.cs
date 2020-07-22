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
        public const string AppRegistryPathNameForPagesApp = "pages";

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

            var appRegistration = await legacyDataLoadService.GetAppRegistrationByPathAsync(AppRegistryPathNameForPagesApp).ConfigureAwait(false);

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

            var newLocations = page.AllLocations;

            if (newLocations != null && newLocations.Any())
            {
                if (appRegistration.Locations == null)
                {
                    appRegistration.Locations = newLocations;
                }
                else
                {
                    appRegistration.Locations.AddRange(newLocations);
                }

                appRegistration.LastModifiedDate = DateTime.UtcNow;

                await legacyDataLoadService.UpdateAppRegistrationAsync(appRegistration).ConfigureAwait(false);
            }

            logger.LogInformation($"Load Page {contentId} into App Registration Completed");

            return HttpStatusCode.OK;
        }

        public async Task LoadAsync()
        {
            logger.LogInformation($"Load Pages into App Registration Started");

            var appRegistration = await legacyDataLoadService.GetAppRegistrationByPathAsync(AppRegistryPathNameForPagesApp).ConfigureAwait(false);

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

            appRegistration.Locations = pages.Where(x => x.Url != null).SelectMany(x => x.AllLocations).ToList();
            appRegistration.LastModifiedDate = DateTime.UtcNow;

            await legacyDataLoadService.UpdateAppRegistrationAsync(appRegistration).ConfigureAwait(false);

            logger.LogInformation($"Load Pages into AppRegistration Completed");
        }

        public async Task<HttpStatusCode> RemoveAsync(Guid contentId)
        {
            logger.LogInformation($"Remove page {contentId} from App Registration");

            var appRegistration = await legacyDataLoadService.GetAppRegistrationByPathAsync(AppRegistryPathNameForPagesApp).ConfigureAwait(false);

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

            var locationsToRemove = page.AllLocations;

            if (locationsToRemove != null && locationsToRemove.Any() && appRegistration.Locations != null && appRegistration.Locations.Any())
            {
                locationsToRemove.ForEach(f => appRegistration.Locations.Remove(f));
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

            var url = new Uri($"{pagesClientOptions.BaseAddress}{pagesClientOptions.Endpoint}", UriKind.Absolute);
            var result = await apiDataService.GetAsync<List<PageModel>>(httpClient, url).ConfigureAwait(false);

            logger.LogInformation($"Retrieved Page data");

            return result;
        }
    }
}
