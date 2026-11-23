using System.Net;
using System.Text;
using System.Text.Json;
using IndieVault.Services.Implementations.ExternalApis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace IndieVault.Tests.Services.ExternalApis
{
    public class GeminiServiceTests
    {
        private static readonly object Schema = new { type = "object" };

        private const string InteractionJson = """
            {
              "id": "int_123",
              "status": "completed",
              "steps": [
                { "type": "thought", "status": "done", "content": [ { "type": "text", "text": "thinking..." } ] },
                { "type": "model_output", "status": "done", "content": [ { "type": "text", "text": "{\"recommendations\":[]}" } ] }
              ]
            }
            """;

        // Records the request (including its body) and returns a canned response. No real network calls.
        private class CapturingHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            private readonly string _responseBody;

            public CapturingHandler(string responseBody, HttpStatusCode status = HttpStatusCode.OK)
            {
                _responseBody = responseBody;
                _status = status;
            }

            public int CallCount { get; private set; }
            public HttpMethod? Method { get; private set; }
            public string? Path { get; private set; }
            public string? ApiKeyHeader { get; private set; }
            public string? RequestBody { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                Method = request.Method;
                Path = request.RequestUri!.PathAndQuery;
                ApiKeyHeader = request.Headers.TryGetValues("x-goog-api-key", out var values) ? values.Single() : null;
                RequestBody = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

                return new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
                };
            }
        }

        private static GeminiService Service(CapturingHandler handler, string? apiKey = "test-key", string? model = "gemini-test-model")
        {
            var settings = new Dictionary<string, string?>
            {
                ["Gemini:ApiKey"] = apiKey,
                ["Gemini:Model"] = model
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/") };

            return new GeminiService(client, NullLogger<GeminiService>.Instance, configuration);
        }

        [Fact]
        public async Task GenerateJsonAsync_PostsToInteractionsWithKeyInHeader()
        {
            var handler = new CapturingHandler(InteractionJson);

            await Service(handler).GenerateJsonAsync("Recommend games", Schema);

            Assert.Equal(1, handler.CallCount);
            Assert.Equal(HttpMethod.Post, handler.Method);
            Assert.Equal("/v1beta/interactions", handler.Path);   // key is not in the URL
            Assert.Equal("test-key", handler.ApiKeyHeader);
        }

        [Fact]
        public async Task GenerateJsonAsync_SendsModelPromptAndJsonResponseFormat()
        {
            var handler = new CapturingHandler(InteractionJson);

            await Service(handler).GenerateJsonAsync("Recommend games", Schema);

            using var body = JsonDocument.Parse(handler.RequestBody!);
            var root = body.RootElement;
            Assert.Equal("gemini-test-model", root.GetProperty("model").GetString());
            Assert.Equal("Recommend games", root.GetProperty("input").GetString());
            var format = root.GetProperty("response_format");
            Assert.Equal("text", format.GetProperty("type").GetString());
            Assert.Equal("application/json", format.GetProperty("mime_type").GetString());
            Assert.Equal("object", format.GetProperty("schema").GetProperty("type").GetString());
        }

        [Fact]
        public async Task GenerateJsonAsync_NoModelConfigured_UsesDefaultModel()
        {
            var handler = new CapturingHandler(InteractionJson);

            await Service(handler, model: null).GenerateJsonAsync("Recommend games", Schema);

            using var body = JsonDocument.Parse(handler.RequestBody!);
            Assert.Equal(GeminiService.DefaultModel, body.RootElement.GetProperty("model").GetString());
        }

        [Fact]
        public async Task GenerateJsonAsync_ReturnsTextOfModelOutputStepOnly()
        {
            var handler = new CapturingHandler(InteractionJson);

            var result = await Service(handler).GenerateJsonAsync("Recommend games", Schema);

            Assert.Equal("{\"recommendations\":[]}", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Your-Key-Here")]
        public async Task GenerateJsonAsync_ApiKeyMissing_ReturnsNullWithoutCallingApi(string? apiKey)
        {
            var handler = new CapturingHandler(InteractionJson);

            var result = await Service(handler, apiKey: apiKey).GenerateJsonAsync("Recommend games", Schema);

            Assert.Null(result);
            Assert.Equal(0, handler.CallCount);
        }

        [Theory]
        [InlineData(HttpStatusCode.TooManyRequests)]      // free tier quota used up
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GenerateJsonAsync_ApiError_ReturnsNull(HttpStatusCode status)
        {
            var handler = new CapturingHandler("{}", status);

            Assert.Null(await Service(handler).GenerateJsonAsync("Recommend games", Schema));
        }

        [Theory]
        [InlineData("not json at all")]
        [InlineData("{}")]
        [InlineData("""{ "steps": [] }""")]
        [InlineData("""{ "steps": [ { "type": "thought", "content": [ { "type": "text", "text": "hmm" } ] } ] }""")]
        public async Task GenerateJsonAsync_ResponseWithoutModelText_ReturnsNull(string responseBody)
        {
            var handler = new CapturingHandler(responseBody);

            Assert.Null(await Service(handler).GenerateJsonAsync("Recommend games", Schema));
        }

        [Fact]
        public async Task GenerateJsonAsync_RequestCancelled_Throws()
        {
            var handler = new CapturingHandler(InteractionJson);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                Service(handler).GenerateJsonAsync("Recommend games", Schema, cts.Token));
        }

        [Fact]
        public void ExtractModelText_JoinsMultipleTextBlocks()
        {
            const string json = """
                { "steps": [ { "type": "model_output", "content": [ { "type": "text", "text": "{\"a\":" }, { "type": "text", "text": "1}" } ] } ] }
                """;

            Assert.Equal("{\"a\":1}", GeminiService.ExtractModelText(json));
        }
    }
}
