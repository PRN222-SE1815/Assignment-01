namespace BusinessLogic.Settings;

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gemini-1.5-pro";
    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models";
    public int MaxRetries { get; set; } = 2;
    public int RetryDelaySeconds { get; set; } = 2;
}
