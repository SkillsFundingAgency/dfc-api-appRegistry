using System.Net.Http;

namespace DFC.Api.AppRegistry.UnitTests.FakeHttpHandlers
{
    public interface IFakeHttpRequestSender
    {
        HttpResponseMessage Send(HttpRequestMessage request);
    }
}