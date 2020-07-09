using DFC.Api.AppRegistry.Functions;
using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "GetList - Http trigger tests")]
    public class GetListHttpTriggerTests
    {
        private readonly ILogger<GetListHttpTrigger> fakeLogger = A.Fake<ILogger<GetListHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Fact]
        public async Task GetListReturnsSuccessWhenDataPresent()
        {
            // Arrange
            var fakeAppRegistrationModels = A.CollectionOfDummy<AppRegistrationModel>(2);
            var expectedResult = new OkObjectResult(fakeAppRegistrationModels);
            var function = new GetListHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAllAsync()).Returns(fakeAppRegistrationModels);

            // Act
            var result = await function.Run(A.Fake<HttpRequest>()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAllAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);

            A.Equals(expectedResult, statusResult);
        }

        [Fact]
        public async Task GetListReturnsnoContentWhenDataNotPresent()
        {
            // Arrange
            var expectedResult = new NoContentResult();
            IEnumerable<AppRegistrationModel>? fakeAppRegistrationModels = null;
            var function = new GetListHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAllAsync()).Returns(fakeAppRegistrationModels);

            // Act
            var result = await function.Run(A.Fake<HttpRequest>()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAllAsync()).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<NoContentResult>(result);

            A.Equals(expectedResult, statusResult);
        }
    }
}
