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
    [Trait("Category", "LegacyDataLoad - Http trigger tests")]
    public class LegacyDataLoadHttpTriggerTests
    {
        private readonly ILogger<LegacyDataLoadHttpTrigger> fakeLogger = A.Fake<ILogger<LegacyDataLoadHttpTrigger>>();
        private readonly ILegacyDataLoadService fakeLegacyDataLoadService = A.Fake<ILegacyDataLoadService>();

        [Fact]
        public async Task GetReturnsSuccessWhenDataPresent()
        {
            // Arrange
            var expectedResult = new OkResult();
            var function = new LegacyDataLoadHttpTrigger(fakeLogger, fakeLegacyDataLoadService);

            A.CallTo(() => fakeLegacyDataLoadService.LoadAsync());

            // Act
            var result = await function.Run(A.Fake<HttpRequest>()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLegacyDataLoadService.LoadAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkResult>(result);

            A.Equals(expectedResult, statusResult);
        }
    }
}
