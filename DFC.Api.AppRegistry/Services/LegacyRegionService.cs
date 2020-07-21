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
    public class LegacyRegionService : ILegacyRegionService
    {
        private readonly ILogger<LegacyRegionService> logger;
        private readonly HttpClient httpClient;
        private readonly RegionClientOptions regionClientOptions;
        private readonly IApiDataService apiDataService;

        public LegacyRegionService(
            ILogger<LegacyRegionService> logger,
            HttpClient httpClient,
            RegionClientOptions regionClientOptions,
            IApiDataService apiDataService)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.regionClientOptions = regionClientOptions;
            this.apiDataService = apiDataService;
        }

        public async Task<IList<LegacyRegionModel>?> GetListAsync(string? path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));

            logger.LogInformation($"Retrieving Region data for: {path}");

            var url = new Uri($"{regionClientOptions.BaseAddress}{regionClientOptions.Endpoint.Replace("{path}", path, StringComparison.OrdinalIgnoreCase)}", UriKind.Absolute);
            var result = await apiDataService.GetAsync<List<LegacyRegionModel>>(httpClient, url).ConfigureAwait(false);

            logger.LogInformation($"Retrieved Region data for: {path}");

            return result;
        }
    }
}
