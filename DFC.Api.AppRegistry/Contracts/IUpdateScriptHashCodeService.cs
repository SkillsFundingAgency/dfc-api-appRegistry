using DFC.Api.AppRegistry.Models;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface IUpdateScriptHashCodeService
    {
        Task<HttpStatusCode> UpdateAllAsync(string? cdnLocation);

        Task<int> RefreshHashcodesAsync(AppRegistrationModel? appRegistrationModel, string? cdnLocation);
    }
}