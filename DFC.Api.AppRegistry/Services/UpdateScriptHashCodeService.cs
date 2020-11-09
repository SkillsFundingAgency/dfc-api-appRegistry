using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.ClientOptions;
using DFC.Compui.Cosmos.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class UpdateScriptHashCodeService : IUpdateScriptHashCodeService
    {
        private const string ContentMDS = "content-md5";
        private readonly ILogger<UpdateScriptHashCodeService> logger;
        private readonly HttpClient httpClient;
        private readonly UpdateScriptHashCodeClientOptions updateScriptHashCodeClientOptions;
        private readonly IDocumentService<AppRegistrationModel> documentService;

        public UpdateScriptHashCodeService(
           ILogger<UpdateScriptHashCodeService> logger,
           HttpClient httpClient,
           UpdateScriptHashCodeClientOptions updateScriptHashCodeClientOptions,
           IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.updateScriptHashCodeClientOptions = updateScriptHashCodeClientOptions;
            this.documentService = documentService;
        }

        public string? CdnLocation { get; set; }

        public async Task<HttpStatusCode> UpdateAllAsync()
        {
            logger.LogInformation($"Retrieving all app registrations to update their JavaScript hash codes ftom CDN: {CdnLocation}");

            var appRegistrations = await documentService.GetAllAsync().ConfigureAwait(false);

            if (appRegistrations == null)
            {
                return HttpStatusCode.NoContent;
            }

            foreach (var appRegistration in appRegistrations.Where(w => w.JavaScriptNames != null && w.JavaScriptNames.Any()))
            {
                await UpdateHashCodesAsync(appRegistration).ConfigureAwait(false);
            }

            logger.LogInformation("Updated all app registrations with their JavaScript hash codes");

            return HttpStatusCode.OK;
        }

        public async Task UpdateHashCodesAsync(AppRegistrationModel? appRegistrationModel)
        {
            _ = appRegistrationModel ?? throw new ArgumentNullException(nameof(appRegistrationModel));

            logger.LogInformation($"Attempting to update JavaScript hash codes in app registration for: {appRegistrationModel.Path}");

            var updatedHashcodeCount = await RefreshHashcodesAsync(appRegistrationModel).ConfigureAwait(false);

            if (updatedHashcodeCount > 0)
            {
                appRegistrationModel.LastModifiedDate = DateTime.UtcNow;

                var statusCode = await documentService.UpsertAsync(appRegistrationModel).ConfigureAwait(false);

                if (statusCode == HttpStatusCode.OK)
                {
                    logger.LogInformation($"Updated ({updatedHashcodeCount}) app registration JavaScript hash codes for: {appRegistrationModel.Path}");
                }
                else
                {
                    logger.LogError($"Error updating app registration JavaScript hash codes for: {appRegistrationModel.Path}: Status code {statusCode}");
                }
            }
            else
            {
                logger.LogInformation($"No app registration JavaScript hash codes updated for: {appRegistrationModel.Path}");
            }
        }

        public async Task<int> RefreshHashcodesAsync(AppRegistrationModel? appRegistrationModel)
        {
            _ = appRegistrationModel ?? throw new ArgumentNullException(nameof(appRegistrationModel));

            int updatedHashcodeCount = 0;

            foreach (var key in appRegistrationModel.JavaScriptNames!.Keys.ToList())
            {
                var fullUrlPath = key.StartsWith("/", StringComparison.Ordinal) ? CdnLocation + key : key;

                var hashcode = await GetFileHashAsync(new Uri(fullUrlPath, UriKind.Absolute)).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(hashcode) && (appRegistrationModel.JavaScriptNames[key] == null || !appRegistrationModel.JavaScriptNames[key]!.Equals(hashcode, StringComparison.OrdinalIgnoreCase)))
                {
                    appRegistrationModel.JavaScriptNames[key] = hashcode;
                    updatedHashcodeCount++;
                }
            }

            return updatedHashcodeCount;
        }

        public async Task<string> GetFileHashAsync(Uri assetLocation)
        {
            const string defaultFormat = "yyyyMMddHH";

            try
            {
                var response = await httpClient.GetAsync(assetLocation).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var hashcode = response.Content.Headers.GetValues(ContentMDS).FirstOrDefault();

                    return !string.IsNullOrWhiteSpace(hashcode)
                        ? hashcode.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase)
                        : DateTime.Now.ToString(defaultFormat, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to get file hash for {assetLocation}");
            }

            //If we don't get a valid response use the current time to the nearest hour.
            return DateTime.Now.ToString(defaultFormat, CultureInfo.InvariantCulture);
        }
    }
}
