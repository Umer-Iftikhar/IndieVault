using IndieVault.Enums;
using IndieVault.Models;
using IndieVault.Tests.TestHelpers;

namespace IndieVault.Tests.Models
{
    public class ModelDefaultsTests
    {
        [Fact]
        public void ApplicationUser_GithubUserNameOver100Chars_FailsValidation()
        {
            var user = new ApplicationUser { GithubUserName = new string('g', 101) };

            var results = ModelValidator.Validate(user);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ApplicationUser.GithubUserName)));
        }

        [Fact]
        public void ApplicationUser_GithubUserNameAt100Chars_PassesValidation()
        {
            var user = new ApplicationUser { GithubUserName = new string('g', 100) };

            var results = ModelValidator.Validate(user);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("0HN7:00000001", true)]
        public void ErrorViewModel_ShowRequestId_DependsOnRequestId(string? requestId, bool expected)
        {
            var model = new ErrorViewModel { RequestId = requestId };

            Assert.Equal(expected, model.ShowRequestId);
        }

        [Fact]
        public void SortBy_DefaultValue_IsNewest()
        {
            Assert.Equal(SortBy.Newest, default(SortBy));
        }

        [Fact]
        public void Wishlist_And_DownloadHistory_DefaultDatesAreSetOnCreation()
        {
            var before = DateTime.UtcNow.AddSeconds(-1);

            var wishlist = new Wishlist();
            var download = new DownloadHistory();

            Assert.InRange(wishlist.CreatedDate, before, DateTime.UtcNow.AddSeconds(1));
            Assert.InRange(download.DownloadDate, before, DateTime.UtcNow.AddSeconds(1));
        }
    }
}
