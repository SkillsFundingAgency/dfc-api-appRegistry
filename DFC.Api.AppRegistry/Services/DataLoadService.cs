using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class DataLoadService : IDataLoadService
    {
        private readonly ILogger<DataLoadService> logger;
        private readonly IModelValidationService modelValidationService;
        private readonly IDocumentService<AppRegistrationModel> documentService;

        public DataLoadService(
            ILogger<DataLoadService> logger,
            IModelValidationService modelValidationService,
            IDocumentService<AppRegistrationModel> documentService)
        {
            this.logger = logger;
            this.modelValidationService = modelValidationService;
            this.documentService = documentService;
        }

        public async Task<AppRegistrationModel?> GetAppRegistrationByPathAsync(string? path)
        {
            logger.LogInformation($"Retrieving App Registration: {path}");

            var result = await documentService.GetAsync(x => x.Path == path).ConfigureAwait(false);

            if (result == null)
            {
                logger.LogInformation($"App Registration: {path} not found");
            }

            return result?.FirstOrDefault();
        }

        public async Task UpdateAppRegistrationAsync(AppRegistrationModel appRegistrationModel)
        {
            _ = appRegistrationModel ?? throw new ArgumentNullException(nameof(appRegistrationModel));

            logger.LogInformation($"Upserting App Registration: {JsonConvert.SerializeObject(appRegistrationModel)}");

            if (modelValidationService.ValidateModel(appRegistrationModel))
            {
                var upsertResult = await documentService.UpsertAsync(appRegistrationModel).ConfigureAwait(false);

                if (upsertResult == HttpStatusCode.OK || upsertResult == HttpStatusCode.Created)
                {
                    logger.LogInformation($"Upserted app registration: {appRegistrationModel.Path}: Status code: {upsertResult}");
                }
                else
                {
                    logger.LogError($"Failed to upsert app registration: {appRegistrationModel.Path}: Status code: {upsertResult}");
                }
            }
        }
    }
}
