using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models.Legacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Services
{
    public class LegacyPathService : ILegacyPathService
    {
        public async Task<List<LegacyPathModel>> GetListAsync()
        {
            return new List<LegacyPathModel>();
        }
    }
}
