using DFC.Api.AppRegistry.Functions;
using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    public class GetHttpTriggerTests
    {
        private readonly ILogger<GetHttpTrigger> fakeLogger = A.Fake<ILogger<GetHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Fact]
        public async Task GetReturnsSuccessWhenDataPresent()
        {
            // Arrange
            const string path = "unit-tests";
            var fakeAppRegistrationModels = A.CollectionOfDummy<AppRegistrationModel>(2);
            var expectedResult = new OkObjectResult(fakeAppRegistrationModels.First());
            var function = new GetHttpTrigger(fakeLogger, fakeDocumentService);

            fakeAppRegistrationModels.First().Path = path;

            A.CallTo(() => fakeDocumentService.GetAllAsync()).Returns(fakeAppRegistrationModels);

            // Act
            var result = await function.Run(A.Fake<HttpRequest>(), path).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAllAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);

            A.Equals(expectedResult, statusResult);
        }

        [Fact]
        public async Task GetReturnsNullWhenDatanotPresent()
        {
            // Arrange
            const string path = "unit-tests";
            IEnumerable<AppRegistrationModel>? fakeAppRegistrationModels = null;
            var expectedResult = new OkObjectResult(null);
            var function = new GetHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAllAsync()).Returns(fakeAppRegistrationModels);

            // Act
            var result = await function.Run(A.Fake<HttpRequest>(), path).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAllAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);

            A.Equals(expectedResult, statusResult);
        }
    }
}
