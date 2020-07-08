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
    [Trait("Category", "LegacyRegion - Service tests")]
    public class LegacyRegionServiceTests
    {
        private readonly Uri dummyUrl = new Uri("https://somewhere.com", UriKind.Absolute);
        private readonly ILogger<LegacyRegionService> fakeLogger = A.Fake<ILogger<LegacyRegionService>>();
        private readonly HttpClient fakeHttpClient = A.Fake<HttpClient>();
        private readonly IApiDataService fakeApiDataService = A.Fake<IApiDataService>();
        private readonly RegionClientOptions regionClientOptions;

        public LegacyRegionServiceTests()
        {
            regionClientOptions = new RegionClientOptions
            {
                BaseAddress = dummyUrl,
            };
        }

        [Fact]
        public async Task GetListAsyncIsSuccessful()
        {
            // Arrange
            const string path = "unit-test";
            var expectedResult = A.CollectionOfDummy<LegacyRegionModel>(2);
            var service = new LegacyRegionService(fakeLogger, fakeHttpClient, regionClientOptions, fakeApiDataService);

            A.CallTo(() => fakeApiDataService.GetAsync<IList<LegacyRegionModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(expectedResult);

            // Act
            var result = await service.GetListAsync(path).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeApiDataService.GetAsync<List<LegacyRegionModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();

            A.Equals(expectedResult, result);
        }

        [Fact]
        public async Task GetListAsyncReturnsExceptionWhenPathIsNull()
        {
            // Arrange
            const string? path = null;
            var service = new LegacyRegionService(fakeLogger, fakeHttpClient, regionClientOptions, fakeApiDataService);

            // Act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetListAsync(path).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeApiDataService.GetAsync<List<LegacyRegionModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustNotHaveHappened();

            Assert.Equal("Value cannot be null. (Parameter 'path')", exceptionResult.Message);
        }
    }
}
