using IndieVault.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IndieVault.Tests.Controllers
{
    public class ErrorControllerRateLimitTests
    {
        private readonly ErrorController _controller = new();

        [Fact]
        public void StatusCodePage_429_ReturnsTooManyRequestsView()
        {
            var result = _controller.StatusCodePage(StatusCodes.Status429TooManyRequests);

            Assert.Equal("Error429", Assert.IsType<ViewResult>(result).ViewName);
        }

        [Theory]
        [InlineData(StatusCodes.Status404NotFound, "Error404")]
        [InlineData(StatusCodes.Status403Forbidden, "Error")]
        [InlineData(StatusCodes.Status500InternalServerError, "Error")]
        public void StatusCodePage_OtherCodes_KeepTheirExistingViews(int statusCode, string expectedView)
        {
            var result = _controller.StatusCodePage(statusCode);

            Assert.Equal(expectedView, Assert.IsType<ViewResult>(result).ViewName);
        }
    }
}
