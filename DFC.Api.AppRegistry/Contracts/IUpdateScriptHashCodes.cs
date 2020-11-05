using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Contracts
{
    public interface IUpdateScriptHashCodes
    {
        string CdnLocation { get; set; }

        Task<HttpStatusCode> UpdateAllAsync();
    }
}