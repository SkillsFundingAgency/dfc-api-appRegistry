using DFC.Api.AppRegistry.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class ApiService : IApiService
    {
        private readonly ILogger<ApiService> logger;

        public ApiService(ILogger<ApiService> logger)
        {
            this.logger = logger;
        }

        public Task<HttpStatusCode> DeleteAsync(HttpClient? httpClient, Uri url)
        {
            throw new NotImplementedException();
        }

        public async Task<string?> GetAsync(HttpClient? httpClient, Uri url, string acceptHeader)
        {
            _ = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            logger.LogInformation($"Loading data from {url}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(acceptHeader));

            try
            {
                var response = await httpClient.SendAsync(request).ConfigureAwait(false);
                string? responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Failed to get {acceptHeader} data from {url}, received error : '{responseString}', returning empty content.");
                    responseString = null;
                }
                else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    logger.LogInformation($"Status - {response.StatusCode} with response '{responseString}' received from {url}, returning empty content.");
                    responseString = null;
                }

                return responseString;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error received getting {acceptHeader} data '{ex.InnerException?.Message}'. Received from {url}, returning empty content");
            }

            return default;
        }

        public Task<string?> GetAsync(HttpClient httpClient, string contentType, string id, string acceptHeader)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetAsync(HttpClient httpClient, string contentType, string acceptHeader)
        {
            throw new NotImplementedException();
        }

        public Task<HttpStatusCode> PostAsync<TApiModel>(HttpClient? httpClient, Uri url, TApiModel model) where TApiModel : class
        {
            throw new NotImplementedException();
        }
    }
}
