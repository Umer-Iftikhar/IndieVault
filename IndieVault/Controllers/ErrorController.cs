using Microsoft.AspNetCore.Mvc;

namespace IndieVault.Controllers;

public class ErrorController : Controller
{
    [Route("/Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    // Re-executed for 4xx/5xx responses, so the original status code is kept
    [Route("/Error/{statusCode:int}")]
    public IActionResult StatusCodePage(int statusCode)
    {
        switch (statusCode)
        {
            case StatusCodes.Status404NotFound:
                return View("Error404");

            case StatusCodes.Status429TooManyRequests: // rate limiter rejections
                // The rate limiter sets Retry-After in seconds; the page shows it in minutes
                if (int.TryParse(HttpContext?.Response.Headers.RetryAfter.ToString(), out var seconds) && seconds > 0)
                {
                    ViewData["RetryAfterMinutes"] = (int)Math.Ceiling(seconds / 60.0);
                }
                return View("Error429");

            default:
                return View("Error");
        }
    }
}
