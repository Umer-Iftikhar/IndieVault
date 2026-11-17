using IndieVault.Enums;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Mvc;
using IndieVault.Services.Interfaces;

namespace IndieVault.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGameBrowseService _gameBrowseService;

        public HomeController(IGameBrowseService gameBrowseService)
        {
           _gameBrowseService = gameBrowseService;
        }

        public async Task<IActionResult> Index(string? searchTerm, decimal? minPrice, decimal? maxPrice, int? genreId,List<int>? platformIds, SortBy sortBy = SortBy.Newest, int pageNumber = 1, int pageSize = 12)
        {
            var (games, totalCount) = await _gameBrowseService.GetBrowseGamesAsync(pageNumber, pageSize, searchTerm, minPrice, maxPrice, genreId, platformIds, sortBy);
            var genres = await _gameBrowseService.GetGenreListAsync();
            var platforms = await _gameBrowseService.GetPlatformListAsync();
            var featuredGames = await _gameBrowseService.GetFeaturedGamesAsync();
            var viewModel = new GameBrowseViewModel
            {
                Games = games,
                CurrentPage = pageNumber,
                TotalCount = totalCount,
                TotalPages = (totalCount + pageSize - 1) / pageSize,
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SelectedGenreId = genreId,
                SelectedPlatformsId = platformIds ?? new List<int>(),
                SortBy = sortBy,
                Genres = genres,
                Platforms = platforms,
                FeaturedGames = featuredGames.ToList(),
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

       
    }
}
