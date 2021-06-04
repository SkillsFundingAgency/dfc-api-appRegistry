using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Services;
using DFC.Api.AppRegistry.UnitTests.TestModels;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "LegacyDataLoad - Service tests")]
    public class LegacyDataLoadServiceTests
    {
        private readonly ILogger<LegacyDataLoadService> fakeLogger = A.Fake<ILogger<LegacyDataLoadService>>();
        private readonly IModelValidationService fakeModelValidationService = A.Fake<IModelValidationService>();
        private readonly IDocumentService<AppRegistrationModel> fakeDocumentService = A.Fake<IDocumentService<AppRegistrationModel>>();
        private readonly LegacyDataLoadService legacyDataLoadService;

        public LegacyDataLoadServiceTests()
        {
            legacyDataLoadService = new LegacyDataLoadService(fakeLogger, fakeModelValidationService, fakeDocumentService);
        }

        [Fact]
        public async Task GetAppRegistrationByPathAsyncReturnsAppRegistrationModel()
        {
            // Arrange
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(new List<AppRegistrationModel> { new AppRegistrationModel { Path = PagesDataLoadService.AppRegistryPathNameForPagesApp } });

            // Act
            var result = await legacyDataLoadService.GetAppRegistrationByPathAsync("a-path").ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappened();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAppRegistrationByPathAsyncReturnsNull()
        {
            // Arrange
            IEnumerable<AppRegistrationModel>? models = null;
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).Returns(models);

            // Act
            var result = await legacyDataLoadService.GetAppRegistrationByPathAsync("a-path").ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<AppRegistrationModel, bool>>>.Ignored)).MustHaveHappened();
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAppRegistrationAsyncReturnsExceptionForNullAppRegistrationModel()
        {
            // Arrange
            AppRegistrationModel? dummyAppRegistrationModel = null;

            // Act
            var exceptionResult = await Assert.ThrowsAsync<ArgumentNullException>(async () => await legacyDataLoadService.UpdateAppRegistrationAsync(dummyAppRegistrationModel).ConfigureAwait(false)).ConfigureAwait(false);

            // assert
            A.CallTo(() => fakeModelValidationService.ValidateModel(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal("Value cannot be null. (Parameter 'appRegistrationModel')", exceptionResult.Message);
        }

        [Fact]
        public async Task UpdateAppRegistrationAsyncAppRegistrationxxxxxxxxxxxxxxx()
        {
            // Arrange
            const HttpStatusCode upsertResult = HttpStatusCode.OK;
            const bool validationResult = true;
            var validAppRegistrationModel = ModelBuilders.ValidAppRegistrationModel(ModelBuilders.PathName);

            A.CallTo(() => fakeModelValidationService.ValidateModel(A<AppRegistrationModel>.Ignored)).Returns(validationResult);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).Returns(upsertResult);

            // Act
            await legacyDataLoadService.UpdateAppRegistrationAsync(validAppRegistrationModel).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeModelValidationService.ValidateModel(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<AppRegistrationModel>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
