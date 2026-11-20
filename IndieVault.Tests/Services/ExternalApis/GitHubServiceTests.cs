using IndieVault.Services.Implementations.ExternalApis;
using IndieVault.Tests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace IndieVault.Tests.Services.ExternalApis
{
    public class GitHubServiceTests
    {
        private static GitHubService Service(StubHttpMessageHandler handler) =>
            new(new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") }, NullLogger<GitHubService>.Instance);

        private const string UserJson = """{ "html_url": "https://github.com/umer", "public_repos": 6 }""";

        private const string ReposJson = """
            [
              { "stargazers_count": 10, "language": "C#" },
              { "stargazers_count": 4,  "language": "C#" },
              { "stargazers_count": 2,  "language": "TypeScript" },
              { "stargazers_count": 1,  "language": "Python" },
              { "stargazers_count": 0,  "language": "Python" },
              { "stargazers_count": 3,  "language": "Go" },
              { "stargazers_count": 5,  "language": null }
            ]
            """;

        [Fact]
        public async Task GetProfileAsync_ReturnsProfileWithTotalsAndTopThreeLanguages()
        {
            var handler = new StubHttpMessageHandler()
                .Respond("/users/umer/repos", ReposJson)
                .Respond("/users/umer", UserJson);

            var profile = await Service(handler).GetProfileAsync("umer");

            Assert.NotNull(profile);
            Assert.Equal("https://github.com/umer", profile!.ProfileUrl);
            Assert.Equal(6, profile.PublicRepos);
            Assert.Equal(25, profile.TotalStars);
            Assert.Equal(3, profile.TopLanguages.Count);
            Assert.Equal("C#", profile.TopLanguages[0]);
            Assert.Contains("Python", profile.TopLanguages);
            Assert.DoesNotContain(profile.TopLanguages, l => l == null);
        }

        [Fact]
        public async Task GetProfileAsync_CallsUserAndReposEndpoints()
        {
            var handler = new StubHttpMessageHandler()
                .Respond("/users/umer/repos", "[]")
                .Respond("/users/umer", UserJson);

            await Service(handler).GetProfileAsync("umer");

            Assert.Equal(new[] { "/users/umer", "/users/umer/repos" }, handler.Requests.Select(r => r.RequestUri!.AbsolutePath));
        }

        [Fact]
        public async Task GetProfileAsync_UserNotFound_ReturnsNull()
        {
            var handler = new StubHttpMessageHandler(); // everything 404s

            Assert.Null(await Service(handler).GetProfileAsync("ghost"));
        }

        [Fact]
        public async Task GetProfileAsync_NullUserBody_ReturnsNull()
        {
            var handler = new StubHttpMessageHandler().Respond("/users/umer", "null");

            Assert.Null(await Service(handler).GetProfileAsync("umer"));
        }

        [Fact]
        public async Task GetProfileAsync_NoRepos_ReturnsZeroStarsAndNoLanguages()
        {
            var handler = new StubHttpMessageHandler()
                .Respond("/users/umer/repos", "[]")
                .Respond("/users/umer", UserJson);

            var profile = await Service(handler).GetProfileAsync("umer");

            Assert.Equal(0, profile!.TotalStars);
            Assert.Empty(profile.TopLanguages);
        }
    }
}
