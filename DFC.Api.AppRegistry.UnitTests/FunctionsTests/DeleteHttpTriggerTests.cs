using DFC.Api.AppRegistry.Enums;
using DFC.Api.AppRegistry.Functions;
using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.FunctionsTests
{
    [Trait("Category", "Delete - Http trigger tests")]
    public class DeleteHttpTriggerTests
    {
        private const string PathName = "unit-tests";
        private readonly ILogger<DeleteHttpTrigger> fakeLogger = A.Fake<ILogger<DeleteHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Fact]
        public async Task DeleteReturnsSuccessWhenValidAndDeleted()
        {
            // Arrange
            var expectedResult = HttpStatusCode.OK;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithMmodel(validAppRegistrationModels.First());
            var function = new DeleteHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);
            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task DeleteReturnsBadRequestWhenPathIsEmpty()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var validAppRegistrationModel = ValidAppRegistrationModel(PathName);
            var request = BuildRequestWithMmodel(validAppRegistrationModel);
            var function = new DeleteHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, string.Empty).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task DeleteReturnsNoContentWhenPathDoesNotExist()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            var validAppRegistrationModel = ValidAppRegistrationModel(PathName);
            IEnumerable<AppRegistrationModel>? nullAppRegistrationModel = null;
            var request = BuildRequestWithMmodel(validAppRegistrationModel);
            var function = new DeleteHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(nullAppRegistrationModel);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<NoContentResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task DeleteReturnsUnprocessableEntityWhenDeleteFails()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithMmodel(validAppRegistrationModels.First());
            var function = new DeleteHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);
            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(false);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<UnprocessableEntityResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task DeleteReturnsUnprocessableEntityWhenDeleteRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithMmodel(validAppRegistrationModels.First());
            var function = new DeleteHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);
            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).Throws<Exception>();

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<UnprocessableEntityResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        private static IEnumerable<AppRegistrationModel> ValidAppRegistrationModels()
        {
            return new List<AppRegistrationModel>
            {
                ValidAppRegistrationModel(PathName),
                ValidAppRegistrationModel(PathName + "-2"),
            };
        }

        private static AppRegistrationModel ValidAppRegistrationModel(string path)
        {
            return new AppRegistrationModel
            {
                Id = Guid.NewGuid(),
                Path = path,
                Regions = new List<RegionModel>
                {
                    new RegionModel
                    {
                        PageRegion = PageRegion.Body,
                        RegionEndpoint = "https://somewhere.com/body",
                        HealthCheckRequired = true,
                    },
                    new RegionModel
                    {
                        PageRegion = PageRegion.Breadcrumb,
                        RegionEndpoint = "https://somewhere.com/breadcrumb",
                        HealthCheckRequired = true,
                    },
                },
            };
        }

        private static HttpRequest BuildRequestWithMmodel<TModel>(TModel model)
          where TModel : class
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = BuildStreamFromModel(model),
            };
        }

        private static Stream BuildStreamFromModel<TModel>(TModel model)
        {
            var jsonData = JsonConvert.SerializeObject(model);
            byte[] byteArray = Encoding.ASCII.GetBytes(jsonData);

            return new MemoryStream(byteArray);
        }
    }
}
