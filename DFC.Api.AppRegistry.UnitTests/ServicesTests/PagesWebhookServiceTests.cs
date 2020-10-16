using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
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
        public async Task PagesWebhookServiceProcessMessageNoneOperationReturnsOk()
        {
            // Arrange
            var serviceToTest = new PagesWebhookService(pageDataLoadService, logger);

            // Act
            var result = await serviceToTest.ProcessMessageAsync(Compui.Subscriptions.Pkg.Data.Enums.WebhookCacheOperation.None, Guid.NewGuid(), Guid.NewGuid(), new Uri("http://somewhere.com")).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
        }

        [Fact]
        public async Task PagesWebhookServiceProcessMessageCreateOrUpdateOperationReturnsOk()
        {
            // Arrange
            var serviceToTest = new PagesWebhookService(pageDataLoadService, logger);
            A.CallTo(() => pageDataLoadService.CreateOrUpdateAsync(A<Guid>.Ignored)).Returns(HttpStatusCode.OK);

            // Act
            var result = await serviceToTest.ProcessMessageAsync(Compui.Subscriptions.Pkg.Data.Enums.WebhookCacheOperation.CreateOrUpdate, Guid.NewGuid(), Guid.NewGuid(), new Uri("http://somewhere.com")).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
        }

        [Fact]
        public async Task PagesWebhookServiceProcessMessageRemoveOperationReturnsOk()
        {
            // Arrange
            var serviceToTest = new PagesWebhookService(pageDataLoadService, logger);
            A.CallTo(() => pageDataLoadService.RemoveAsync(A<Guid>.Ignored)).Returns(HttpStatusCode.OK);

            // Act
            var result = await serviceToTest.ProcessMessageAsync(Compui.Subscriptions.Pkg.Data.Enums.WebhookCacheOperation.Delete, Guid.NewGuid(), Guid.NewGuid(), new Uri("http://somewhere.com")).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result);
        }
    }
}
