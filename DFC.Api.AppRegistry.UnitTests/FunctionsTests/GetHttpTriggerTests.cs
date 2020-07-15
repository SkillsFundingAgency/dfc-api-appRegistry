using DFC.Api.AppRegistry.Functions;
using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "Get - Http trigger tests")]
    public class GetHttpTriggerTests
    {
        private const string PathName = "unit-tests";
        private readonly ILogger<GetHttpTrigger> fakeLogger = A.Fake<ILogger<GetHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Fact]
        public async Task GetReturnsSuccessWhenDataPresent()
        {
            // Arrange
            var expectedResult = A.Dummy<AppRegistrationModel>();
            var function = new GetHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(expectedResult);

            // Act
            var result = await function.Run(A.Fake<HttpRequest>(), PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetReturnsNullWhenDataNotPresent()
        {
            // Arrange
            AppRegistrationModel? fakeAppRegistrationModel = null;
            var expectedResult = new OkObjectResult(null);
            var function = new GetHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(fakeAppRegistrationModel);

            // Act
            var result = await function.Run(A.Fake<HttpRequest>(), PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);

            A.Equals(expectedResult, statusResult);
        }
    }
}
