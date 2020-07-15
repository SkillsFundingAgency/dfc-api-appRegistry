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
        private const PageRegion ValidPageRegionValue = PageRegion.Body;
        private const PageRegion InvalidPageRegionValue = PageRegion.Head;
        private const PageRegion NonExistingPageRegionValue = PageRegion.Footer;
        private readonly ILogger<PatchHttpTrigger> fakeLogger = A.Fake<ILogger<PatchHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PatchReturnsSuccessWhenValid(bool isHealthy)
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var validAppRegistrationModel = ValidAppRegistrationModel();
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, isHealthy);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModel);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
            A.Equals(isHealthy, validAppRegistrationModel.Regions.FirstOrDefault(f => f.PageRegion == ValidPageRegionValue).IsHealthy);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenNullRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            HttpRequest? request = null;
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenNullBody()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenNullPath()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, string.Empty, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenInvalidPageRegion()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName, 999).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenEmptyBodyObject()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = BuildRequestWithInvalidBody(string.Empty);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsBadRequestWhenInvalidBodyObject()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = BuildRequestWithInvalidBody("some rubbish patch string");
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsNoContentWhenAppRegistrationDoesNotExist()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            AppRegistrationModel? invalidAppRegistrationModel = null;
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(invalidAppRegistrationModel);

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<NoContentResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsNoContentWhenRegionDoesNotExist()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            var validAppRegistrationModel = ValidAppRegistrationModel();
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModel);

            // Act
            var result = await function.Run(request, PathName, (int)NonExistingPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<NoContentResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsUnprocessableEntityWhenRegionDoesNotExist()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var validAppRegistrationModel = ValidAppRegistrationModel();
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModel);

            // Act
            var result = await function.Run(request, PathName, (int)InvalidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<UnprocessableEntityResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PatchReturnsUnprocessableEntityResultWhenUpsertRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var validAppRegistrationModel = ValidAppRegistrationModel();
            var request = BuildRequestWithPatchObject<RegionModel, bool>(x => x.IsHealthy, true);
            var function = new PatchHttpTrigger(fakeLogger, fakeDocumentService);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(validAppRegistrationModel);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).ThrowsAsync(new ApplicationException());

            // Act
            var result = await function.Run(request, PathName, (int)ValidPageRegionValue).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<UnprocessableEntityResult>(result);

            A.Equals(expectedResult, statusResult.StatusCode);
        }

        private static AppRegistrationModel ValidAppRegistrationModel()
        {
            return new AppRegistrationModel
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
