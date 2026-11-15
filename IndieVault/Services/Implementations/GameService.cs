using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IndieVault.Services.Implementations
{
    public class GameService : IGameService
    {
        private readonly IWebHostEnvironment _environment; // For handling file uploads and accessing web root path
        private readonly UserManager<ApplicationUser> _userManager; // For accessing user information and managing user-related operations
        private readonly IEngineRepository _engineRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IPlatformRepository _platformRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IScreenshotRepository _screenshotRepository; // Repository for managing screenshots
        private readonly IWishlistRepository _wishlistRepository; // Repository for managing wishlists
        private readonly IReviewRepository _reviewRepository; // Repository for managing reviews
        private readonly ILogger<GameService> _logger;

        public GameService
            (IWebHostEnvironment environment, UserManager<ApplicationUser> userManager, IEngineRepository engineRepository,
            IGenreRepository genreRepository, IPlatformRepository platformRepository, ITagRepository tagRepository,
            IGameRepository gameRepository, IScreenshotRepository screenshotRepository, IWishlistRepository wishlistRepository,
            IReviewRepository reviewRepository, ILogger<GameService> logger  )
        {
            _environment = environment;
            _userManager = userManager;
            _engineRepository = engineRepository;
            _genreRepository = genreRepository;
            _platformRepository = platformRepository;
            _tagRepository = tagRepository;
            _gameRepository = gameRepository;
            _screenshotRepository = screenshotRepository;   
            _wishlistRepository = wishlistRepository;
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        // This method retrieves the necessary data for populating the game upload/edit form, such as genres, engines, platforms, and tags.
        public async Task<GameFormDataDto> GetFormDataAsync()
        {
            // Retrieve all genres, engines, platforms, and tags from the database using the respective repositories  
            _logger.LogInformation("Retrieving game form data (genres, engines, platforms, tags)");

            var genreEntities = await _genreRepository.GetAllAsync();
            var engineEntities = await _engineRepository.GetAllAsync();
            var platformEntities = await _platformRepository.GetAllAsync();
            var tagEntities = await _tagRepository.GetAllAsync();

            // Map the retrieved entities to LookupDto objects, which contain only the Id and Name properties needed for dropdowns in the form
            var genres = genreEntities.Select(g => new LookupDto { Id = g.Id, Name = g.Name }).ToList();
            var engines = engineEntities.Select(e => new LookupDto { Id = e.Id, Name = e.Name }).ToList();
            var platforms = platformEntities.Select(p => new LookupDto { Id = p.Id, Name = p.Name }).ToList();
            var tags = tagEntities.Select(t => new LookupDto { Id = t.Id, Name = t.Name }).ToList();

            _logger.LogInformation("Form data retrieved: {GenreCount} genres, {EngineCount} engines, {PlatformCount} platforms, {TagCount} tags", genres.Count, engines.Count, platforms.Count, tags.Count);

            // Return a GameFormDataDto containing the lists of genres, engines, platforms, and tags to be used in the game upload/edit form
            return new GameFormDataDto
            {
                Genres = genres,
                Engines = engines,
                Platforms = platforms,
                Tags = tags
            };
        }
        public async Task UploadGameAsync(GameUploadDto uploadDto, string userId)
        {
            _logger.LogInformation("Uploading new game for developer {UserId}. Title: {Title}", userId, uploadDto.Title);

            // 1. Create a new Game entity and populate its properties with the data from the GameUploadDto
            var game = new Game
            {
                Title = uploadDto.Title,
                Description = uploadDto.Description,
                DownloadLink = uploadDto.DownloadLink,
                Price = uploadDto.Price,
                ReleaseDate = uploadDto.ReleaseDate,
                GenreId = uploadDto.SelectedGenreId,
                EngineId = uploadDto.SelectedEngineId,
                DeveloperId = userId,
                CreatedDate = DateTime.UtcNow,
                CoverImagePath = "", // This will be set after handling the file upload
                GamePlatforms = uploadDto.SelectedPlatforms?.Select(p => new GamePlatform { PlatformId = Convert.ToInt32(p) }).ToList() ?? new List<GamePlatform>(),
                GameTags = uploadDto.SelectedTags?.Select(t => new GameTag { TagId = Convert.ToInt32(t) }).ToList() ?? new List<GameTag>()
            };
            await _gameRepository.CreateAsync(game); // Save the game to the database to generate an ID for file storage
            _logger.LogInformation("Game created with ID {GameId}", game.Id);

            // Handle file uploads (cover image)

            // Creating id specific folder: wwwroot/images/games/{id}
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "games", game.Id.ToString());
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
                _logger.LogInformation("Created directory for game images: {Directory}", uploadsFolder);
            }

            if (uploadDto.CoverImage != null)
            {
                try
                {


                    // Save the cover image to the server
                    var coverExtension = Path.GetExtension(uploadDto.CoverImage.FileName); // Get the file extension to preserve it
                    var coverName = $"cover{coverExtension}";
                    var coverPath = Path.Combine(uploadsFolder, coverName);
                    using (var fileStream = new FileStream(coverPath, FileMode.Create)) // Save the file to the server
                    {
                        await uploadDto.CoverImage.CopyToAsync(fileStream);
                    }
                    // Set the cover image path relative to wwwroot for later retrieval
                    game.CoverImagePath = $"/images/games/{game.Id}/{coverName}";
                    _logger.LogInformation("Cover image saved for game {GameId}: {Path}", game.Id, game.CoverImagePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save cover image for game {GameId}", game.Id);
                    throw; // Re-throw because game creation would be incomplete
                }
            }

            // Handle file uploads (screenshots)
            if (uploadDto.Screenshots != null && uploadDto.Screenshots.Any())
            {
                var screenshotList = new List<Screenshot>();
                int count = 1; // Start from 1 for naming screenshots

                foreach (var file in uploadDto.Screenshots)
                {
                    try
                    {
                        var sExtension = Path.GetExtension(file.FileName);
                        var sName = $"screen_{count++}{sExtension}";
                        var sPath = Path.Combine(uploadsFolder, sName);

                        using (var stream = new FileStream(sPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        // Add the screenshot path to the database
                        screenshotList.Add(new Screenshot
                        {
                            ImagePath = $"/images/games/{game.Id}/{sName}",
                            GameId = game.Id
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save screenshot for game {GameId}", game.Id);
                        throw;
                    }
                }
                await _screenshotRepository.AddScreenshotsAsync(screenshotList);
                _logger.LogInformation("Added {Count} screenshots for game {GameId}", screenshotList.Count, game.Id);
            }
            // 6. Save changes again to update CoverPath and add Screenshots
            await _gameRepository.UpdateAsync(game);
            _logger.LogInformation("Game upload completed for game {GameId}", game.Id);

        }
        public async Task<List<MyGameDto>> GetMyGamesAsync(string userId)
        {
            _logger.LogInformation("Fetching games for developer {UserId}", userId);
            // Retrieve all games developed by the current user using the game repository
            var games = await _gameRepository.GetGamesByDevIdAsync(userId);
            _logger.LogInformation("Found {Count} games for developer {UserId}", games.Count, userId);

            // Map the retrieved Game entities to MyGameDto objects, which contain only the necessary properties for displaying in the "My Games" section
            return games.Select(g => new MyGameDto
            {
                Id = g.Id,
                Title = g.Title,
                Price = g.Price,
                ReleaseDate = g.ReleaseDate,
                CoverImagePath = g.CoverImagePath,
                GenreName = g.Genre.Name,
                Engine = g.Engine.Name
            }).ToList();
        }
    }
}
