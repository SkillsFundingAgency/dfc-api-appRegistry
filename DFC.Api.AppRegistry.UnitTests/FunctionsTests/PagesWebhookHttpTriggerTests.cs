using DFC.Api.AppRegistry.Contracts;
using DFC.Api.AppRegistry.Functions;
using DFC.Compui.Subscriptions.Pkg.Data.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.AppRegistry.UnitTests.ServicesTests
{
    [Trait("Category", "PagesWebhookHttpTrigger - Http trigger tests")]
    public class PagesWebhookHttpTriggerTests
    {
        private readonly ILogger<PagesWebhookHttpTrigger> fakeLogger = A.Fake<ILogger<PagesWebhookHttpTrigger>>();
        private readonly IWebhookReceiver fakewebhookReceiver = A.Fake<IWebhookReceiver>();

        [Fact]
        public async Task PostWithBodyReturnsOk()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.OK);
            var function = new PagesWebhookHttpTrigger(fakeLogger, fakewebhookReceiver);
            var request = BuildRequestWithValidBody("A webhook test");

            A.CallTo(() => fakewebhookReceiver.ReceiveEvents(A<string>.Ignored)).Returns(new StatusCodeResult(200));

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakewebhookReceiver.ReceiveEvents(A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostNullRequestReturnsBadRequest()
        {
            // Arrange
            HttpRequest? request = null;
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.BadRequest);
            var function = new PagesWebhookHttpTrigger(fakeLogger, fakewebhookReceiver);

            A.CallTo(() => fakewebhookReceiver.ReceiveEvents(A<string>.Ignored)).Returns(new StatusCodeResult(200));

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostNullRequestBodyReturnsBadRequest()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.BadRequest);
            var function = new PagesWebhookHttpTrigger(fakeLogger, fakewebhookReceiver);
            var request = new DefaultHttpRequest(new DefaultHttpContext());

            A.CallTo(() => fakewebhookReceiver.ReceiveEvents(A<string>.Ignored)).Returns(new StatusCodeResult(200));

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostCatchesException()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            var function = new PagesWebhookHttpTrigger(fakeLogger, fakewebhookReceiver);
            var request = BuildRequestWithValidBody("A webhook test");

            A.CallTo(() => fakewebhookReceiver.ReceiveEvents(A<string>.Ignored)).Throws(new NotImplementedException());

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        private static HttpRequest BuildRequestWithValidBody(string bodyString)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyString)),
            };
        }
    }
}
