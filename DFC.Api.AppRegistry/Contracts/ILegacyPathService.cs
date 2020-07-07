using DFC.Api.AppRegistry.Models.Legacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface ILegacyPathService
    {
        Task<List<LegacyPathModel>> GetListAsync();
    }
}