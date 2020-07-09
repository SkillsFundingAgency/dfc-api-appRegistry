using AutoMapper;
using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public void MapAndUpdateRegionModel(AppRegistrationModel? appRegistrationModel, LegacyRegionModel? legacyRegionModel)
        {
            _ = appRegistrationModel ?? throw new ArgumentNullException(nameof(appRegistrationModel));
            _ = legacyRegionModel ?? throw new ArgumentNullException(nameof(legacyRegionModel));

            if (appRegistrationModel.Regions == null)
            {
                appRegistrationModel.Regions = new List<RegionModel>();
            }

            var model = mapper.Map<RegionModel>(legacyRegionModel);

            var regionToUpdate = appRegistrationModel.Regions.FirstOrDefault(x => x.PageRegion == legacyRegionModel.PageRegion);

            if (regionToUpdate == null)
            {
                appRegistrationModel.Regions.Add(model);
            }
            else
            {
                appRegistrationModel.Regions.Remove(regionToUpdate);
                appRegistrationModel.Regions.Add(model);
            }
        }
    }
}
