using DFC.Api.AppRegistry.Enums;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
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

        public static List<LegacyPathModel> ValidLegacyPathModels()
        {
            return new List<LegacyPathModel>
            {
                ValidLegacyPathModel(PathName),
                ValidLegacyPathModel(PathName + "-2"),
            };
        }

        public static LegacyPathModel ValidLegacyPathModel(string path)
        {
            return new LegacyPathModel
            {
                Path = path,
                Layout = Layout.FullWidth,
            };
        }

        public static List<LegacyRegionModel> ValidLegacyRegionModels()
        {
            return new List<LegacyRegionModel>
            {
                ValidLegacyRegionModel(PathName, PageRegion.Head),
                ValidLegacyRegionModel(PathName + "-2", PageRegion.Body),
            };
        }

        public static LegacyRegionModel ValidLegacyRegionModel(string path, PageRegion pageRegion)
        {
            return new LegacyRegionModel
            {
                Path = path,
                PageRegion = pageRegion,
                RegionEndpoint = "https://somewhere.com",
            };
        }
    }
}
