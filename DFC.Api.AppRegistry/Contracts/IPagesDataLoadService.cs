using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface IPagesDataLoadService
    {
        Task LoadAsync();

        Task<HttpStatusCode> CreateOrUpdateAsync(Guid contentId);

        Task<HttpStatusCode> RemoveAsync(Guid contentId);
    }
}
