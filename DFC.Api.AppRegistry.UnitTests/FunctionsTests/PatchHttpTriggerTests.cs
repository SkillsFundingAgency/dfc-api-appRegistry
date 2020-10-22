using DFC.Api.AppRegistry.Enums;
using DFC.Api.AppRegistry.Functions;
using DFC.Api.AppRegistry.Models;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.JsonPatch;
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

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "Patch - Http trigger tests")]
    public class PatchHttpTriggerTests
    {
        private const string PathName = "unit-tests";
        private readonly ILogger<PatchHttpTrigger> fakeLogger = A.Fake<ILogger<PatchHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PatchReturnsSuccessWhenValid(bool isOnline)
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithPatchObject<AppRegistrationModel, bool>(x => x.IsOnline, isOnline);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
            Assert.Equal(isOnline, validAppRegistrationModels.First().IsOnline);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenNullRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            HttpRequest? request = null;
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenNullBody()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenNullPath()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = BuildRequestWithPatchObject<AppRegistrationModel, bool>(x => x.IsOnline, true);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, string.Empty).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenEmptyBodyObject()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = BuildRequestWithInvalidBody(string.Empty);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenInvalidBodyObject()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = BuildRequestWithInvalidBody("some rubbish patch string");
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsNoContentWhenAppRegistrationDoesNotExist()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            IEnumerable<AppRegistrationModel>? invalidAppRegistrationModel = null;
            var request = BuildRequestWithPatchObject<AppRegistrationModel, bool>(x => x.IsOnline, true);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(invalidAppRegistrationModel);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<NoContentResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenValidationErrorsExist()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var invalidAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithPatchObject<AppRegistrationModel, string?>(x => x.Path, string.Empty);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(invalidAppRegistrationModels);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<UnprocessableEntityResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenValidationRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithPatchObject<AppRegistrationModel, Guid?>(x => x.Id, null);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);

            // Act
            var result = await function.Run(request, PathName).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsUnprocessableEntityResultWhenUpsertRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithPatchObject<AppRegistrationModel, bool>(x => x.IsOnline, true);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).ThrowsAsync(new ApplicationException());

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
                new AppRegistrationModel
                {
                    Id = Guid.NewGuid(),
                    Path = PathName,
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            PageRegion = PageRegion.Body,
                            RegionEndpoint = "https://somewhere.com",
                            HealthCheckRequired = true,
                        },
                        new RegionModel
                        {
                            PageRegion = PageRegion.BodyTop,
                            RegionEndpoint = "https://somewhere.com",
                        },
                    },
                },
                new AppRegistrationModel
                {
                    Id = Guid.NewGuid(),
                    Path = PathName + "-2",
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            PageRegion = PageRegion.Body,
                            RegionEndpoint = "https://somewhere.com",
                            HealthCheckRequired = true,
                        },
                        new RegionModel
                        {
                            PageRegion = PageRegion.BodyTop,
                            RegionEndpoint = "https://somewhere.com",
                        },
                    },
                },
            };
        }

        private static HttpRequest BuildRequestWithPatchObject<TModel, TProp>(Expression<Func<TModel, TProp>> path, TProp value)
            where TModel : class
        {
            var patchDocument = new JsonPatchDocument<TModel>();
            patchDocument.Add(path, value);

            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = BuildStreamFromModel(patchDocument),
            };
        }

        private static HttpRequest BuildRequestWithInvalidBody(string bodyString)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyString)),
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
