using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;

namespace IndieVault.Tests.ViewModels
{
    public class GameEditViewModelValidationTests
    {
        private static GameEditViewModel Valid() => new()
        {
            Id = 1,
            Title = "Celeste",
            Description = "Climb the mountain.",
            Price = 19.99m,
            SelectedGenreId = 1,
            SelectedEngineId = 1,
            DownloadLink = "https://example.com/celeste",
            ReleaseDate = new DateTime(2018, 1, 25)
        };

        [Fact]
        public void ValidModel_WithoutNewCover_PassesValidation()
        {
            // Cover and screenshots are optional when editing
            Assert.Empty(ModelValidator.Validate(Valid()));
        }

        [Fact]
        public void ZeroPrice_Fails()
        {
            var model = Valid();
            model.Price = 0;

            Assert.Contains(ModelValidator.Validate(model), r => r.MemberNames.Contains(nameof(GameEditViewModel.Price)));
        }

        [Fact]
        public void MissingTitle_Fails()
        {
            var model = Valid();
            model.Title = "";

            Assert.Contains(ModelValidator.Validate(model), r => r.MemberNames.Contains(nameof(GameEditViewModel.Title)));
        }
    }
}
