namespace IndieVault.DTOs
{
    public class RecommendationResultDto
    {
        public List<RecommendedGameDto> Games { get; set; } = new();

        // True when the AI model picked at least one of the games; false when the rule-based ranking was used
        public bool GeneratedByAi { get; set; }
    }
}
