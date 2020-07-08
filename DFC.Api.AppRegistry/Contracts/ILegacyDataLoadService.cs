using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface ILegacyDataLoadService
    {
        Task LoadAsync();

        void MapModels(AppRegistrationModel appRegistrationModel, LegacyPathModel legacyPathModel, IList<LegacyRegionModel> legacyRegionModels);

        Task ProcessPathAsync(LegacyPathModel legacyPathModel);

        Task ProcessPathsAsync(IList<LegacyPathModel> legacyPathModels);

        bool ValidateModel(AppRegistrationModel appRegistrationModel);
    }
}