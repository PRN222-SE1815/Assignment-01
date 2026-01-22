
using BusinessLogic.DTOs.AI;
using BusinessLogic.Interfaces.AI;
using Microsoft.Extensions.Options;
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

        public Task<AiAnalysisResult> AnalyzeAsync(AiStudentDataDTO data)
        {
            // dùng rule-based cho phân tích
            var gpa = data.GPA;

            return Task.FromResult(new AiAnalysisResult
            {
                Level = gpa >= 8 ? "Excellent" : gpa >= 6.5 ? "Good" : "Weak",
                Trend = "Based on recent GPA",
                Risk = gpa < 5 ? "High" : "Low",
                Strength = gpa >= 7 ? "Good academic base" : "Basic knowledge",
                Weakness = gpa < 7 ? "Need improvement" : "None",
                Recommendation = "Use chat for detailed advice"
            });
        }

        public async Task<string> ChatAsync(AiStudentDataDTO data, string message)
        {
            var prompt = $"""
    You are an academic advisor.

    Student: {data.StudentName}
    GPA: {data.GPA}

    Scores:
    {string.Join("\n", data.Scores.Select(s => $"{s.Course}: {s.Score}"))}

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

            // Sử dụng phiên bản v1 và model gemini-2.5-flash
            var modelName = "gemini-2.5-flash";
            var req = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1/models/{modelName}:generateContent?key={_key}"
            );



            req.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var res = await _http.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 1️⃣ Nếu Gemini trả về lỗi
            if (root.TryGetProperty("error", out var err))
            {
                return "Gemini API Error: " + err.GetProperty("message").GetString();
            }

            // 2️⃣ Nếu không có candidates
            if (!root.TryGetProperty("candidates", out var candidates))
            {
                return "Gemini returned no answer.";
            }

            // 3️⃣ Parse bình thường
            return candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
        }
    }
}
