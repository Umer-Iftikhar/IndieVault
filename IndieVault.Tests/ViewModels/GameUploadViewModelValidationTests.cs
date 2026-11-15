using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;

namespace IndieVault.Tests.ViewModels
{
    public class GameUploadViewModelValidationTests
    {
        private static GameUploadViewModel Valid() => new()
        {
            Title = "Celeste",
            Description = "Climb the mountain.",
            Price = 19.99m,
            SelectedGenreId = 1,
            SelectedEngineId = 1,
            DownloadLink = "https://example.com/celeste",
            ReleaseDate = new DateTime(2018, 1, 25),
            CoverImage = FakeFormFile.Create("cover.png")
        };

        [Fact]
        public void ValidModel_PassesValidation()
        {
            Assert.Empty(ModelValidator.Validate(Valid()));
        }

        [Theory]
        [InlineData(nameof(GameUploadViewModel.Title))]
        [InlineData(nameof(GameUploadViewModel.Description))]
        [InlineData(nameof(GameUploadViewModel.DownloadLink))]
        public void MissingRequiredText_Fails(string property)
        {
            var model = Valid();
            typeof(GameUploadViewModel).GetProperty(property)!.SetValue(model, string.Empty);

            Assert.Contains(ModelValidator.Validate(model), r => r.MemberNames.Contains(property));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void PriceNotGreaterThanZero_Fails(decimal price)
        {
            var model = Valid();
            model.Price = price;

            var result = Assert.Single(ModelValidator.Validate(model), r => r.MemberNames.Contains(nameof(GameUploadViewModel.Price)));
            Assert.Equal("Price must be greater than 0.", result.ErrorMessage);
        }

        [Fact]
        public void NoGenreOrEngineSelected_FailsWithFriendlyMessages()
        {
            var model = Valid();
            model.SelectedGenreId = 0;
            model.SelectedEngineId = 0;

            var results = ModelValidator.Validate(model);

            Assert.Contains(results, r => r.ErrorMessage == "Please select a genre.");
            Assert.Contains(results, r => r.ErrorMessage == "Please select an engine.");
        }
    }
}
