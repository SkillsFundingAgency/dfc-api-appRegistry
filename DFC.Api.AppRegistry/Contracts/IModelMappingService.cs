using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
using System.Collections.Generic;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface IModelMappingService
    {
        void MapModels(AppRegistrationModel? appRegistrationModel, LegacyPathModel? legacyPathModel, IList<LegacyRegionModel>? legacyRegionModels);

        void MapRegionModelToAppRegistration(AppRegistrationModel? appRegistrationModel, LegacyRegionModel? legacyRegionModel);
    }
}