using IndieVault.Models;
using System.ComponentModel.DataAnnotations;

namespace IndieVault.Tests
{
    public class GameValidationTests
    {
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Game_ValidData_PassesValidation()
        {
            var game = new Game
            {
                Title = "Hollow Knight",
                Description = "A masterpiece.",
                Price = 14.99m,
                ReleaseDate = DateTime.Now,
                EngineId = 1,
                GenreId = 1,
                DeveloperId = "dev-123",
                CoverImagePath = "/images/hk.jpg",
                DownloadLink = "https://indievault.com/download/hk"
            };

            var results = ValidateModel(game);

            Assert.Empty(results);
        }

        [Fact]
        public void Game_FreeGame_PassesValidation()
        {
            // Imported RAWG games are stored with Price = 0
            var game = new Game { Title = "Free Game", Description = "Free to play.", Price = 0m };

            var results = ValidateModel(game);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-5.00)]
        public void Game_NegativePrice_FailsValidation(decimal invalidPrice)
        {
            var game = new Game
            {
                Title = "Valid Title",
                Description = "Valid Description",
                Price = invalidPrice,
                CoverImagePath = "path.jpg",
                DownloadLink = "link.com"
            };

            var results = ValidateModel(game);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Game.Price)));
        }

        [Fact]
        public void Game_NewInstance_HasExpectedDefaults()
        {
            var game = new Game();

            Assert.False(game.IsFeatured);
            Assert.False(game.IsFromExternalApi);
            Assert.Null(game.ExternalApiId);
            Assert.Empty(game.GamePlatforms);
            Assert.Empty(game.GameTags);
            Assert.Empty(game.Reviews);
            Assert.Empty(game.Screenshots);
        }
    }
}
