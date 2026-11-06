using IndieVault.Models;
using System.ComponentModel.DataAnnotations;

namespace IndieVault.Tests
{
    public class ReviewValidationTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void Review_ValidReview_PassesValidation()
        {
            var review = new Review
            {
                Rating = 4,
                Comment = "Great game!",
                GameId = 1,
                UserId = "user-123"
            };

            var results = ValidateModel(review);

            Assert.Empty(results);
        }

        [Fact]
        public void Review_WithoutComment_PassesValidation()
        {
            var review = new Review { Rating = 5, GameId = 1, UserId = "user-123", Comment = null };

            var results = ValidateModel(review);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public void Review_InvalidRatingRange_FailsValidation(int invalidRating)
        {
            var review = new Review
            {
                Rating = invalidRating,
                GameId = 1,
                UserId = "user-123"
            };

            var results = ValidateModel(review);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Review.Rating)));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void Review_BoundaryRatings_PassValidation(int rating)
        {
            var review = new Review { Rating = rating, GameId = 1, UserId = "user-123" };

            var results = ValidateModel(review);

            Assert.Empty(results);
        }
    }
}
