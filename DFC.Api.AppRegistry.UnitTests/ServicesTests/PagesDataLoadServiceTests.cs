using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.ClientOptions;
using DFC.Api.AppRegistry.Models.Pages;
using DFC.Api.AppRegistry.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "LegacyPath - Service tests")]
    public class PagesDataLoadServiceTests
    {
        private static readonly Dictionary<Guid, PageLocationModel> PageLocationModels = new Dictionary<Guid, PageLocationModel>
        {
            {
                Guid.NewGuid(),
                new PageLocationModel
                {
                    Locations = new List<string> { "/somewhere/somewhere-else" },
                }
            },
            {
                Guid.NewGuid(),
                new PageLocationModel
                {
                    Locations = new List<string> { "/somewhere/somewhere-else-2" },
                }
            },
        };

        private readonly ILogger<PagesDataLoadService> logger = A.Fake<ILogger<PagesDataLoadService>>();
        private readonly HttpClient fakeHttpClient = A.Fake<HttpClient>();
        private readonly IApiDataService fakeApiDataService = A.Fake<IApiDataService>();
        private readonly IDataLoadService fakeDataLoadService = A.Fake<IDataLoadService>();
        private readonly PagesClientOptions pagesClientOptions;

        public PagesDataLoadServiceTests()
        {
            pagesClientOptions = new PagesClientOptions { BaseAddress = new Uri("http://somewhere.com"), Endpoint = PagesDataLoadService.AppRegistryPathNameForPagesApp };
        }

        [Fact]
        public async Task PagesDataLoadServiceLoadAsyncNullRegistrationDocumentNoUpdate()
        {
            // Arrange
            AppRegistrationModel? model = null;
            var serviceToTest = new PagesDataLoadService(logger, fakeHttpClient, pagesClientOptions, fakeApiDataService, fakeDataLoadService);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).Returns(model);

            // Act
            await serviceToTest.LoadAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDataLoadService.UpdateAppRegistrationAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task PagesDataLoadServiceLoadAsyncNullPagesNoRegistrationDocumentUpdated()
        {
            // Arrange
            AppRegistrationModel model = new AppRegistrationModel { Path = "/pages" };
            var serviceToTest = new PagesDataLoadService(logger, fakeHttpClient, pagesClientOptions, fakeApiDataService, fakeDataLoadService);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).Returns(model);

            Dictionary<Guid, PageLocationModel>? pageLocations = null;

            A.CallTo(() => fakeApiDataService.GetAsync<Dictionary<Guid, PageLocationModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(pageLocations);

            // Act
            await serviceToTest.LoadAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDataLoadService.UpdateAppRegistrationAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeApiDataService.GetAsync<Dictionary<Guid, PageLocationModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PagesDataLoadServiceLoadAsyncRegistrationDocumentUpdated()
        {
            // Arrange
            AppRegistrationModel model = new AppRegistrationModel { Path = "/pages" };
            var serviceToTest = new PagesDataLoadService(logger, fakeHttpClient, pagesClientOptions, fakeApiDataService, fakeDataLoadService);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).Returns(model);
            A.CallTo(() => fakeApiDataService.GetAsync<Dictionary<Guid, PageLocationModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(PageLocationModels);

            // Act
            await serviceToTest.LoadAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDataLoadService.UpdateAppRegistrationAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeApiDataService.GetAsync<Dictionary<Guid, PageLocationModel>>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.NotNull(model.PageLocations);
            Assert.Equal(PageLocationModels.Count, model.PageLocations!.Count);
        }

        [Fact]
        public async Task PagesDataLoadServiceCreateOrUpdateAsyncRegistrationDocumentlocationsUpdated()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            AppRegistrationModel model = new AppRegistrationModel { Path = "/pages", PageLocations = new Dictionary<Guid, PageLocationModel> { { testGuid, new PageLocationModel { Locations = new List<string> { "http://somewhere.com/a-place-a-page" } } } } };
            int expectedPageLocationCount = model.PageLocations.Count;
            var serviceToTest = new PagesDataLoadService(logger, fakeHttpClient, pagesClientOptions, fakeApiDataService, fakeDataLoadService);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).Returns(model);
            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(PageLocationModels.FirstOrDefault().Value);

            // Act
            await serviceToTest.CreateOrUpdateAsync(testGuid).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDataLoadService.UpdateAppRegistrationAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.NotNull(model.PageLocations);
            Assert.Equal(expectedPageLocationCount, model.PageLocations!.Count);
        }

        [Fact]
        public async Task PagesDataLoadServiceCreateOrUpdateAsyncRegistrationDocumentLocationsReplaced()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            AppRegistrationModel model = new AppRegistrationModel { Path = "/pages", PageLocations = null, };
            var serviceToTest = new PagesDataLoadService(logger, fakeHttpClient, pagesClientOptions, fakeApiDataService, fakeDataLoadService);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).Returns(model);
            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(PageLocationModels.FirstOrDefault().Value);

            // Act
            await serviceToTest.CreateOrUpdateAsync(testGuid).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDataLoadService.UpdateAppRegistrationAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.NotNull(model.PageLocations);
            Assert.Single(model.PageLocations!);
        }

        [Fact]
        public async Task PagesDataLoadServiceCreateOrUpdateAsyncNullPagesNoRegistrationDocumentUpdated()
        {
            // Arrange
            AppRegistrationModel model = new AppRegistrationModel { Path = "/pages" };
            var serviceToTest = new PagesDataLoadService(logger, fakeHttpClient, pagesClientOptions, fakeApiDataService, fakeDataLoadService);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).Returns(model);

            PageLocationModel? pageModel = null;

            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(pageModel);

            // Act
            var result = await serviceToTest.CreateOrUpdateAsync(Guid.Parse("66088614-5c55-4698-8382-42b47ec0be10")).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDataLoadService.UpdateAppRegistrationAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PagesDataLoadServiceCreateOrUpdateAsyncNullRegistrationDocumentNoUpdate()
        {
            // Arrange
            AppRegistrationModel? model = null;
            var serviceToTest = new PagesDataLoadService(logger, fakeHttpClient, pagesClientOptions, fakeApiDataService, fakeDataLoadService);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).Returns(model);

            // Act
            await serviceToTest.CreateOrUpdateAsync(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDataLoadService.UpdateAppRegistrationAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task PagesDataLoadServiceRemoveAsyncNullRegistrationDocumentNoUpdate()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NotFound;
            AppRegistrationModel? model = null;
            var serviceToTest = new PagesDataLoadService(logger, fakeHttpClient, pagesClientOptions, fakeApiDataService, fakeDataLoadService);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).Returns(model);

            // Act
            var result = await serviceToTest.RemoveAsync(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDataLoadService.UpdateAppRegistrationAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task PagesDataLoadServiceRemoveAsyncRegistrationDocumentUpdated()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var testGuid = Guid.NewGuid();
            AppRegistrationModel model = new AppRegistrationModel { Path = "/pages", PageLocations = new Dictionary<Guid, PageLocationModel> { { testGuid, new PageLocationModel { Locations = new List<string> { "http://somewhere.com/a-place-a-page" } } } } };
            var serviceToTest = new PagesDataLoadService(logger, fakeHttpClient, pagesClientOptions, fakeApiDataService, fakeDataLoadService);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).Returns(model);
            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(PageLocationModels.FirstOrDefault().Value);

            // Act
            var result = await serviceToTest.RemoveAsync(testGuid).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDataLoadService.UpdateAppRegistrationAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task PagesDataLoadServiceRemoveAsyncRegistrationDocumentUpdatedWhenNullPageLocations()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var testGuid = Guid.NewGuid();
            AppRegistrationModel model = new AppRegistrationModel { Path = "/pages", PageLocations = null, };
            var serviceToTest = new PagesDataLoadService(logger, fakeHttpClient, pagesClientOptions, fakeApiDataService, fakeDataLoadService);
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).Returns(model);
            A.CallTo(() => fakeApiDataService.GetAsync<PageLocationModel>(A<HttpClient>.Ignored, A<Uri>.Ignored)).Returns(PageLocationModels.FirstOrDefault().Value);

            // Act
            var result = await serviceToTest.RemoveAsync(testGuid).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDataLoadService.GetAppRegistrationByPathAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDataLoadService.UpdateAppRegistrationAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }
    }
}
