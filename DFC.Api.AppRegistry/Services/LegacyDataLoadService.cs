using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
using DFC.Compui.Cosmos.Contracts;
using Microsoft.Extensions.Logging;
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

        public async Task LoadAsync()
        {
            logger.LogInformation("Loading legacy data");

            var legacyPathModels = await legacyPathService.GetListAsync().ConfigureAwait(false);

            if (legacyPathModels != null && legacyPathModels.Any())
            {
                await ProcessPathsAsync(legacyPathModels).ConfigureAwait(false);
            }

            logger.LogInformation("Loaded legacy data");
        }

        public async Task ProcessPathsAsync(IList<LegacyPathModel>? legacyPathModels)
        {
            _ = legacyPathModels ?? throw new ArgumentNullException(nameof(legacyPathModels));

            foreach (var legacyPathModel in legacyPathModels.OrderBy(o => o.Path))
            {
                await ProcessPathAsync(legacyPathModel).ConfigureAwait(false);
            }
        }

        public async Task ProcessPathAsync(LegacyPathModel? legacyPathModel)
        {
            _ = legacyPathModel ?? throw new ArgumentNullException(nameof(legacyPathModel));

            var legacyRegionModels = await legacyRegionService.GetListAsync(legacyPathModel.Path).ConfigureAwait(false);

            //TODO: ian - need to improve the following two lines of code in the Cosmos nuget
            var appRegistrationModels = await documentService.GetAllAsync().ConfigureAwait(false);
            var appRegistrationModel = appRegistrationModels?.FirstOrDefault(f => f.Path == legacyPathModel.Path) ?? new AppRegistrationModel();

            modelMappingService.MapModels(appRegistrationModel, legacyPathModel, legacyRegionModels);

            if (modelValidationService.ValidateModel(appRegistrationModel))
            {
                var upsertResult = await documentService.UpsertAsync(appRegistrationModel).ConfigureAwait(false);

                if (upsertResult == HttpStatusCode.OK)
                {
                    logger.LogInformation($"Upserted app registration: {appRegistrationModel.Path}");
                }
                else
                {
                    logger.LogError($"Failed to upsert app registration: {appRegistrationModel.Path}: Status code: {upsertResult}");
                }
            }
        }
    }
}
