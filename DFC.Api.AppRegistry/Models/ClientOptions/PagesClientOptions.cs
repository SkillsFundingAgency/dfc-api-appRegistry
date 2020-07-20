namespace DFC.Api.AppRegistry.Models.ClientOptions
{
    public class PagesClientOptions : ClientOptionsModel
    {
        public string SummaryEndpoint { get; set; } = "api/pages";
        public string Endpoint { get; set; } = "api/page";
    }
}
