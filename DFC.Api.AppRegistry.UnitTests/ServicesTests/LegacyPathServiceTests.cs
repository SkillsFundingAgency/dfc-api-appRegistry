using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models.ClientOptions;
using DFC.Api.AppRegistry.Models.Legacy;
using DFC.Api.AppRegistry.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "LegacyPath - Service tests")]
    public class LegacyPathServiceTests
    {
        private readonly Uri dummyUrl = new Uri("https://somewhere.com", UriKind.Absolute);
        private readonly ILogger<LegacyPathService> fakeLogger = A.Fake<ILogger<LegacyPathService>>();
        private readonly HttpClient fakeHttpClient = A.Fake<HttpClient>();
        private readonly IApiDataService fakeApiDataService = A.Fake<IApiDataService>();
        private readonly PathClientOptions pathClientOptions;
        private readonly LegacyPathService legacyPathService;

        public LegacyPathServiceTests()
        {
            pathClientOptions = new PathClientOptions
            {
                BaseAddress = dummyUrl,
            };
            legacyPathService = new LegacyPathService(fakeLogger, fakeHttpClient, pathClientOptions, fakeApiDataService);
        }

        [Fact]
        public async Task GetListAsyncIsSuccessful()
        {
            // Arrange
            var expectedResult = new List<LegacyPathModel> { new LegacyPathModel { Path = "a-path" } };

            A.CallTo(() => fakeApiDataService.GetAsync<List<LegacyPathModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(expectedResult);

            // Act
            var result = await legacyPathService.GetListAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeApiDataService.GetAsync<List<LegacyPathModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }
    }
}
