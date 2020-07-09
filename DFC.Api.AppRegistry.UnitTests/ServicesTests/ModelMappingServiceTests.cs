using AutoMapper;
using DFC.Api.AppRegistry.Models;
using DFC.Api.AppRegistry.Models.Legacy;
using DFC.Api.AppRegistry.Services;
using DFC.Api.AppRegistry.UnitTests.TestModels;
using FakeItEasy;
using System;
using System.Collections.Generic;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "ModelMapping - Service tests")]
    public class ModelMappingServiceTests
    {
        private readonly IMapper fakeMapper = A.Fake<IMapper>();
        private readonly ModelMappingService modelMappingService;

        public ModelMappingServiceTests()
        {
            modelMappingService = new ModelMappingService(fakeMapper);
        }

        [Fact]
        public void MapModelsReturnsExceptionForNullAppRegistrationModel()
        {
            // Arrange
            AppRegistrationModel? appRegistrationModel = null;
            var legacyPathModel = ModelBuilders.ValidLegacyPathModel(ModelBuilders.PathName);
            var legacyRegionModels = ModelBuilders.ValidLegacyRegionModels();

            // Act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => modelMappingService.MapModels(appRegistrationModel, legacyPathModel, legacyRegionModels));

            // assert
            A.CallTo(() => fakeMapper.Map(legacyPathModel, appRegistrationModel)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<List<RegionModel>>(legacyRegionModels)).MustNotHaveHappened();

            Assert.Equal("Value cannot be null. (Parameter 'appRegistrationModel')", exceptionResult.Message);
        }

        [Fact]
        public void MapModelsReturnsExceptionForNullLegacyPathModel()
        {
            // Arrange
            var appRegistrationModel = ModelBuilders.ValidAppRegistrationModel(ModelBuilders.PathName);
            LegacyPathModel? legacyPathModel = null;
            var legacyRegionModels = ModelBuilders.ValidLegacyRegionModels();

            // Act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => modelMappingService.MapModels(appRegistrationModel, legacyPathModel, legacyRegionModels));

            // assert
            A.CallTo(() => fakeMapper.Map(legacyPathModel, appRegistrationModel)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<List<RegionModel>>(legacyRegionModels)).MustNotHaveHappened();

            Assert.Equal("Value cannot be null. (Parameter 'legacyPathModel')", exceptionResult.Message);
        }

        [Fact]
        public void MapModelsReturnsSuccessForValidDataModelsWithLegacyRegions()
        {
            // Arrange
            var appRegistrationModel = ModelBuilders.ValidAppRegistrationModel(ModelBuilders.PathName);
            var legacyPathModel = ModelBuilders.ValidLegacyPathModel(ModelBuilders.PathName);
            var legacyRegionModels = ModelBuilders.ValidLegacyRegionModels();
            var validRegionModels = ModelBuilders.ValidRegionModels();

            A.CallTo(() => fakeMapper.Map(legacyPathModel, appRegistrationModel));
            A.CallTo(() => fakeMapper.Map<List<RegionModel>>(legacyRegionModels)).Returns(validRegionModels);

            // Act
            modelMappingService.MapModels(appRegistrationModel, legacyPathModel, legacyRegionModels);

            // assert
            A.CallTo(() => fakeMapper.Map(legacyPathModel, appRegistrationModel)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<List<RegionModel>>(legacyRegionModels)).MustHaveHappenedOnceExactly();

            Assert.NotNull(appRegistrationModel.Regions);
        }

        [Fact]
        public void MapModelsReturnsSuccessForValidDataModelsWithoutLegacyRegions()
        {
            // Arrange
            var appRegistrationModel = ModelBuilders.ValidAppRegistrationModel(ModelBuilders.PathName);
            var legacyPathModel = ModelBuilders.ValidLegacyPathModel(ModelBuilders.PathName);
            IList<LegacyRegionModel>? legacyRegionModels = null;
            var validRegionModels = ModelBuilders.ValidRegionModels();

            A.CallTo(() => fakeMapper.Map(legacyPathModel, appRegistrationModel));
            A.CallTo(() => fakeMapper.Map<List<RegionModel>>(legacyRegionModels)).Returns(validRegionModels);

            // Act
            modelMappingService.MapModels(appRegistrationModel, legacyPathModel, legacyRegionModels);

            // assert
            A.CallTo(() => fakeMapper.Map(legacyPathModel, appRegistrationModel)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<List<RegionModel>>(legacyRegionModels)).MustNotHaveHappened();

            Assert.Null(appRegistrationModel.Regions);
        }
    }
}
