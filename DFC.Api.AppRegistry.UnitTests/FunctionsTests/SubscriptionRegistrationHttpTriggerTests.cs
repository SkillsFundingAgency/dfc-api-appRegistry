using DFC.Api.AppRegistry.Functions;
using DFC.Compui.Subscriptions.Pkg.NetStandard.Data.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.FunctionsTests
{
    [Trait("Category", "SubscriptionRegistration - Http trigger tests")]
    public class SubscriptionRegistrationHttpTriggerTests
    {
        private readonly ILogger<SubscriptionRegistrationHttpTrigger> logger;
        private readonly ISubscriptionRegistrationService subscriptionRegistrationService;
        public SubscriptionRegistrationHttpTriggerTests()
        {
            logger = A.Fake<ILogger<SubscriptionRegistrationHttpTrigger>>();
            subscriptionRegistrationService = A.Fake<ISubscriptionRegistrationService>();
        }

        [Fact]
        public async Task SubscriptionRegistrationPostReturnsOk()
        {
            // Arrange
            var function = new SubscriptionRegistrationHttpTrigger(logger, subscriptionRegistrationService);

            // Act
            var result = await function.Run(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => subscriptionRegistrationService.RegisterSubscription(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SubscriptionRegistrationPostThrowsException()
        {
            // Arrange
            A.CallTo(() => subscriptionRegistrationService.RegisterSubscription(A<string>.Ignored)).ThrowsAsync(new HttpRequestException());
            var function = new SubscriptionRegistrationHttpTrigger(logger, subscriptionRegistrationService);

            // Act
            var result = await function.Run(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => subscriptionRegistrationService.RegisterSubscription(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.IsType<InternalServerErrorResult>(result);
        }
    }
}
