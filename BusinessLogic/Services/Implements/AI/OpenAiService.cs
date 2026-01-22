using BusinessLogic.DTOs.AI;
using BusinessLogic.Interfaces.AI;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace BusinessLogic.Services.Implements.AI
{
    public class OpenAiService : IOpenAiService
    {
        private readonly HttpClient _http;

        public OpenAiService(HttpClient http)
        {
            _http = http;
        }

        private string BuildStudentContext(AiStudentDataDTO data)
        {
            return $@"
Student name: {data.StudentName}
GPA: {data.GPA}

Courses:
{string.Join("\n", data.Scores.Select(s => $"{s.Course}: {s.Score}"))}
";
        }

        public async Task<string> ChatAsync(AiStudentDataDTO data, string message)
        {
            var prompt = $"""
You are an academic advisor AI.

Student: {data.StudentName}
GPA: {data.GPA}

Scores:
{string.Join("\n", data.Scores.Select(s => $"{s.Course}: {s.Score}"))}

User question:
{message}

Give a short, helpful academic answer.
""";

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
            new { role = "system", content = "You are an academic advisor." },
            new { role = "user", content = prompt }
        }
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _http.PostAsync(
                "v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var content = await response.Content.ReadAsStringAsync();

            // 🔥 1️⃣ Nếu OpenAI trả lỗi → show rõ
            if (!response.IsSuccessStatusCode)
            {
                return "OpenAI API Error:\n" + content;
            }

            using var doc = JsonDocument.Parse(content);

            // 🔥 2️⃣ Bắt lỗi JSON không đúng
            if (!doc.RootElement.TryGetProperty("choices", out var choices))
            {
                return "Invalid OpenAI response:\n" + content;
            }

            var msg = choices[0].GetProperty("message").GetProperty("content").GetString();

            return msg;
        }


        // dùng lại rule-based cho phần phân tích
        public Task<AiAnalysisResult> AnalyzeAsync(AiStudentDataDTO data)
        {
            var r = new AiAnalysisResult();

            if (data.GPA >= 8)
            {
                r.Level = "Excellent";
                r.Trend = "Stable high performance";
                r.Risk = "Low";
                r.Strength = "Academic excellence";
                r.Weakness = "None significant";
                r.Recommendation = "Continue advanced courses.";
            }
            else if (data.GPA >= 6.5)
            {
                r.Level = "Good";
                r.Trend = "Moderate";
                r.Risk = "Medium";
                r.Strength = "Solid foundation";
                r.Weakness = "Some weak subjects";
                r.Recommendation = "Focus on weaker courses.";
            }
            else
            {
                r.Level = "Weak";
                r.Trend = "Declining";
                r.Risk = "High";
                r.Strength = "Basic understanding";
                r.Weakness = "Low GPA";
                r.Recommendation = "Urgent improvement needed.";
            }

            return Task.FromResult(r);
        }
    }
}
