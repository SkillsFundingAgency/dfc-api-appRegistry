using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface ILegacyDataLoadService
    {
        Task UpdateAppRegistrationAsync(AppRegistrationModel appRegistrationModel);

        Task<AppRegistrationModel?> GetAppRegistrationByPathAsync(string path);
    }
}