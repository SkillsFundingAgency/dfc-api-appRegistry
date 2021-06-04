using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
using DFC.Compui.Cosmos.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class LegacyDataLoadService : ILegacyDataLoadService
    {
        private readonly ILogger<LegacyDataLoadService> logger;
        private readonly IModelMappingService modelMappingService;
        private readonly IModelValidationService modelValidationService;
        private readonly IDocumentService<AppRegistrationModel> documentService;
        private readonly ILegacyPathService legacyPathService;
        private readonly ILegacyRegionService legacyRegionService;

        public LegacyDataLoadService(
            ILogger<LegacyDataLoadService> logger,
            IModelMappingService modelMappingService,
            IModelValidationService modelValidationService,
            IDocumentService<AppRegistrationModel> documentService,
            ILegacyPathService legacyPathService,
            ILegacyRegionService legacyRegionService)
        {
            this.logger = logger;
            this.modelMappingService = modelMappingService;
            this.modelValidationService = modelValidationService;
            this.documentService = documentService;
            this.legacyPathService = legacyPathService;
            this.legacyRegionService = legacyRegionService;
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
