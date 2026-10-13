using Microsoft.AspNetCore.Mvc;

namespace IndieVault.Web.Controllers;

public class ErrorController : Controller
{
    [Route("/Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    // Re-executed for 4xx/5xx responses, so the original status code is kept
    [Route("/Error/{statusCode:int}")]
    public IActionResult StatusCodePage(int statusCode)
        => statusCode == StatusCodes.Status404NotFound ? View("Error404") : View("Error");
}
