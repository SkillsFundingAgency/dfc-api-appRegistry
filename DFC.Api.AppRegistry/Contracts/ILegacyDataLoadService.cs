using DFC.Api.AppRegistry.Models.Legacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface ILegacyDataLoadService
    {
        Task LoadAsync();

        Task ProcessPathsAsync(IList<LegacyPathModel> legacyPathModels);

        Task ProcessPathAsync(LegacyPathModel legacyPathModel);
    }
}