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
    public class PatchRegionHttpTriggerTests
    {
        private const string PathName = "unit-tests";
        private const PageRegion ValidPageRegionValue = PageRegion.Body;
        private const PageRegion InvalidPageRegionValue = PageRegion.Head;
        private const PageRegion NonExistingPageRegionValue = PageRegion.Footer;
        private readonly ILogger<PatchRegionHttpTrigger> fakeLogger = A.Fake<ILogger<PatchRegionHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PatchReturnsSuccessWhenValid(bool isHealthy)
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, isHealthy);
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
            Assert.Equal(isHealthy, validAppRegistrationModels.First().Regions.FirstOrDefault(f => f.PageRegion == ValidPageRegionValue).IsHealthy);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenNullRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            HttpRequest? request = null;
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

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
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

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
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, string.Empty, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenInvalidPageRegion()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName, 999).ConfigureAwait(false);

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
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

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
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

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
            IEnumerable<AppRegistrationModel>? invalidAppRegistrationModels = null;
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(invalidAppRegistrationModels);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<NoContentResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsNoContentWhenRegionDoesNotExist()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);

            // Act
            var result = await function.Run(request, PathName, (int)NonExistingPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<NoContentResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsUnprocessableEntityWhenValidationErrorsExist()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var validAppRegistrationModels = ValidAppRegistrationModels();
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);

            // Act
            var result = await function.Run(request, PathName, (int)InvalidPageRegionValue).ConfigureAwait(false);

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
            var request = BuildRequestWithPatchObject<RegionModel, PageRegion?>(x => x.PageRegion, null);
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

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
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchRegionHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModels);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).ThrowsAsync(new ApplicationException());

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

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
                    Path = PathName,
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            PageRegion = ValidPageRegionValue,
                            RegionEndpoint = "https://somewhere.com",
                            HealthCheckRequired = true,
                        },
                        new RegionModel
                        {
                            PageRegion = InvalidPageRegionValue,
                        },
                    },
                },
                new AppRegistrationModel
                {
                    Path = PathName + "-1",
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            PageRegion = ValidPageRegionValue,
                            RegionEndpoint = "https://somewhere.com",
                            HealthCheckRequired = true,
                        },
                        new RegionModel
                        {
                            PageRegion = InvalidPageRegionValue,
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
