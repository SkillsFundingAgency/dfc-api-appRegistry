using DFC.Api.AppRegistry.Services;
using DFC.Api.AppRegistry.UnitTests.FakeHttpHandlers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "API Service Unit Tests")]
    public class ApiServiceTests
    {
        private readonly Uri dummyUrl = new Uri("https://somewhere.com", UriKind.Absolute);
        private readonly ILogger<ApiService> logger;

        public ApiServiceTests()
        {
            logger = A.Fake<ILogger<ApiService>>();
        }

        [Fact]
        public async Task ApiServiceGetReturnsOkStatusCodeForValidUrl()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            const string expectedResponse = "Expected response string";
            var httpResponse = new HttpResponseMessage { StatusCode = expectedResult, Content = new StringContent(expectedResponse) };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var apiService = new ApiService(logger);

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            // act
            var result = await apiService.GetAsync(httpClient, dummyUrl, MediaTypeNames.Application.Json).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResponse, result);

            httpResponse.Dispose();
            httpClient.Dispose();
            fakeHttpMessageHandler.Dispose();
        }

        [Fact]
        public async Task ApiServiceGetReturnsNotFoundStatusCode()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NotFound;
            string expectedResponse = string.Empty;
            var httpResponse = new HttpResponseMessage { StatusCode = expectedResult, Content = new StringContent(expectedResponse) };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var apiService = new ApiService(logger);

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            // act
            var result = await apiService.GetAsync(httpClient, dummyUrl, MediaTypeNames.Application.Json).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Null(result);

            httpResponse.Dispose();
            httpClient.Dispose();
            fakeHttpMessageHandler.Dispose();
        }

        [Fact]
        public async Task ApiServiceGetReturnsNoContentStatusCode()
        {
            // arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            string expectedResponse = string.Empty;
            var httpResponse = new HttpResponseMessage { StatusCode = expectedResult, Content = new StringContent(expectedResponse) };
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var apiService = new ApiService(logger);

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            // act
            var result = await apiService.GetAsync(httpClient, dummyUrl, MediaTypeNames.Application.Json).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Null(result);

            httpResponse.Dispose();
            httpClient.Dispose();
            fakeHttpMessageHandler.Dispose();
        }

        [Fact]
        public async Task ApiServiceGetReturnsExceptionResult()
        {
            // arrange
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var apiService = new ApiService(logger);

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Throws(new ArgumentException("fake exception"));

            // act
            var result = await apiService.GetAsync(httpClient, dummyUrl, MediaTypeNames.Application.Json).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Null(result);

            httpClient.Dispose();
            fakeHttpMessageHandler.Dispose();
        }

        [Fact]
        public async Task ApiServiceGetReturnsExceptionForNoHttpClient()
        {
            // arrange
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            var apiService = new ApiService(logger);

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await apiService.GetAsync(null, dummyUrl, MediaTypeNames.Application.Json).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'httpClient')", exceptionResult.Message);
        }
    }
}
