using System.Net.Http.Json;
using System.Text.Json;
using BusinessLogic.DTOs.Response;
using BusinessLogic.Services.Interfaces;
using BusinessLogic.Settings;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services.Implements;

public sealed class GeminiClient : IGeminiClient
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly JsonSerializerOptions _serializerOptions;

    public GeminiClient(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<AIChatAssistantResponseDto> SendAsync(IReadOnlyList<AIChatMessageDto> messages, IReadOnlyList<AIChatToolDefinitionDto> tools, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        var url = $"{_options.Endpoint}/{_options.ModelName}:generateContent?key={_options.ApiKey}";
        var request = new
        {
            contents = messages.Select(MapMessage).ToList(),
            tools = tools.Count == 0 ? null : new[]
            {
                new
                {
                    functionDeclarations = tools.Select(MapToolDefinition).ToList()
                }
            }
        };

        var attempts = 0;
        while (true)
        {
            attempts++;
            try
            {
                using var response = await _httpClient.PostAsJsonAsync(url, request, _serializerOptions, cancellationToken);
                response.EnsureSuccessStatusCode();

                var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
                return ParseResponse(payload);
            }
            catch (Exception ex) when (attempts <= _options.MaxRetries && IsTransient(ex))
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds * attempts), cancellationToken);
            }
        }
    }

    private static object MapMessage(AIChatMessageDto message)
    {
        var role = message.SenderType.ToUpperInvariant() switch
        {
            "ASSISTANT" => "model",
            _ => "user"
        };

        return new
        {
            role,
            parts = new[] { new { text = message.Content } }
        };
    }

    private object MapToolDefinition(AIChatToolDefinitionDto tool)
    {
        var parameters = JsonSerializer.Deserialize<JsonElement>(tool.JsonSchema, _serializerOptions);
        return new
        {
            name = tool.Name,
            description = tool.Description,
            parameters
        };
    }

    private static AIChatAssistantResponseDto ParseResponse(JsonElement payload)
    {
        if (!payload.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            return new AIChatAssistantResponseDto { Reply = "" };
        }

        var content = candidates[0].GetProperty("content");
        if (!content.TryGetProperty("parts", out var parts))
        {
            return new AIChatAssistantResponseDto { Reply = "" };
        }

        var toolCalls = new List<AIChatToolCallDto>();
        var replyParts = new List<string>();

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var text))
            {
                replyParts.Add(text.GetString() ?? string.Empty);
                continue;
            }

            if (part.TryGetProperty("functionCall", out var functionCall))
            {
                var name = functionCall.GetProperty("name").GetString() ?? string.Empty;
                var args = functionCall.GetProperty("args").GetRawText();
                toolCalls.Add(new AIChatToolCallDto
                {
                    ToolName = name,
                    ArgumentsJson = args
                });
            }
        }

        return new AIChatAssistantResponseDto
        {
            Reply = string.Join("\n", replyParts).Trim(),
            ToolCalls = toolCalls
        };
    }

    private static bool IsTransient(Exception ex)
    {
        return ex is HttpRequestException or TaskCanceledException;
    }
}
