using IndieVault.Data;
using IndieVault.Models;

namespace IndieVault.Tests.TestHelpers
{
    // Seeds the minimum related rows a Game needs (developer, genre, engine).
    public class TestData
    {
        private readonly AppDbContext _context;

        public ApplicationUser Developer { get; }
        public ApplicationUser Player { get; }
        public Genre Genre { get; }
        public Engine Engine { get; }

        public TestData(AppDbContext context)
        {
            _context = context;
            Developer = new ApplicationUser { Id = "dev-1", UserName = "devone", Email = "dev@indievault.test" };
            Player = new ApplicationUser { Id = "player-1", UserName = "playerone", Email = "player@indievault.test" };
            Genre = new Genre { Name = "Action" };
            Engine = new Engine { Name = "Unity" };

            _context.Users.AddRange(Developer, Player);
            _context.Genres.Add(Genre);
            _context.Engines.Add(Engine);
            _context.SaveChanges();
        }

        public Game AddGame(string title = "Test Game", string? developerId = null, int? externalApiId = null, Genre? genre = null)
        {
            var game = new Game
            {
                Title = title,
                Description = "Description",
                Price = 9.99m,
                ReleaseDate = new DateTime(2025, 1, 1),
                CoverImagePath = "/images/games/cover.png",
                DownloadLink = "https://example.com/download",
                DeveloperId = developerId ?? Developer.Id,
                GenreId = (genre ?? Genre).Id,
                EngineId = Engine.Id,
                ExternalApiId = externalApiId
            };
            _context.Games.Add(game);
            _context.SaveChanges();
            return game;
        }
    }
}
