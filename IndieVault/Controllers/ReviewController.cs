using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;



namespace IndieVault.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        [Authorize(Roles ="Player")]
        public async Task<IActionResult> Create(int id)
        {
            await _reviewService.GetReviewFormAsync(id);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Getting the user ID from claims

            bool alreadyReviewed = await _reviewService.HasUserReviewedAsync(id, currentUserId!); // Check if the user has already reviewed this game
            if (alreadyReviewed)
            {
                TempData["ErrorMessage"] = "You have already reviewed this game.";
                return RedirectToAction("Details", "Game", new { id });
            }

            var viewModel = new ReviewViewModel // We only need the GameId to link the review to the correct game
            {
                GameId = id
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public async Task<IActionResult> Create(ReviewViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Getting the user ID from claims
            bool alreadyReviewed = await _reviewService.HasUserReviewedAsync(viewModel.GameId, currentUserId!);

            if (alreadyReviewed) // Double-checking on POST to prevent any bypass of the GET check (e.g., via direct POST request)
            {
                TempData["ErrorMessage"] = "You have already reviewed this game.";
                return RedirectToAction("Details", "Game", new { id = viewModel.GameId });
            }
            var createReviewDto = new CreateReviewDto // Mapping the ViewModel to a DTO for service layer
            {
                GameId = viewModel.GameId,
                Rating = viewModel.Rating,
                Comment = viewModel.Comment
            };

            await _reviewService.CreateReviewAsync(createReviewDto, currentUserId!); 

            return RedirectToAction("Details", "Game", new { id = viewModel.GameId }); // Redirecting back to the Game Details page after successful review creation
        }

        [HttpPost]
        [Authorize] // Ensures only logged-in users can even hit this logic
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Delete(int id, int gameId)
        {
            // Fetch the review
            if (!ModelState.IsValid)  // Basic validation check, though we only have the review ID here
            {
                return BadRequest();
            }

            // Authorization Check
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            // Attempt to delete the review
            await _reviewService.DeleteReviewAsync(id, currentUserId!, isAdmin);

            // Let the user know it worked
            TempData["Message"] = "Review removed successfully.";


            // Redirect back to the Game Details page
            return RedirectToAction("Details", "Game", new { id = gameId });
        }

    }
}
