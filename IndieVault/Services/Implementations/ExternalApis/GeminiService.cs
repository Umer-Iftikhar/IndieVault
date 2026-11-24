using System.Text;
using System.Text.Json;
using IndieVault.DTOs;
using IndieVault.Services.Interfaces.ExternalApis;

namespace IndieVault.Services.Implementations.ExternalApis
{
    public class GeminiService : IGeminiService
    {
        public const string DefaultModel = "gemini-3.8-flash";
        private const string ApiKeyPlaceholder = "Your-Key-Here";

        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;
        private readonly IConfiguration _configuration;

        public GeminiService(HttpClient httpClient, ILogger<GeminiService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string?> GenerateJsonAsync(string prompt, object responseSchema, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == ApiKeyPlaceholder)
            {
                _logger.LogWarning("Gemini API key is not configured. Skipping AI call.");
                return null;
            }

            var model = _configuration["Gemini:Model"];
            if (string.IsNullOrWhiteSpace(model))
            {
                model = DefaultModel;
            }

            try
            {
                // Don't spend an AI call on a request the user has already abandoned
                cancellationToken.ThrowIfCancellationRequested();

                var body = new GeminiInteractionRequestDto
                {
                    Model = model,
                    Input = prompt,
                    ResponseFormat = new GeminiResponseFormatDto { Schema = responseSchema }
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, "interactions")
                {
                    Content = JsonContent.Create(body)
                };
                // The key travels in a header, never in the URL, so it does not end up in logs
                request.Headers.Add("x-goog-api-key", apiKey);

                _logger.LogInformation("Calling Gemini model {Model}", model);
                using var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    // 429 here means the free tier quota was hit
                    _logger.LogWarning("Gemini API call failed. Status Code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var text = ExtractModelText(json);
                if (text == null)
                {
                    _logger.LogWarning("Gemini API response did not contain any model text");
                }
                return text;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The user left the page; let the caller stop too
                throw;
            }
            catch (Exception ex)
            {
                // Timeouts, network errors and bad JSON all end up here: the caller falls back to rule-based results
                _logger.LogError(ex, "Gemini API error");
                return null;
            }
        }

        // An interaction response holds a list of steps. The answer is in the text blocks of the
        // "model_output" steps: { "steps": [ { "type": "model_output", "content": [ { "type": "text", "text": "..." } ] } ] }
        public static string? ExtractModelText(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("steps", out var steps) ||
                steps.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (var step in steps.EnumerateArray())
            {
                if (step.ValueKind == JsonValueKind.Object &&
                    step.TryGetProperty("type", out var stepType) &&
                    stepType.ValueKind == JsonValueKind.String &&
                    stepType.GetString() == "model_output")
                {
                    AppendTextBlocks(step, builder);
                }
            }

            return builder.Length == 0 ? null : builder.ToString();
        }

        // Collects every { "type": "text", "text": "..." } block found inside the element
        private static void AppendTextBlocks(JsonElement element, StringBuilder builder)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("type", out var type) &&
                    type.ValueKind == JsonValueKind.String &&
                    type.GetString() == "text" &&
                    element.TryGetProperty("text", out var text) &&
                    text.ValueKind == JsonValueKind.String)
                {
                    builder.Append(text.GetString());
                    return;
                }

                foreach (var property in element.EnumerateObject())
                {
                    AppendTextBlocks(property.Value, builder);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    AppendTextBlocks(item, builder);
                }
            }
        }
    }
}
