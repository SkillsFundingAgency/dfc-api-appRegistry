using DFC.Api.AppRegistry.Models.Legacy;
using DFC.Api.AppRegistry.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    public class LegacyPathServiceTests
    {
        [Fact]
        public async Task GetListAsyncIsSuccessful()
        {
            // Arrange
            var expectedResult = new List<LegacyPathModel>();
            var service = new LegacyPathService();

            // Act
            var result = await service.GetListAsync().ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
