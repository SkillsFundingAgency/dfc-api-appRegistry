using AutoMapper;
using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
using System;
using System.Collections.Generic;

namespace DFC.Api.AppRegistry.Services
{
    public class ModelMappingService : IModelMappingService
    {
        private readonly IMapper mapper;

        public ModelMappingService(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public void MapModels(AppRegistrationModel? appRegistrationModel, LegacyPathModel? legacyPathModel, IList<LegacyRegionModel>? legacyRegionModels)
        {
            _ = appRegistrationModel ?? throw new ArgumentNullException(nameof(appRegistrationModel));
            _ = legacyPathModel ?? throw new ArgumentNullException(nameof(legacyPathModel));

            mapper.Map(legacyPathModel, appRegistrationModel);

            if (legacyRegionModels != null)
            {
                appRegistrationModel.Regions = mapper.Map<List<RegionModel>>(legacyRegionModels);
            }
            else
            {
                appRegistrationModel.Regions = null;
            }
        }
    }
}
