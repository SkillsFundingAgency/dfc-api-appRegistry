using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models.ClientOptions;
using DFC.Api.AppRegistry.Models.Legacy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class LegacyPathService : ILegacyPathService
    {
        private readonly ILogger<LegacyPathService> logger;
        private readonly HttpClient httpClient;
        private readonly PathClientOptions pathClientOptions;
        private readonly IApiDataService apiDataService;

        public LegacyPathService(
            ILogger<LegacyPathService> logger,
            HttpClient httpClient,
            PathClientOptions pathClientOptions,
            IApiDataService apiDataService)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.pathClientOptions = pathClientOptions;
            this.apiDataService = apiDataService;
        }

        public async Task<IList<LegacyPathModel>?> GetListAsync()
        {
            logger.LogInformation($"Retrieving all Path data");

            var url = new Uri($"{pathClientOptions.BaseAddress}{pathClientOptions.Endpoint}", UriKind.Absolute);
            var result = await apiDataService.GetAsync<List<LegacyPathModel>>(httpClient, url).ConfigureAwait(false);

            logger.LogInformation($"Retrieved all Path data");

            return result;
        }
    }
}
