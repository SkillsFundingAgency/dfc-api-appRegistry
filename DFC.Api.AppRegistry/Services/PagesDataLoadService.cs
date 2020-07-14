using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models.ClientOptions;
using DFC.Api.AppRegistry.Models.Pages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

            appRegistration.Locations = new List<string>();
            appRegistration.Locations = pages.Where(x => x.Url != null).Select(y => y.Url!.ToString()).ToList();

            await legacyDataLoadService.UpdateAppRegistrationAsync(appRegistration).ConfigureAwait(false);

            logger.LogInformation($"Load Pages into AppRegistration Completed");
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
