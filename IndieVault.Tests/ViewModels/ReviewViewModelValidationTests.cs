using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;

namespace IndieVault.Tests.ViewModels
{
    public class ReviewViewModelValidationTests
    {
        [Fact]
        public void ValidReview_PassesValidation()
        {
            Assert.Empty(ModelValidator.Validate(new ReviewViewModel { GameId = 1, Rating = 4, Comment = "Great game!" }));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void RatingOutOfRange_Fails(int rating)
        {
            var results = ModelValidator.Validate(new ReviewViewModel { GameId = 1, Rating = rating });

            Assert.Contains(results, r => r.ErrorMessage == "Rating must be between 1 and 5.");
        }

        [Fact]
        public void CommentOver1000Chars_Fails()
        {
            // Ported from the original ReviewValidationTests: the rule now lives on the view model
            var results = ModelValidator.Validate(new ReviewViewModel { GameId = 1, Rating = 5, Comment = new string('C', 1001) });

            Assert.Contains(results, r => r.ErrorMessage == "Comment cannot exceed 1000 characters.");
        }

        [Fact]
        public void CommentExactly1000Chars_Passes()
        {
            Assert.Empty(ModelValidator.Validate(new ReviewViewModel { GameId = 1, Rating = 5, Comment = new string('C', 1000) }));
        }
    }
}
