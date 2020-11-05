using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class UpdateScriptHashCodes : IUpdateScriptHashCodes
    {
        private readonly ILogger<UpdateScriptHashCodes> logger;
        private readonly IDocumentService<AppRegistrationModel> documentService;

        public UpdateScriptHashCodes(
           ILogger<UpdateScriptHashCodes> logger,
           IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
        }

        public string CdnLocation { get; set; }

        public async Task<HttpStatusCode> UpdateAllAsync()
        {
            logger.LogInformation("Retrieving all app registrations to update their JavaScript hash codes");

            var appRegistrations = await documentService.GetAllAsync().ConfigureAwait(false);

            if (appRegistrations == null)
            {
                return HttpStatusCode.NoContent;
            }

            foreach (var appRegistration in appRegistrations.Where(w => w.JavaScriptNames.Any()))
            {
                await UpdateHashCodesAsync(appRegistration).ConfigureAwait(false);
            }

            logger.LogInformation("Updated all app registrations with their JavaScript hash codes");

            return HttpStatusCode.OK;
        }

        public async Task UpdateHashCodesAsync(AppRegistrationModel appRegistration)
        {
            _ = appRegistration ?? throw new ArgumentNullException(nameof(appRegistration));
            _ = appRegistration.JavaScriptNames ?? throw new ArgumentNullException(nameof(appRegistration.JavaScriptNames));

            logger.LogInformation($"Attempting to update app registration for: {appRegistration.Path}");

            foreach(var javaScriptName in appRegistration.JavaScriptNames)
            {
                var hashcode = await GetHashcode(javaScriptName).ConfigureAwait(false);
            }

            logger.LogInformation($"Updated app registration JavaScript hash codes for: {appRegistration.Path}");
        }
    }
}
