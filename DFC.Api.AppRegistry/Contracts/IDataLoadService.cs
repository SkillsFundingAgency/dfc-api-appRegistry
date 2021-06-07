using DFC.Api.AppRegistry.Models;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface IDataLoadService
    {
        Task UpdateAppRegistrationAsync(AppRegistrationModel appRegistrationModel);

        Task<AppRegistrationModel?> GetAppRegistrationByPathAsync(string path);
    }
}