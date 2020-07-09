using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Services;
using DFC.Api.AppRegistry.UnitTests.TestModels;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "ModelValidation - Service tests")]
    public class ModelValidationServiceTests
    {
        private readonly ILogger<ModelValidationService> fakeLogger = A.Fake<ILogger<ModelValidationService>>();
        private readonly ModelValidationService modelValidationService;

        public ModelValidationServiceTests()
        {
            modelValidationService = new ModelValidationService(fakeLogger);
        }

        [Fact]
        public void ValidateModelReturnsExceptionForNullAppRegistrationModel()
        {
            // Arrange
            AppRegistrationModel? appRegistrationModel = null;

            // Act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => modelValidationService.ValidateModel(appRegistrationModel));

            // assert
            Assert.Equal("Value cannot be null. (Parameter 'appRegistrationModel')", exceptionResult.Message);
        }

        [Fact]
        public void ValidateModelReturnsSuccessForValidDataModels()
        {
            // Arrange
            const bool expectedResult = true;
            var appRegistrationModel = ModelBuilders.ValidAppRegistrationModel(ModelBuilders.PathName);

            // Act
            var result = modelValidationService.ValidateModel(appRegistrationModel);

            // assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ValidateModelReturnsFailureForInvalidDataModels()
        {
            // Arrange
            const bool expectedResult = false;
            var appRegistrationModel = ModelBuilders.ValidAppRegistrationModel(ModelBuilders.PathName);

            appRegistrationModel.Path = null;

            // Act
            var result = modelValidationService.ValidateModel(appRegistrationModel);

            // assert
            Assert.Equal(expectedResult, result);
        }
    }
}
