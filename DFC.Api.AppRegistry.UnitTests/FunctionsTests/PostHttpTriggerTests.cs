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
    [Trait("Category", "Post - Http trigger tests")]
    public class PostHttpTriggerTests
    {
        private const string PathName = "unit-tests";
        private readonly ILogger<PostHttpTrigger> fakeLogger = A.Fake<ILogger<PostHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Fact]
        public async Task PostReturnsSuccessWhenValidAndCreated()
        {
            // Arrange
            var expectedResult = HttpStatusCode.Created;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithModel(validAppRegistrationModels.First());
            var function = new PostHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(expectedResult);
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<CreatedResult>(result);
            var createdAppRegistrationModel = statusResult.Value as AppRegistrationModel;

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
            Assert.Equal(validAppRegistrationModels.First(), createdAppRegistrationModel);
        }

        [Fact]
        public async Task PostReturnsSuccessWhenValidAndUpdated()
        {
            // Arrange
            var expectedResult = HttpStatusCode.OK;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithModel(validAppRegistrationModels.First());
            var function = new PostHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(expectedResult);
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<OkResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostReturnsBadRequestWhenNullRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            HttpRequest? request = null;
            var function = new PostHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostReturnsBadRequestWhenNullBody()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            var function = new PostHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostReturnsUnprocessableEntityWhenUpsertFails()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var validAppRegistrationModel = ValidAppRegistrationModel(PathName);
            var request = BuildRequestWithModel(validAppRegistrationModel);
            var function = new PostHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<UnprocessableEntityResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostReturnsUnprocessableEntityWhenUpsertRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var validAppRegistrationModel = ValidAppRegistrationModel(PathName);
            var request = BuildRequestWithModel(validAppRegistrationModel);
            var function = new PostHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Throws<Exception>();

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();

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

        private static HttpRequest BuildRequestWithModel<TModel>(TModel model)
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
