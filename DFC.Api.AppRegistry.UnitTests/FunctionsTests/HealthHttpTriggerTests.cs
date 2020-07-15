using DFC.Api.AppRegistry.Functions;
using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "Health - Http trigger tests")]
    public class HealthHttpTriggerTests
    {
        private readonly ILogger<HealthHttpTrigger> fakeLogger = A.Fake<ILogger<HealthHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Fact]
        public async Task HealthReturnsSuccessWhenHealthy()
        {
            // Arrange
            var expectedResult = new OkResult();
            var function = new HealthHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.PingAsync()).Returns(true);

            // Act
            var result = await function.Health(A.Fake<HttpRequest>()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.PingAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task HealthReturnsServiceUnavailableWhenUnhealthy()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            var function = new HealthHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.PingAsync()).Returns(false);

            // Act
            var result = await function.Health(A.Fake<HttpRequest>()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.PingAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<StatusCodeResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task HealthReturnsServiceUnavailableWhenException()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            var function = new HealthHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.PingAsync()).Throws<HttpRequestException>();

            // Act
            var result = await function.Health(A.Fake<HttpRequest>()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.PingAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<StatusCodeResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }
    }
}
