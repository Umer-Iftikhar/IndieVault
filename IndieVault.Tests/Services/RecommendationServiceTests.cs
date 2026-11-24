using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Implementations;
using IndieVault.Services.Interfaces.ExternalApis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndieVault.Tests.Services
{
    public class RecommendationServiceTests : IDisposable
    {
        private const string UserId = "player-1";

        private readonly Mock<IRecommendationRepository> _repository = new();
        private readonly Mock<IGeminiService> _gemini = new();
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        private readonly RecommendationService _service;

        public RecommendationServiceTests()
        {
            _service = new RecommendationService(_repository.Object, _gemini.Object, _cache,
                NullLogger<RecommendationService>.Instance);

            // Defaults: a new player, no candidates, AI unavailable. Tests override what they need.
            SetupLikedGames();
            SetupKnownGameIds();
            SetupCandidates();
            SetupAiResponse(null);
        }

        public void Dispose() => _cache.Dispose();

        // ---- helpers ----

        private void SetupLikedGames(params Game[] games)
        {
            _repository.Setup(r => r.GetGamesUserLikedAsync(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(games.ToList());
        }

        private void SetupKnownGameIds(params int[] ids)
        {
            _repository.Setup(r => r.GetKnownGameIdsAsync(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ids.ToList());
        }

        private void SetupCandidates(params RecommendationCandidateDto[] candidates)
        {
            _repository.Setup(r => r.GetCandidateGamesAsync(It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(candidates.ToList());
        }

        private void SetupAiResponse(string? json)
        {
            _gemini.Setup(g => g.GenerateJsonAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(json);
        }

        private static Game LikedGame(string genre, params string[] tags) => new()
        {
            Title = "Liked game",
            Genre = new Genre { Name = genre },
            GameTags = tags.Select(t => new GameTag { Tag = new Tag { Name = t } }).ToList()
        };

        private static RecommendationCandidateDto Candidate(int id, string genre = "Puzzle", double rating = 0, params string[] tags) => new()
        {
            Id = id,
            Title = $"Game {id:00}",
            Description = $"Description of game {id}",
            Price = 9.99m,
            CoverImagePath = $"/images/games/{id}/cover.png",
            Developer = "devone",
            GenreName = genre,
            AverageRating = rating,
            Tags = tags.ToList()
        };

        private static string AiJson(params (int Id, string Reason)[] picks)
        {
            var items = picks.Select(p => $"{{\"gameId\":{p.Id},\"reason\":\"{p.Reason}\"}}");
            return $"{{\"recommendations\":[{string.Join(",", items)}]}}";
        }

        // ---- AI path ----

        [Fact]
        public async Task GetRecommendationsAsync_AiPicks_AreUsedInOrderWithReasons()
        {
            SetupCandidates(Candidate(1), Candidate(2), Candidate(3));
            SetupAiResponse(AiJson((3, "Third is great"), (1, "First is fun")));

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.True(result.GeneratedByAi);
            Assert.Equal(3, result.Games[0].Game.Id);
            Assert.Equal("Third is great", result.Games[0].Reason);
            Assert.Equal(1, result.Games[1].Game.Id);
            Assert.Equal("First is fun", result.Games[1].Reason);
        }

        [Fact]
        public async Task GetRecommendationsAsync_AiInventsGameIds_TheyAreDropped()
        {
            SetupCandidates(Candidate(1), Candidate(2));
            SetupAiResponse(AiJson((999, "Does not exist"), (2, "Real game")));

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.DoesNotContain(result.Games, g => g.Game.Id == 999);
            Assert.Equal(2, result.Games[0].Game.Id);
            Assert.True(result.GeneratedByAi);
        }

        [Fact]
        public async Task GetRecommendationsAsync_AiRepeatsAGame_ItAppearsOnce()
        {
            SetupCandidates(Candidate(1), Candidate(2));
            SetupAiResponse(AiJson((1, "Once"), (1, "Twice")));

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.Single(result.Games, g => g.Game.Id == 1);
        }

        [Fact]
        public async Task GetRecommendationsAsync_AiReturnsTooFew_RestIsFilledFromRanking()
        {
            SetupCandidates(Enumerable.Range(1, 10).Select(i => Candidate(i)).ToArray());
            SetupAiResponse(AiJson((7, "AI pick")));

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.Equal(RecommendationService.RecommendationCount, result.Games.Count);
            Assert.Equal(7, result.Games[0].Game.Id);
            Assert.Equal(result.Games.Count, result.Games.Select(g => g.Game.Id).Distinct().Count());
        }

        [Fact]
        public async Task GetRecommendationsAsync_AiReturnsTooMany_OnlySixAreKept()
        {
            SetupCandidates(Enumerable.Range(1, 10).Select(i => Candidate(i)).ToArray());
            SetupAiResponse(AiJson(Enumerable.Range(1, 10).Select(i => (i, "Pick")).ToArray()));

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.Equal(RecommendationService.RecommendationCount, result.Games.Count);
        }

        [Fact]
        public async Task GetRecommendationsAsync_AiReasonIsVeryLong_ItIsTruncated()
        {
            SetupCandidates(Candidate(1));
            SetupAiResponse(AiJson((1, new string('x', 500))));

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.Equal(RecommendationService.MaxReasonLength, result.Games[0].Reason.Length);
        }

        [Fact]
        public async Task GetRecommendationsAsync_AiReasonIsBlank_UsesRuleBasedReason()
        {
            SetupLikedGames(LikedGame("Puzzle"));
            SetupCandidates(Candidate(1, genre: "Puzzle"));
            SetupAiResponse(AiJson((1, " ")));

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.Equal("Because you like Puzzle games.", result.Games[0].Reason);
        }

        [Fact]
        public async Task GetRecommendationsAsync_AiJsonWrappedInCodeFence_IsStillParsed()
        {
            SetupCandidates(Candidate(1));
            SetupAiResponse("```json\n" + AiJson((1, "Fenced")) + "\n```");

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.True(result.GeneratedByAi);
            Assert.Equal("Fenced", result.Games[0].Reason);
        }

        // ---- fallback path ----

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("this is not json")]
        [InlineData("{\"recommendations\":[]}")]
        public async Task GetRecommendationsAsync_AiUnavailableOrUseless_FallsBackToRuleBasedRanking(string? aiResponse)
        {
            SetupCandidates(Candidate(1), Candidate(2));
            SetupAiResponse(aiResponse);

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.False(result.GeneratedByAi);
            Assert.Equal(2, result.Games.Count);
            Assert.All(result.Games, g => Assert.False(string.IsNullOrWhiteSpace(g.Reason)));
        }

        [Fact]
        public async Task GetRecommendationsAsync_Fallback_RanksGenreMatchThenTagMatchThenRating()
        {
            SetupLikedGames(LikedGame("RPG", "Pixel Art"), LikedGame("RPG"));
            SetupCandidates(
                Candidate(1, genre: "Sports", rating: 5),
                Candidate(2, genre: "Racing", rating: 1, tags: "Pixel Art"),
                Candidate(3, genre: "RPG", rating: 0));

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.Equal(new[] { 3, 2, 1 }, result.Games.Select(g => g.Game.Id));
            Assert.Equal("Because you like RPG games.", result.Games[0].Reason);
            Assert.Equal("Matches your interest in Pixel Art games.", result.Games[1].Reason);
            Assert.Equal("Highly rated by other players.", result.Games[2].Reason);
        }

        [Fact]
        public async Task GetRecommendationsAsync_NewPlayerWithoutHistory_GetsHighestRatedGamesFirst()
        {
            SetupCandidates(Candidate(1, rating: 2), Candidate(2, rating: 5), Candidate(3, rating: 3.5));

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.Equal(new[] { 2, 3, 1 }, result.Games.Select(g => g.Game.Id));
        }

        [Fact]
        public async Task GetRecommendationsAsync_NoCandidates_ReturnsEmptyWithoutCallingAi()
        {
            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.Empty(result.Games);
            Assert.False(result.GeneratedByAi);
            _gemini.Verify(g => g.GenerateJsonAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ---- what is sent to the database and the AI ----

        [Fact]
        public async Task GetRecommendationsAsync_ExcludesGamesThePlayerAlreadyKnows()
        {
            SetupKnownGameIds(4, 8);
            SetupCandidates(Candidate(1));

            await _service.GetRecommendationsAsync(UserId);

            _repository.Verify(r => r.GetCandidateGamesAsync(
                It.Is<List<int>>(ids => ids.SequenceEqual(new[] { 4, 8 })),
                RecommendationService.CandidatePoolSize,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRecommendationsAsync_SendsOnlyTheTopTwentyCandidatesToAi()
        {
            // Ratings rise with the id, so games 6..25 make the shortlist and 1..5 do not
            SetupCandidates(Enumerable.Range(1, 25).Select(i => Candidate(i, rating: i / 10.0)).ToArray());
            string? prompt = null;
            _gemini.Setup(g => g.GenerateJsonAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Callback((string p, object _, CancellationToken _) => prompt = p)
                .ReturnsAsync((string?)null);

            await _service.GetRecommendationsAsync(UserId);

            Assert.NotNull(prompt);
            Assert.Contains("Game 25", prompt);
            Assert.Contains("Game 06", prompt);
            Assert.DoesNotContain("Game 05", prompt);
            Assert.DoesNotContain("Game 01", prompt);
        }

        [Fact]
        public async Task GetRecommendationsAsync_AiPickOutsideShortlist_IsIgnored()
        {
            SetupCandidates(Enumerable.Range(1, 25).Select(i => Candidate(i, rating: i / 10.0)).ToArray());
            SetupAiResponse(AiJson((1, "Was never offered to the AI")));

            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.False(result.GeneratedByAi);
            Assert.DoesNotContain(result.Games, g => g.Game.Id == 1);
        }

        [Fact]
        public void BuildPrompt_ContainsTasteAndCandidatesButTrimsLongDescriptions()
        {
            var candidate = Candidate(1, genre: "RPG", rating: 4.26, tags: "Pixel Art");
            candidate.Description = new string('d', 500);

            var prompt = RecommendationService.BuildPrompt(
                new List<string> { "RPG" }, new List<string> { "Pixel Art" }, new List<RecommendationCandidateDto> { candidate });

            Assert.Contains("\"favouriteGenres\":[\"RPG\"]", prompt);
            Assert.Contains("\"favouriteTags\":[\"Pixel Art\"]", prompt);
            Assert.Contains("\"id\":1", prompt);
            Assert.Contains("\"rating\":4.3", prompt);
            Assert.Contains(new string('d', 160), prompt);
            Assert.DoesNotContain(new string('d', 161), prompt);
        }

        [Fact]
        public async Task GetRecommendationsAsync_MapsCandidateToGameCardDto()
        {
            SetupCandidates(Candidate(5, genre: "RPG", rating: 4.5));

            var result = await _service.GetRecommendationsAsync(UserId);

            var game = Assert.Single(result.Games).Game;
            Assert.Equal(5, game.Id);
            Assert.Equal("Game 05", game.Title);
            Assert.Equal("RPG", game.GenreName);
            Assert.Equal("devone", game.Developer);
            Assert.Equal(9.99m, game.Price);
            Assert.Equal(4.5, game.AverageRatings);
            Assert.Equal("/images/games/5/cover.png", game.CoverImagePath);
        }

        // ---- caching ----

        [Fact]
        public async Task GetRecommendationsAsync_SecondCall_IsServedFromCache()
        {
            SetupCandidates(Candidate(1));
            SetupAiResponse(AiJson((1, "Cached")));

            var first = await _service.GetRecommendationsAsync(UserId);
            var second = await _service.GetRecommendationsAsync(UserId);

            Assert.Same(first, second);
            _gemini.Verify(g => g.GenerateJsonAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
            _repository.Verify(r => r.GetGamesUserLikedAsync(UserId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRecommendationsAsync_CacheIsPerUser()
        {
            SetupCandidates(Candidate(1));
            _repository.Setup(r => r.GetGamesUserLikedAsync("player-2", It.IsAny<CancellationToken>())).ReturnsAsync(new List<Game>());
            _repository.Setup(r => r.GetKnownGameIdsAsync("player-2", It.IsAny<CancellationToken>())).ReturnsAsync(new List<int>());

            await _service.GetRecommendationsAsync(UserId);
            await _service.GetRecommendationsAsync("player-2");

            _gemini.Verify(g => g.GenerateJsonAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ClearRecommendations_NextCallBuildsFreshResult()
        {
            SetupCandidates(Candidate(1));
            SetupAiResponse(AiJson((1, "First")));
            await _service.GetRecommendationsAsync(UserId);

            _service.ClearRecommendations(UserId);
            SetupAiResponse(AiJson((1, "Second")));
            var result = await _service.GetRecommendationsAsync(UserId);

            Assert.Equal("Second", result.Games[0].Reason);
        }

        [Fact]
        public async Task GetRecommendationsAsync_EmptyResult_IsNotCached()
        {
            await _service.GetRecommendationsAsync(UserId);
            await _service.GetRecommendationsAsync(UserId);

            _repository.Verify(r => r.GetCandidateGamesAsync(It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
