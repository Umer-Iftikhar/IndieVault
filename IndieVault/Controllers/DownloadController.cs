using IndieVault.Data;
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
        public async Task<IActionResult> Download(int id)
        {
            var currentUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var downloadLink = await _downloadService.DownloadGameAsync(id, currentUser!);
            return Redirect(downloadLink);
        }

        [HttpGet]
        public async Task<IActionResult> MyDownloads()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(); 
            }
            var gameList = await _downloadService.GetDownloadHistoryAsync(currentUserId);
            return View(gameList);
        }
    }
}
