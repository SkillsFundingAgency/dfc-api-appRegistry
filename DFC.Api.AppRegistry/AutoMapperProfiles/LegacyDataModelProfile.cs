using AutoMapper;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
using Microsoft.Azure.Documents;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.AppRegistry.AutoMapperProfiles
{
    [ExcludeFromCodeCoverage]
    public class LegacyDataModelProfile : Profile
    {
        public LegacyDataModelProfile()
        {
            CreateMap<LegacyPathModel, AppRegistrationModel>();

            CreateMap<LegacyRegionModel, RegionModel>();
        }
    }
}
