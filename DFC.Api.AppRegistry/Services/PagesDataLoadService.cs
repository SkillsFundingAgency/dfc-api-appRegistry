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
        private readonly IDataLoadService dataLoadService;

        public PagesDataLoadService(
            ILogger<PagesDataLoadService> logger,
            HttpClient httpClient,
            PagesClientOptions pagesClientOptions,
            IApiDataService apiDataService,
            IDataLoadService dataLoadService)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.pagesClientOptions = pagesClientOptions;
            this.apiDataService = apiDataService;
            this.dataLoadService = dataLoadService;
        }

        public async Task<HttpStatusCode> CreateOrUpdateAsync(Guid contentId)
        {
            logger.LogInformation($"Load page location {contentId} into App Registration started");

            var appRegistration = await dataLoadService.GetAppRegistrationByPathAsync(AppRegistryPathNameForPagesApp).ConfigureAwait(false);

            if (appRegistration == null)
            {
                return HttpStatusCode.NotFound;
            }

            var pageLocation = await GetPageAsync(contentId).ConfigureAwait(false);

            if (pageLocation == null)
            {
                logger.LogError($"Page {contentId} returned null from Pages API");
                return HttpStatusCode.NotFound;
            }

            appRegistration.PageLocations?.Remove(contentId);

            if (pageLocation.Locations.Any())
            {
                if (appRegistration.PageLocations == null)
                {
                    appRegistration.PageLocations = new Dictionary<Guid, PageLocationModel>();
                }

                appRegistration.PageLocations.Add(contentId, pageLocation);

                appRegistration.LastModifiedDate = DateTime.UtcNow;

                await dataLoadService.UpdateAppRegistrationAsync(appRegistration).ConfigureAwait(false);
            }

            logger.LogInformation($"Load page location {contentId} into App Registration completed");

            return HttpStatusCode.OK;
        }

        public async Task LoadAsync()
        {
            logger.LogInformation($"Load page locations into App Registration started");

            var appRegistration = await dataLoadService.GetAppRegistrationByPathAsync(AppRegistryPathNameForPagesApp).ConfigureAwait(false);

            if (appRegistration == null)
            {
                return;
            }

            var pageLocations = await GetPagesAsync().ConfigureAwait(false);

            if (pageLocations == null)
            {
                logger.LogInformation($"No page locations returned from {nameof(GetPagesAsync)}");
                return;
            }

            appRegistration.PageLocations = pageLocations;
            appRegistration.LastModifiedDate = DateTime.UtcNow;

            await dataLoadService.UpdateAppRegistrationAsync(appRegistration).ConfigureAwait(false);

            logger.LogInformation($"Load page locations into AppRegistration completed");
        }

        public async Task<HttpStatusCode> RemoveAsync(Guid contentId)
        {
            logger.LogInformation($"Remove page location {contentId} from App Registration started");

            var appRegistration = await dataLoadService.GetAppRegistrationByPathAsync(AppRegistryPathNameForPagesApp).ConfigureAwait(false);

            if (appRegistration == null)
            {
                return HttpStatusCode.NotFound;
            }

            appRegistration.PageLocations?.Remove(contentId);
            appRegistration.LastModifiedDate = DateTime.UtcNow;

            await dataLoadService.UpdateAppRegistrationAsync(appRegistration).ConfigureAwait(false);

            logger.LogInformation($"Remove page location {contentId} from App Registration completed");
            return HttpStatusCode.OK;
        }

        private async Task<PageLocationModel?> GetPageAsync(Guid id)
        {
            logger.LogInformation($"Retrieving Page {id} data");

            var url = new Uri($"{pagesClientOptions.BaseAddress}{pagesClientOptions.Endpoint}/{id}", UriKind.Absolute);
            var result = await apiDataService.GetAsync<PageLocationModel>(httpClient, url).ConfigureAwait(false);

            logger.LogInformation($"Retrieved Page {id} data");

            return result;
        }

        private async Task<Dictionary<Guid, PageLocationModel>?> GetPagesAsync()
        {
            logger.LogInformation($"Retrieving Pages data");

            var url = new Uri($"{pagesClientOptions.BaseAddress}{pagesClientOptions.Endpoint}", UriKind.Absolute);
            var result = await apiDataService.GetAsync<Dictionary<Guid, PageLocationModel>>(httpClient, url).ConfigureAwait(false);

            logger.LogInformation($"Retrieved Page data");

            return result;
        }
    }
}
