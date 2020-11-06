using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DFC.Api.AppRegistry.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class HttpRequestExtensions
    {
        public static async Task<TModel?> GetModelFromBodyAsync<TModel>(this HttpRequest request)
            where TModel : class
        {
            var bodyString = await request.ReadAsStringAsync().ConfigureAwait(false);
            var bodyModel = JsonConvert.DeserializeObject<TModel>(bodyString);

            return bodyModel;
        }
    }
}
