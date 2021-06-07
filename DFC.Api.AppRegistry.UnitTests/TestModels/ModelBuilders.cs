using DFC.Api.AppRegistry.Enums;
using DFC.Api.AppRegistry.Models;
using System.Collections.Generic;

namespace DFC.Api.AppRegistry.UnitTests.TestModels
{
    public static class ModelBuilders
    {
        public const string PathName = "unit-tests";

        public static List<AppRegistrationModel> ValidAppRegistrationModels(string path)
        {
            return new List<AppRegistrationModel>
            {
                ValidAppRegistrationModel(path),
                ValidAppRegistrationModel(path + "-2"),
            };
        }

        public static AppRegistrationModel ValidAppRegistrationModel(string path)
        {
            return new AppRegistrationModel
            {
                Id = System.Guid.NewGuid(),
                Path = path,
            };
        }

        public static List<RegionModel> ValidRegionModels()
        {
            return new List<RegionModel>
            {
                ValidRegionModel(PageRegion.Head),
                ValidRegionModel(PageRegion.Body),
            };
        }

        public static RegionModel ValidRegionModel(PageRegion pageRegion)
        {
            return new RegionModel
            {
                PageRegion = pageRegion,
            };
        }
      }
}
