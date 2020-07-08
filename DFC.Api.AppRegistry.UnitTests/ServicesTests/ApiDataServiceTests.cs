using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Services;
using DFC.Api.AppRegistry.UnitTests.TestModels;
using FakeItEasy;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "API Data Service Unit Tests")]
    public class ApiDataServiceTests
    {
        private readonly Uri dummyUrl = new Uri("https://somewhere.com", UriKind.Absolute);
        private readonly IApiService fakeApiService = A.Fake<IApiService>();

        [Fact]
        public async Task ApiDataServiceGetReturnsSuccess()
        {
            // arrange
            var expectedResult = new ApiTestModel
            {
                Id = Guid.NewGuid(),
                Name = "a name",
            };
            var jsonResponse = JsonConvert.SerializeObject(expectedResult);

            A.CallTo(() => fakeApiService.GetAsync(A<HttpClient>.Ignored, A<Uri>.Ignored, A<string>.Ignored)).Returns(jsonResponse);

            var apiDataService = new ApiDataService(fakeApiService);

            // act
            var result = await apiDataService.GetAsync<ApiTestModel>(A.Fake<HttpClient>(), dummyUrl).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiService.GetAsync(A<HttpClient>.Ignored, A<Uri>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.Equals(result, expectedResult);
        }

        [Fact]
        public async Task ApiDataServiceGetReturnsNullForNoData()
        {
            // arrange
            ApiTestModel? expectedResult = null;

            A.CallTo(() => fakeApiService.GetAsync(A<HttpClient>.Ignored, A<Uri>.Ignored, A<string>.Ignored)).Returns(string.Empty);

            var apiDataService = new ApiDataService(fakeApiService);

            // act
            var result = await apiDataService.GetAsync<ApiTestModel>(A.Fake<HttpClient>(), dummyUrl).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiService.GetAsync(A<HttpClient>.Ignored, A<Uri>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.Equals(result, expectedResult);
        }

        [Fact]
        public async Task ApiDataServiceGetReturnsExceptionForNoHttpClient()
        {
            // arrange
            var apiDataService = new ApiDataService(fakeApiService);

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await apiDataService.GetAsync<ApiTestModel>(null, dummyUrl).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiService.GetAsync(A<HttpClient>.Ignored, A<Uri>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            Assert.Equal("Value cannot be null. (Parameter 'httpClient')", exceptionResult.Message);
        }
    }
}
