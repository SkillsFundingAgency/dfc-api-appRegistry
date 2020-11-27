using DFC.Api.AppRegistry.Common;
using DFC.Api.AppRegistry.Contracts;
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
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.FunctionsTests
{
    [Trait("Category", "Post CDN - Http trigger tests")]
    public class CdnUpdatePostHttpTriggerTests
    {
        private readonly ILogger<CdnUpdatePostHttpTrigger> fakeLogger = A.Fake<ILogger<CdnUpdatePostHttpTrigger>>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();
        private readonly IUpdateScriptHashCodeService fakeUpdateScriptHashCodes = A.Fake<IUpdateScriptHashCodeService>();

        [Fact]
        public async Task PostCdnReturnsSuccessResult()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var existingAppRegistrations = BuildExistingAppRegistrations();
            var request = BuildRequestWithModel(new CdnPostModel { Cdn = "https://somewhere.com" });
            var function = new CdnUpdatePostHttpTrigger(fakeLogger, fakeDocumentService, fakeUpdateScriptHashCodes);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(existingAppRegistrations);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(HttpStatusCode.OK);
            A.CallTo(() => fakeUpdateScriptHashCodes.UpdateAllAsync(A<string>.Ignored)).Returns(HttpStatusCode.OK);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeUpdateScriptHashCodes.UpdateAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostCdnReturnsBadRequestWhenRequestIsNull()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var function = new CdnUpdatePostHttpTrigger(fakeLogger, fakeDocumentService, fakeUpdateScriptHashCodes);

            // Act
            var result = await function.Run(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeUpdateScriptHashCodes.UpdateAllAsync(A<string>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostCdnReturnsBadRequestWhenNullBody()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            var function = new CdnUpdatePostHttpTrigger(fakeLogger, fakeDocumentService, fakeUpdateScriptHashCodes);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeUpdateScriptHashCodes.UpdateAllAsync(A<string>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostCdnReturnsBadRequestWhenInvalidlModel()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var request = BuildRequestWithModel(new CdnPostModel());
            var function = new CdnUpdatePostHttpTrigger(fakeLogger, fakeDocumentService, fakeUpdateScriptHashCodes);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeUpdateScriptHashCodes.UpdateAllAsync(A<string>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostCdnReturnsNoContentWhenShellDoesNotExist()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            IEnumerable<AppRegistrationModel>? nullAppRegistrations = null;
            var request = BuildRequestWithModel(new CdnPostModel { Cdn = "https://somewhere.com" });
            var function = new CdnUpdatePostHttpTrigger(fakeLogger, fakeDocumentService, fakeUpdateScriptHashCodes);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(nullAppRegistrations);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeUpdateScriptHashCodes.UpdateAllAsync(A<string>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<NoContentResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostCdnReturnsUnprocessableEntityResultWhenUpsertFails()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var existingAppRegistrations = BuildExistingAppRegistrations();
            var request = BuildRequestWithModel(new CdnPostModel { Cdn = "https://somewhere.com" });
            var function = new CdnUpdatePostHttpTrigger(fakeLogger, fakeDocumentService, fakeUpdateScriptHashCodes);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(existingAppRegistrations);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(HttpStatusCode.BadRequest);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeUpdateScriptHashCodes.UpdateAllAsync(A<string>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<UnprocessableEntityResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostCdnReturnsUnprocessableEntityResultWhenRaisesException()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var existingAppRegistrations = BuildExistingAppRegistrations();
            var request = BuildRequestWithModel(new CdnPostModel { Cdn = "https://somewhere.com" });
            var function = new CdnUpdatePostHttpTrigger(fakeLogger, fakeDocumentService, fakeUpdateScriptHashCodes);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(existingAppRegistrations);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Throws<Exception>();

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeUpdateScriptHashCodes.UpdateAllAsync(A<string>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<UnprocessableEntityResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostCdnReturnsUnprocessableEntityResultWhenHashUpdateFails()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.UnprocessableEntity;
            var existingAppRegistrations = BuildExistingAppRegistrations();
            var request = BuildRequestWithModel(new CdnPostModel { Cdn = "https://somewhere.com" });
            var function = new CdnUpdatePostHttpTrigger(fakeLogger, fakeDocumentService, fakeUpdateScriptHashCodes);

            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(existingAppRegistrations);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(HttpStatusCode.OK);
            A.CallTo(() => fakeUpdateScriptHashCodes.UpdateAllAsync(A<string>.Ignored)).Returns(HttpStatusCode.BadRequest);

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeUpdateScriptHashCodes.UpdateAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<UnprocessableEntityResult>(result);

            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        private static IEnumerable<AppRegistrationModel> BuildExistingAppRegistrations()
        {
            return new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    Path = Constants.PathNameForShell,
                },
            };
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
