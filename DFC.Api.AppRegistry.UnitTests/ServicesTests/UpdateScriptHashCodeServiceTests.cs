using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.ClientOptions;
using DFC.Api.AppRegistry.Services;
using DFC.Api.AppRegistry.UnitTests.FakeHttpHandlers;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "Update Script Hashcodes Service Unit Tests")]
    public class UpdateScriptHashCodeServiceTests
    {
        private const string DefaultCdnLocation = "https://somewhere.com";
        private readonly ILogger<UpdateScriptHashCodeService> fakeLogger = A.Fake<ILogger<UpdateScriptHashCodeService>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Fact]
        public async Task UpdateAllAsyncReturnsSuccessWithAppRegistrations()
        {
            // arrange
            const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
            var nullAppRegistrationModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    Path = "a-path",
                },
            };

            A.CallTo(() => fakeDocumentService.GetAllAsync(A<string>.Ignored)).Returns(nullAppRegistrationModels);

            var service = BuildServiceToTest();

            // act
            var result = await service.UpdateAllAsync(DefaultCdnLocation).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedStatusCode, result);
        }

        [Fact]
        public async Task UpdateAllAsyncReturnsExceptionWhenNoCdnLocation()
        {
            // arrange
            var service = BuildServiceToTest();

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.UpdateAllAsync(string.Empty).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal("Value cannot be null. (Parameter 'cdnLocation')", exceptionResult.Message);
        }

        [Fact]
        public async Task UpdateAllAsyncReturnsNoContentWhenNoAppRegistrations()
        {
            // arrange
            const HttpStatusCode expectedStatusCode = HttpStatusCode.NoContent;
            IEnumerable<AppRegistrationModel>? nullAppRegistrationModels = null;

            A.CallTo(() => fakeDocumentService.GetAllAsync(A<string>.Ignored)).Returns(nullAppRegistrationModels);

            var service = BuildServiceToTest();

            // act
            var result = await service.UpdateAllAsync(DefaultCdnLocation).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedStatusCode, result);
        }

        [Fact]
        public async Task UpdateHashCodesAsyncReturnsIsSuccessfulWithUpdates()
        {
            // arrange
            var appRegistrationModel = new AppRegistrationModel
            {
                Path = "a-path",
                JavaScriptNames = new Dictionary<string, string?>
                    {
                        { "/a-file-location", null },
                    },
            };

            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(HttpStatusCode.OK);

            var service = BuildServiceToTest();

            // act
            await service.UpdateHashCodesAsync(appRegistrationModel, DefaultCdnLocation).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateHashCodesAsyncReturnsNoContentWhenNoAppRegistrations()
        {
            // arrange
            AppRegistrationModel? nullAppRegistrationModel = null;

            var service = BuildServiceToTest();

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.UpdateHashCodesAsync(nullAppRegistrationModel, DefaultCdnLocation).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal("Value cannot be null. (Parameter 'appRegistrationModel')", exceptionResult.Message);
        }

        [Fact]
        public async Task UpdateHashCodesAsyncReturnsExceptionWhenNoCdnLocation()
        {
            // arrange
            var appRegistrationModel = new AppRegistrationModel
            {
                Path = "a-path",
                JavaScriptNames = new Dictionary<string, string?>
                    {
                        { "/a-file-location", null },
                    },
            };

            var service = BuildServiceToTest();

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.UpdateHashCodesAsync(appRegistrationModel, string.Empty).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal("Value cannot be null. (Parameter 'cdnLocation')", exceptionResult.Message);
        }

        [Fact]
        public async Task UpdateHashCodesAsyncReturnsIsNotSuccessfulWithUpdates()
        {
            // arrange
            var appRegistrationModel = new AppRegistrationModel
            {
                Path = "a-path",
                JavaScriptNames = new Dictionary<string, string?>
                    {
                        { "/a-file-location", null },
                    },
            };

            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(HttpStatusCode.BadRequest);

            var service = BuildServiceToTest();

            // act
            await service.UpdateHashCodesAsync(appRegistrationModel, DefaultCdnLocation).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateHashCodesAsyncReturnsIsSuccessfulWhenNoUpdates()
        {
            // arrange
            var appRegistrationModel = new AppRegistrationModel
            {
                Path = "a-path",
                JavaScriptNames = new Dictionary<string, string?> { },
            };

            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(HttpStatusCode.OK);

            var service = BuildServiceToTest();

            // act
            await service.UpdateHashCodesAsync(appRegistrationModel, DefaultCdnLocation).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task RefreshHashcodesAsyncReturnsSuccessfulCount()
        {
            // arrange
            const int expectedCount = 3;
            var appRegistrationModel = new AppRegistrationModel
            {
                Path = "a-path",
                JavaScriptNames = new Dictionary<string, string?>
                    {
                        { "/a-file-location", null },
                        { "/another-file-location", "a-hash-code" },
                        { "http://somewhere.com//a-file-location", "a-hash-code" },
                    },
            };

            var service = BuildServiceToTest();

            // act
            var result = await service.RefreshHashcodesAsync(appRegistrationModel, DefaultCdnLocation).ConfigureAwait(false);

            // assert
            Assert.Equal(expectedCount, result);
        }

        [Fact]
        public async Task RefreshHashcodesAsyncReturnsSuccessfulZeroCount()
        {
            // arrange
            const int expectedCount = 0;
            var appRegistrationModel = new AppRegistrationModel
            {
                Path = "a-path",
                JavaScriptNames = new Dictionary<string, string?> { },
            };

            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(HttpStatusCode.OK);

            var service = BuildServiceToTest();

            // act
            var result = await service.RefreshHashcodesAsync(appRegistrationModel, DefaultCdnLocation).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedCount, result);
        }

        [Fact]
        public async Task RefreshHashcodesAsyncReturnsExceptionWhenNoAppRegistrations()
        {
            // arrange
            AppRegistrationModel? nullAppRegistrationModel = null;

            var service = BuildServiceToTest();

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.RefreshHashcodesAsync(nullAppRegistrationModel, DefaultCdnLocation).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal("Value cannot be null. (Parameter 'appRegistrationModel')", exceptionResult.Message);
        }

        [Fact]
        public async Task RefreshHashcodesAsyncReturnsExceptionWhenNoCdnLocation()
        {
            // arrange
            var appRegistrationModel = new AppRegistrationModel
            {
                Path = "a-path",
                JavaScriptNames = new Dictionary<string, string?> { },
            };

            var service = BuildServiceToTest();

            // act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.RefreshHashcodesAsync(appRegistrationModel, string.Empty).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal("Value cannot be null. (Parameter 'cdnLocation')", exceptionResult.Message);
        }

        [Fact]
        public async Task GetFileHashAsyncReturnsSuccessful()
        {
            // arrange
            var service = BuildServiceToTest();

            // act
            var result = await service.GetFileHashAsync(new Uri("http://somewhere.com/an-asset.js", UriKind.Absolute)).ConfigureAwait(false);

            // assert
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        private UpdateScriptHashCodeService BuildServiceToTest()
        {
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);
            var updateScriptHashCodeClientOptions = new UpdateScriptHashCodeClientOptions();

            return new UpdateScriptHashCodeService(fakeLogger, httpClient, updateScriptHashCodeClientOptions, fakeDocumentService);
        }
    }
}
