using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.AppRegistry.Models.ClientOptions
{
    [ExcludeFromCodeCoverage]
    public class RegionClientOptions : ClientOptionsModel
    {
        public string Endpoint { get; set; } = "api/paths/{path}/regions";
    }
}
