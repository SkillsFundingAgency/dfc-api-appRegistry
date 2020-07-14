using DFC.Api.AppRegistry.Models;
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

        Task UpdateRegionAsync(LegacyRegionModel legacyRegionModel);

        Task UpdatePathAsync(LegacyPathModel? legacyPathModel);

        Task UpdateAppRegistrationAsync(AppRegistrationModel appRegistrationModel);

        Task<AppRegistrationModel?> GetAppRegistrationAsync(string appRegistrationName);
    }
}