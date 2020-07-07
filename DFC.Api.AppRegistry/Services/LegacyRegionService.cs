using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models.Legacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class LegacyRegionService : ILegacyRegionService
    {
        public async Task<List<LegacyRegionModel>> GetListAsync(string? path)
        {
            return new List<LegacyRegionModel>();
        }
    }
}
