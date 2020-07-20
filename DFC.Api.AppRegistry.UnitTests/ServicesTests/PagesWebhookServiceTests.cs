using Castle.Core.Logging;
using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    public class PagesWebhookServiceTests
    {
        private readonly ILogger<PagesWebhookService> logger;
        private readonly IPagesDataLoadService pageDataLoadService;

        public PagesWebhookServiceTests()
        {
            logger = A.Fake<ILogger<PagesWebhookService>>();
            pageDataLoadService = A.Fake<IPagesDataLoadService>();
        }

        [Fact]
        public async Task PagesWebhookServiceDeleteContentItemAsyncThrowsNotImplementedException()
        {
            // Arrange
            var serviceToTest = new PagesWebhookService(pageDataLoadService, logger);

            // Act
            // Assert
            await Assert.ThrowsAsync<NotImplementedException>(async () => await serviceToTest.DeleteContentItemAsync(Guid.NewGuid()).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PagesWebhookServiceProcessContentItemAsyncThrowsNotImplementedException()
        {
            // Arrange
            var serviceToTest = new PagesWebhookService(pageDataLoadService, logger);

            // Act
            // Assert
            await Assert.ThrowsAsync<NotImplementedException>(async () => await serviceToTest.ProcessContentItemAsync(new Uri("http://somewhere.com"), Guid.NewGuid()).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PagesWebhookServiceProcessMessageAsyncThrowsNotImplementedException()
        {
            // Arrange
            var serviceToTest = new PagesWebhookService(pageDataLoadService, logger);

            // Act
            // Assert
            await Assert.ThrowsAsync<NotImplementedException>(async () => await serviceToTest.ProcessMessageAsync(Compui.Subscriptions.Pkg.Data.Enums.WebhookCacheOperation.CreateOrUpdate, Guid.NewGuid(), Guid.NewGuid(), new Uri("http://somewhere.com")).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PagesWebhookServiceDeleteContentAsyncReturnsOkStatus()
        {
            // Arrange
            var serviceToTest = new PagesWebhookService(pageDataLoadService, logger);
            A.CallTo(() => pageDataLoadService.RemoveAsync(A<Guid>.Ignored)).Returns(System.Net.HttpStatusCode.OK);

            // Act
            var result = await serviceToTest.DeleteContentAsync(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result);
            A.CallTo(() => pageDataLoadService.RemoveAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PagesWebhookServiceProcessContentAsyncReturnsOkStatus()
        {
            // Arrange
            var serviceToTest = new PagesWebhookService(pageDataLoadService, logger);
            A.CallTo(() => pageDataLoadService.CreateOrUpdateAsync(A<Guid>.Ignored)).Returns(System.Net.HttpStatusCode.OK);

            // Act
            var result = await serviceToTest.ProcessContentAsync(new Uri("http://somewhere.com"), Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result);
            A.CallTo(() => pageDataLoadService.CreateOrUpdateAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
