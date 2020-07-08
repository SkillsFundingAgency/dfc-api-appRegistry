using DFC.Api.AppRegistry.Models.Legacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface ILegacyRegionService
    {
        Task<IList<LegacyRegionModel>?> GetListAsync(string? path);
    }
}