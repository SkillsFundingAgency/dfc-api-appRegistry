using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface IApiDataService
    {
        Task<TApiModel?> GetAsync<TApiModel>(HttpClient? httpClient, Uri url)
            where TApiModel : class;
    }
}