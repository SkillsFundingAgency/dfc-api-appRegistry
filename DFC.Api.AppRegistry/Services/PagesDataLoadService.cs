using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models.ClientOptions;
using DFC.Api.AppRegistry.Models.Pages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        public PagesDataLoadService(
            ILogger<PagesDataLoadService> logger,
            HttpClient httpClient,
            PagesClientOptions pagesClientOptions,
            IApiDataService apiDataService)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.pagesClientOptions = pagesClientOptions;
            this.apiDataService = apiDataService;
        }

        public async Task LoadAsync()
        {
            var pages = await GetPagesAsync();
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
