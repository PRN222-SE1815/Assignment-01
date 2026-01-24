using BusinessLogic.DTOs.AI;
using BusinessLogic.Interfaces.AI;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace BusinessLogic.Services.Implements.AI
{
    public class GeminiService : IOpenAiService
    {
        private readonly HttpClient _http;
        private readonly string _key;

        public GeminiService(HttpClient http, IOptions<GeminiConfig> cfg)
        {
            _http = http;
            _key = cfg.Value.ApiKey;
        }

        // ================= ANALYZE (RULE-BASED) =================
        public Task<AiAnalysisResult> AnalyzeAsync(AiStudentDataDTO data)
        {
            var gpa = data.GPA;

            return Task.FromResult(new AiAnalysisResult
            {
                Level = gpa >= 8 ? "Excellent" : gpa >= 6.5 ? "Good" : "Weak",
                Trend = "Based on GPA",
                Risk = gpa < 5 ? "High" : "Low",
                Strength = gpa >= 7 ? "Good academic performance" : "Basic understanding",
                Weakness = gpa < 7 ? "Needs improvement" : "None",
                Recommendation = "Use chat for specific questions"
            });
        }

        // ================= CHAT (STRICT ANSWER) =================
        public async Task<string> ChatAsync(AiStudentDataDTO data, string message)
        {
            var prompt = $"""
You are an academic advisor.

Student info:
- GPA: {data.GPA}
- Scores:
{string.Join("\n", data.Scores.Select(s => $"- {s.Course}: {s.Total}"))}

IMPORTANT RULES:
- Answer ONLY what is asked in the question.
- No extra explanations.
- You MAY give a simple recommendation
  ONLY if it can be directly inferred from the scores.
- Keep the answer short.
- If the question cannot be answered from the data, reply exactly: "Not enough information."

Question:
{message}
""";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var modelName = "gemini-2.5-flash";

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1/models/{modelName}:generateContent?key={_key}"
            )
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                )
            };

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Gemini error
            if (root.TryGetProperty("error", out var error))
            {
                return "Gemini API Error: " + error.GetProperty("message").GetString();
            }

            // No candidates
            if (!root.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() == 0)
            {
                return "Gemini returned no answer.";
            }

            var text = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return string.IsNullOrWhiteSpace(text)
                ? "Gemini returned empty response."
                : text.Trim();
        }
    }
}
