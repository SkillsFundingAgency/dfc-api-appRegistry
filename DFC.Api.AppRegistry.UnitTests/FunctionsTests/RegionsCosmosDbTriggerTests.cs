using Castle.Core.Logging;
using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Functions;
using DFC.Api.AppRegistry.Models.Legacy;
using DFC.Api.AppRegistry.Services;
using FakeItEasy;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.FunctionsTests
{
    public class Foo : Document
    {
        public string Bar { get; set; }
    }

    public class RegionsCosmosDbTriggerTests
    {
        private readonly ILogger<RegionsCosmosDbTrigger> logger;
        private readonly ILegacyDataLoadService legacyDataLoadService;

        public RegionsCosmosDbTriggerTests()
        {
            logger = A.Fake<ILogger<RegionsCosmosDbTrigger>>();
            legacyDataLoadService = A.Fake<ILegacyDataLoadService>();
        }


        [Fact]
        public void DoSomething()
        {
            // Arrange
            var function = new RegionsCosmosDbTrigger(logger, legacyDataLoadService);

            // Act
            var result = function.Run(new List<Document> { new LegacyRegionModel { Path = "A/Path", Id = Guid.NewGuid().ToString() } });

            // Assert
            Assert.NotNull(result);
        }
    }
}
