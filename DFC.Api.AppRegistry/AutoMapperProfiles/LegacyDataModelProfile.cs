using AutoMapper;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.AppRegistry.AutoMapperProfiles
{
    [ExcludeFromCodeCoverage]
    public class LegacyDataModelProfile : Profile
    {
        public LegacyDataModelProfile()
        {
            CreateMap<LegacyPathModel, AppRegistrationModel>()
                .ForMember(d => d.Etag, s => s.Ignore());

            CreateMap<LegacyRegionModel, RegionModel>()
                .ForMember(d => d.IsHealthy, s => s.Ignore());
        }
    }
}
