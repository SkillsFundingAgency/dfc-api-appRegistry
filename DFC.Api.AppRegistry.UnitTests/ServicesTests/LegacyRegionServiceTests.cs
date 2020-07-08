using DFC.Api.AppRegistry.Models.Legacy;
using DFC.Api.AppRegistry.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    public class LegacyRegionServiceTests
    {
        [Fact]
        public async Task GetListAsyncIsSuccessful()
        {
            // Arrange
            const string path = "unit-test";
            var expectedResult = new List<LegacyRegionModel>();
            var service = new LegacyRegionService();

            // Act
            var result = await service.GetListAsync(path).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
