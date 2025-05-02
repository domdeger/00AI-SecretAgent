public class LlmSettings
{
    public required string Model { get; set; }
    public required string Endpoint { get; set; }
    public required string Token { get; set; }
    public required double Temperature { get; set; }
    public required int MaxTokens { get; set; }
    public required double TopP { get; set; }
}
