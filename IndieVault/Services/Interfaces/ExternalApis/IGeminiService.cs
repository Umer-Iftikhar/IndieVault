namespace IndieVault.Services.Interfaces.ExternalApis
{
    public interface IGeminiService
    {
        // Sends the prompt to Gemini and returns the JSON text of its answer, or null if the call failed.
        Task<string?> GenerateJsonAsync(string prompt, object responseSchema, CancellationToken cancellationToken = default);
    }
}
