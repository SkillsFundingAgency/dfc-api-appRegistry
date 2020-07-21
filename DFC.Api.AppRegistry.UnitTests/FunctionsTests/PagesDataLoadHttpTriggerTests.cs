using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Functions;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "PagesDataLoad - Http trigger tests")]
    public class PagesDataLoadHttpTriggerTests
    {
        private readonly ILogger<PagesDataLoadHttpTrigger> fakeLogger = A.Fake<ILogger<PagesDataLoadHttpTrigger>>();
        private readonly IPagesDataLoadService fakePagesDataLoadService = A.Fake<IPagesDataLoadService>();

        [Fact]
        public async Task GetReturnsSuccessWhenDataPresent()
        {
            // Arrange
            var expectedResult = new OkResult();
            var function = new PagesDataLoadHttpTrigger(fakeLogger, fakePagesDataLoadService);

            A.CallTo(() => fakePagesDataLoadService.LoadAsync());

            // Act
            var result = await function.Run(A.Fake<HttpRequest>()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakePagesDataLoadService.LoadAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }
    }
}
