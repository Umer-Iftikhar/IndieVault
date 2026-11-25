using IndieVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IndieVault.Controllers
{
    [Authorize(Roles = "Player,Admin,GameDev")]
    public class DownloadController : Controller
    {
        private readonly IDownloadService _downloadService;

        public DownloadController(IDownloadService downloadService)
        {
            _downloadService = downloadService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var downloadLink = await _downloadService.DownloadGameAsync(id, currentUser!, cancellationToken);
                return Redirect(downloadLink);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                // No usable link: send the user back to the game with a message instead of a broken redirect
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Game", new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyDownloads(CancellationToken cancellationToken)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var gameList = await _downloadService.GetDownloadHistoryAsync(currentUserId, cancellationToken);
            return View(gameList);
        }
    }
}
