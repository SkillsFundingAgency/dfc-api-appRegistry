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
    [Trait("Category", "Put - Http trigger tests")]
    public class PutHttpTriggerTests
    {
        private const string PathName = "unit-tests";
        private readonly ILogger<PutHttpTrigger> fakeLogger = A.Fake<ILogger<PutHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Fact]
        public async Task PutReturnsSuccessWhenValidAndUpdated()
        {
            // Arrange
            var expectedResult = HttpStatusCode.OK;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithMmodel(validAppRegistrationModels.First());
            var function = new PutHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PutReturnsBadRequestWhenNullRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            HttpRequest? request = null;
            var function = new PutHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PutReturnsBadRequestWhenPathIsEmpty()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var validAppRegistrationModel = ValidAppRegistrationModel(PathName);
            var request = BuildRequestWithMmodel(validAppRegistrationModel);
            var function = new PutHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, string.Empty).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PutReturnsBadRequestWhenNullBody()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            var function = new PutHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PutReturnsBadRequestWhenInvalidBodyObject()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = BuildRequestWithMmodel(InvalidAppRegistrationModel());
            var function = new PutHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PutReturnsBadRequestWhenPathValueMismatch()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var validAppRegistrationModel = ValidAppRegistrationModel(PathName);
            var request = BuildRequestWithMmodel(validAppRegistrationModel);
            var function = new PutHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName + "-bad-path").ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PutReturnsNoContentWhenPathDoesNotExist()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            var validAppRegistrationModel = ValidAppRegistrationModel(PathName);
            IEnumerable<AppRegistrationModel>? nullAppRegistrationModel = null;
            var request = BuildRequestWithMmodel(validAppRegistrationModel);
            var function = new PutHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(nullAppRegistrationModel);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<NoContentResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PutReturnsUnprocessableEntityWhenUpsertFails()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var validAppRegistrationModel = ValidAppRegistrationModel(PathName);
            var request = BuildRequestWithMmodel(validAppRegistrationModel);
            var function = new PutHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<UnprocessableEntityResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PutReturnsUnprocessableEntityWhenUpsertRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var validAppRegistrationModel = ValidAppRegistrationModel(PathName);
            var request = BuildRequestWithMmodel(validAppRegistrationModel);
            var function = new PutHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Throws<Exception>();

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();

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

        private static AppRegistrationModel InvalidAppRegistrationModel()
        {
            return new AppRegistrationModel();
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
