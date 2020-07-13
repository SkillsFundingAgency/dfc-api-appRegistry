using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Functions;
using DFC.Api.AppRegistry.Models.Legacy;
using DFC.Api.AppRegistry.UnitTests.TestModels;
using FakeItEasy;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.FunctionsTests
{
    [Trait("Category", "PathsCosmosDbTrigger - tests")]
    public class PathsCosmosDbTriggerTests
    {
        private readonly ILogger<PathsCosmosDbTrigger> logger;
        private readonly ILegacyDataLoadService legacyDataLoadService;

        public PathsCosmosDbTriggerTests()
        {
            logger = A.Fake<ILogger<PathsCosmosDbTrigger>>();
            legacyDataLoadService = A.Fake<ILegacyDataLoadService>();
        }

        [Fact]
        public async Task PathsCosmosDbTriggerWhenExecutedUpdatesPath()
        {
            // Arrange
            var function = new PathsCosmosDbTrigger(logger, legacyDataLoadService);

            // Act
            await function.Run(new List<Document> { ModelBuilders.ValidLegacyPathModel(ModelBuilders.PathName) }).ConfigureAwait(false);

            // Assert
            A.CallTo(() => legacyDataLoadService.UpdatePathAsync(A<LegacyPathModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PathsCosmosDbTriggerWhenExecutedThrowsException()
        {
            A.CallTo(() => legacyDataLoadService.UpdatePathAsync(A<LegacyPathModel>.Ignored)).ThrowsAsync(new HttpRequestException());

            // Arrange
            var function = new PathsCosmosDbTrigger(logger, legacyDataLoadService);

            // Act
            // Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => function.Run(new List<Document> { ModelBuilders.ValidLegacyPathModel(ModelBuilders.PathName) })).ConfigureAwait(false);
        }
    }
}
