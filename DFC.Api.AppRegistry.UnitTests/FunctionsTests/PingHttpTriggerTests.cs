using DFC.Api.AppRegistry.Functions;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "Ping - Http trigger tests")]
    public class PingHttpTriggerTests
    {
        private readonly ILogger<PingHttpTrigger> fakeLogger = A.Fake<ILogger<PingHttpTrigger>>();

        [Fact]
        public void PingIsSuccessful()
        {
            // Arrange
            var expectedResult = new OkResult();
            var function = new PingHttpTrigger(fakeLogger);

            // Act
            var result = function.Ping(A.Fake<HttpRequest>());

            // Assert
            var statusResult = Assert.IsType<OkResult>(result);

            A.Equals(expectedResult, statusResult);
        }
    }
}
