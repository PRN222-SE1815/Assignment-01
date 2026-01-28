using System.Text.Json;
using BusinessLogic.DTOs.Response;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements;

public sealed class AIChatService : IAIChatService
{
    private const int MaxMessageLength = 2000;
    private const int HistoryLimit = 20;

    private readonly IAIChatSessionRepository _sessionRepository;
    private readonly IAIChatMessageRepository _messageRepository;
    private readonly IAIToolCallRepository _toolCallRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IGradebookRepository _gradebookRepository;
    private readonly IGradeItemRepository _gradeItemRepository;
    private readonly IGradeEntryRepository _gradeEntryRepository;
    private readonly ISemesterRepository _semesterRepository;
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IGeminiClient _geminiClient;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public AIChatService(
        IAIChatSessionRepository sessionRepository,
        IAIChatMessageRepository messageRepository,
        IAIToolCallRepository toolCallRepository,
        IEnrollmentRepository enrollmentRepository,
        IGradebookRepository gradebookRepository,
        IGradeItemRepository gradeItemRepository,
        IGradeEntryRepository gradeEntryRepository,
        ISemesterRepository semesterRepository,
        IClassSectionRepository classSectionRepository,
        IScheduleRepository scheduleRepository,
        IEnrollmentService enrollmentService,
        IGeminiClient geminiClient)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _toolCallRepository = toolCallRepository;
        _enrollmentRepository = enrollmentRepository;
        _gradebookRepository = gradebookRepository;
        _gradeItemRepository = gradeItemRepository;
        _gradeEntryRepository = gradeEntryRepository;
        _semesterRepository = semesterRepository;
        _classSectionRepository = classSectionRepository;
        _scheduleRepository = scheduleRepository;
        _enrollmentService = enrollmentService;
        _geminiClient = geminiClient;
    }

    public async Task<AIChatSessionDto?> StartSessionAsync(int userId, string purpose, string? modelName, string? promptVersion)
    {
        if (userId <= 0)
        {
            return null;
        }

        var createdAt = DateTime.UtcNow;
        var sessionId = await _sessionRepository.CreateSessionAsync(userId, purpose, modelName, promptVersion, createdAt);

        var systemPrompt = BuildSystemPrompt();
        await _messageRepository.AddMessageAsync(sessionId, "SYSTEM", systemPrompt, createdAt);

        return new AIChatSessionDto
        {
            ChatSessionId = sessionId,
            Purpose = purpose,
            ModelName = modelName,
            State = "ACTIVE",
            PromptVersion = promptVersion,
            CreatedAt = createdAt
        };
    }

    public async Task<AIChatSessionDto?> GetSessionAsync(long sessionId, int userId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null || session.UserId != userId)
        {
            return null;
        }

        return new AIChatSessionDto
        {
            ChatSessionId = session.ChatSessionId,
            Purpose = session.Purpose,
            ModelName = session.ModelName,
            State = session.State,
            PromptVersion = session.PromptVersion,
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt
        };
    }

    public async Task<IReadOnlyList<AIChatMessageDto>> GetRecentMessagesAsync(long sessionId, int userId, int limit)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null || session.UserId != userId)
        {
            return Array.Empty<AIChatMessageDto>();
        }

        var messages = await _messageRepository.ListRecentMessagesAsync(sessionId, limit);
        return messages
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.ChatMessageId)
            .Select(MapMessage)
            .ToList();
    }

    public async Task<OperationResult<AIChatMessageDto>> SendUserMessageAsync(long sessionId, int userId, string content)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null || session.UserId != userId)
        {
            return OperationResult<AIChatMessageDto>.Failed("Invalid chat session.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return OperationResult<AIChatMessageDto>.Failed("Message content cannot be empty.");
        }

        var trimmed = content.Trim();
        if (trimmed.Length > MaxMessageLength)
        {
            return OperationResult<AIChatMessageDto>.Failed("Message is too long.");
        }

        var createdAt = DateTime.UtcNow;
        await _messageRepository.AddMessageAsync(sessionId, "USER", trimmed, createdAt);

        var history = await _messageRepository.ListRecentMessagesAsync(sessionId, HistoryLimit);
        var orderedHistory = history
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.ChatMessageId)
            .Select(MapMessage)
            .ToList();

        var tools = BuildToolDefinitions();
        var response = await _geminiClient.SendAsync(orderedHistory, tools, CancellationToken.None);

        if (response.ToolCalls.Count > 0)
        {
            foreach (var toolCall in response.ToolCalls)
            {
                var toolResult = await ExecuteToolAsync(toolCall, session.UserId);
                var requestJson = toolCall.ArgumentsJson;
                var responseJson = JsonSerializer.Serialize(toolResult, _serializerOptions);
                await _toolCallRepository.AddToolCallAsync(sessionId, toolCall.ToolName, requestJson, responseJson, "OK", DateTime.UtcNow);
                await _messageRepository.AddMessageAsync(sessionId, "SYSTEM", responseJson, DateTime.UtcNow);
            }

            var newHistory = await _messageRepository.ListRecentMessagesAsync(sessionId, HistoryLimit);
            orderedHistory = newHistory
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => m.ChatMessageId)
                .Select(MapMessage)
                .ToList();

            response = await _geminiClient.SendAsync(orderedHistory, tools, CancellationToken.None);
        }

        var assistantMessage = new AIChatMessageDto
        {
            SenderType = "ASSISTANT",
            Content = string.IsNullOrWhiteSpace(response.Reply)
                ? "Sorry, I could not generate a suitable response."
                : response.Reply,
            CreatedAt = DateTime.UtcNow
        };

        var assistantId = await _messageRepository.AddMessageAsync(sessionId, assistantMessage.SenderType, assistantMessage.Content, assistantMessage.CreatedAt);
        assistantMessage.ChatMessageId = assistantId;

        return OperationResult<AIChatMessageDto>.Ok(assistantMessage);
    }

    private static AIChatMessageDto MapMessage(DataAccess.Entities.AIChatMessage message)
    {
        return new AIChatMessageDto
        {
            ChatMessageId = message.ChatMessageId,
            SenderType = message.SenderType,
            Content = message.Content,
            CreatedAt = message.CreatedAt
        };
    }

    private static string BuildSystemPrompt()
    {
        return "You are a study assistant for students. Always reply in English, concise, and actionable. " +
               "Do not fabricate grades, credits, or schedules. If data is needed, call a tool. " +
               "If data is missing, ask for clarification.";
    }

    private IReadOnlyList<AIChatToolDefinitionDto> BuildToolDefinitions()
    {
        return new List<AIChatToolDefinitionDto>
        {
            new()
            {
                Name = "tool_get_student_score_summary",
                Description = "Summarize a student's scores by semester.",
                JsonSchema = "{\"type\":\"object\",\"properties\":{\"userId\":{\"type\":\"integer\"},\"semesterId\":{\"type\":\"integer\"}},\"required\":[\"userId\",\"semesterId\"]}"
            },
            new()
            {
                Name = "tool_get_student_enrollments",
                Description = "List courses the student enrolled in for the semester.",
                JsonSchema = "{\"type\":\"object\",\"properties\":{\"userId\":{\"type\":\"integer\"},\"semesterId\":{\"type\":\"integer\"}},\"required\":[\"userId\",\"semesterId\"]}"
            },
            new()
            {
                Name = "tool_get_semester_rules",
                Description = "Semester rules (deadlines, credit limits).",
                JsonSchema = "{\"type\":\"object\",\"properties\":{\"semesterId\":{\"type\":\"integer\"}},\"required\":[\"semesterId\"]}"
            },
            new()
            {
                Name = "tool_get_course_options",
                Description = "Suggest suitable class sections based on filters.",
                JsonSchema = "{\"type\":\"object\",\"properties\":{\"userId\":{\"type\":\"integer\"},\"semesterId\":{\"type\":\"integer\"},\"filters\":{\"type\":\"object\"}},\"required\":[\"userId\",\"semesterId\"]}"
            },
            new()
            {
                Name = "tool_simulate_plan",
                Description = "Simulate a class registration plan.",
                JsonSchema = "{\"type\":\"object\",\"properties\":{\"userId\":{\"type\":\"integer\"},\"semesterId\":{\"type\":\"integer\"},\"plannedSectionIds\":{\"type\":\"array\",\"items\":{\"type\":\"integer\"}}},\"required\":[\"userId\",\"semesterId\",\"plannedSectionIds\"]}"
            }
        };
    }

    private async Task<object> ExecuteToolAsync(AIChatToolCallDto toolCall, int userId)
    {
        return toolCall.ToolName switch
        {
            "tool_get_student_score_summary" => await HandleScoreSummaryAsync(toolCall, userId),
            "tool_get_student_enrollments" => await HandleEnrollmentsAsync(toolCall, userId),
            "tool_get_semester_rules" => await HandleSemesterRulesAsync(toolCall),
            "tool_get_course_options" => await HandleCourseOptionsAsync(toolCall, userId),
            "tool_simulate_plan" => await HandleSimulatePlanAsync(toolCall, userId),
            _ => new { error = "Tool not supported." }
        };
    }

    private async Task<object> HandleScoreSummaryAsync(AIChatToolCallDto toolCall, int userId)
    {
        var args = JsonSerializer.Deserialize<ScoreSummaryArgs>(toolCall.ArgumentsJson, _serializerOptions);
        if (args == null || args.UserId != userId)
        {
            return new { error = "Unauthorized." };
        }

        var semester = await _semesterRepository.GetSemesterAsync(args.SemesterId);
        if (semester == null)
        {
            return new { error = "Semester not found." };
        }

        var enrollments = await _enrollmentRepository.GetStudentEnrollmentsAsync(userId, args.SemesterId, new[]
        {
            EnrollmentStatus.ENROLLED.ToString(),
            EnrollmentStatus.WITHDRAWN.ToString(),
            EnrollmentStatus.COMPLETED.ToString()
        });

        var sectionIds = enrollments.Select(e => e.ClassSectionId).Distinct().ToList();
        var gradebooks = await _gradebookRepository.GetByClassSectionIdsAsync(sectionIds);

        var itemsLookup = new Dictionary<int, IReadOnlyList<DataAccess.Entities.GradeItem>>();
        var entriesLookup = new Dictionary<int, IReadOnlyList<DataAccess.Entities.GradeEntry>>();

        foreach (var gradebook in gradebooks)
        {
            itemsLookup[gradebook.GradeBookId] = await _gradeItemRepository.GetByGradeBookIdAsync(gradebook.GradeBookId);
            entriesLookup[gradebook.GradeBookId] = await _gradeEntryRepository.GetByGradeBookIdAsync(gradebook.GradeBookId);
        }

        var summaries = new List<ScoreSummaryItem>();
        foreach (var enrollment in enrollments)
        {
            var gradebook = gradebooks.FirstOrDefault(g => g.ClassSectionId == enrollment.ClassSectionId);
            decimal? finalScore = null;
            if (gradebook != null)
            {
                var items = itemsLookup[gradebook.GradeBookId];
                var entries = entriesLookup[gradebook.GradeBookId].Where(e => e.EnrollmentId == enrollment.EnrollmentId).ToList();
                finalScore = CalculateFinalScore(items, entries);
            }

            summaries.Add(new ScoreSummaryItem
            {
                CourseCode = enrollment.Course.CourseCode,
                CourseName = enrollment.Course.CourseName,
                SectionCode = enrollment.ClassSection.SectionCode,
                Status = enrollment.Status,
                FinalScore = finalScore
            });
        }

        var weak = summaries.Where(s => s.FinalScore.HasValue && s.FinalScore.Value < 5m).ToList();
        var strong = summaries.Where(s => s.FinalScore.HasValue && s.FinalScore.Value >= 8m).ToList();

        return new
        {
            semester = new { semester.SemesterId, semester.SemesterCode, semester.SemesterName },
            subjects = summaries.Select(s => new
            {
                courseCode = s.CourseCode,
                courseName = s.CourseName,
                sectionCode = s.SectionCode,
                status = s.Status,
                finalScore = s.FinalScore
            }),
            weakSubjectsCount = weak.Count,
            strongSubjectsCount = strong.Count
        };
    }

    private async Task<object> HandleEnrollmentsAsync(AIChatToolCallDto toolCall, int userId)
    {
        var args = JsonSerializer.Deserialize<EnrollmentArgs>(toolCall.ArgumentsJson, _serializerOptions);
        if (args == null || args.UserId != userId)
        {
            return new { error = "Unauthorized." };
        }

        var enrollments = await _enrollmentRepository.GetStudentEnrollmentsAsync(userId, args.SemesterId, new[]
        {
            EnrollmentStatus.ENROLLED.ToString(),
            EnrollmentStatus.WAITLIST.ToString(),
            EnrollmentStatus.DROPPED.ToString(),
            EnrollmentStatus.WITHDRAWN.ToString(),
            EnrollmentStatus.COMPLETED.ToString()
        });

        var items = enrollments.Select(e => new
        {
            e.ClassSectionId,
            e.CourseId,
            e.Course.CourseCode,
            e.Course.CourseName,
            e.ClassSection.SectionCode,
            e.CreditsSnapshot,
            e.Status,
            e.ClassSection.Room,
            e.ClassSection.OnlineUrl
        });

        return new { items };
    }

    private async Task<object> HandleSemesterRulesAsync(AIChatToolCallDto toolCall)
    {
        var args = JsonSerializer.Deserialize<SemesterArgs>(toolCall.ArgumentsJson, _serializerOptions);
        if (args == null)
        {
            return new { error = "Invalid request." };
        }

        var semester = await _semesterRepository.GetSemesterAsync(args.SemesterId);
        if (semester == null)
        {
            return new { error = "Semester not found." };
        }

        return new
        {
            semester.SemesterId,
            semester.SemesterCode,
            semester.SemesterName,
            semester.StartDate,
            semester.EndDate,
            semester.MaxCredits,
            semester.MinCredits,
            semester.RegistrationEndDate,
            semester.AddDropDeadline,
            semester.WithdrawalDeadline
        };
    }

    private async Task<object> HandleCourseOptionsAsync(AIChatToolCallDto toolCall, int userId)
    {
        var args = JsonSerializer.Deserialize<CourseOptionsArgs>(toolCall.ArgumentsJson, _serializerOptions);
        if (args == null || args.UserId != userId)
        {
            return new { error = "Unauthorized." };
        }

        var semester = await _semesterRepository.GetSemesterAsync(args.SemesterId);
        if (semester == null)
        {
            return new { error = "Semester not found." };
        }

        var sections = await _classSectionRepository.GetOpenSectionsAsync(args.SemesterId);
        var enrolledCourseIds = await _enrollmentRepository.GetStudentEnrollmentsAsync(userId, args.SemesterId, new[]
        {
            EnrollmentStatus.ENROLLED.ToString(),
            EnrollmentStatus.WAITLIST.ToString()
        });

        var filtered = sections.Where(s => enrolledCourseIds.All(e => e.CourseId != s.CourseId)).ToList();

        if (!string.IsNullOrWhiteSpace(args.Filters?.Keyword))
        {
            var keyword = args.Filters.Keyword.Trim().ToLowerInvariant();
            filtered = filtered.Where(s => s.Course.CourseCode.ToLowerInvariant().Contains(keyword)
                || s.Course.CourseName.ToLowerInvariant().Contains(keyword)).ToList();
        }

        var rangeStartUtc = new DateTime(semester.StartDate.Year, semester.StartDate.Month, semester.StartDate.Day, 0, 0, 0, DateTimeKind.Utc);
        var rangeEndUtc = new DateTime(semester.EndDate.Year, semester.EndDate.Month, semester.EndDate.Day, 23, 59, 59, DateTimeKind.Utc);

        var sectionIds = filtered.Select(s => s.ClassSectionId).ToList();
        var scheduleEvents = sectionIds.Count == 0
            ? Array.Empty<DataAccess.Entities.ScheduleEvent>()
            : await _scheduleRepository.GetScheduleEventsBySectionIdsAsync(sectionIds, new[]
            {
                ScheduleEventStatus.PUBLISHED.ToString(),
                ScheduleEventStatus.RESCHEDULED.ToString()
            }, rangeStartUtc, rangeEndUtc);

        var scheduleLookup = scheduleEvents.GroupBy(e => e.ClassSectionId)
            .ToDictionary(g => g.Key, g => g.Select(e => new ScheduleInfo
            {
                Title = e.Title,
                StartAt = e.StartAt,
                EndAt = e.EndAt,
                Location = e.Location,
                OnlineUrl = e.OnlineUrl
            }).ToList());

        var options = filtered.Select(section => new
        {
            section.ClassSectionId,
            section.CourseId,
            section.Course.CourseCode,
            section.Course.CourseName,
            section.SectionCode,
            section.Course.Credits,
            section.MaxCapacity,
            section.CurrentEnrollment,
            section.Room,
            section.OnlineUrl,
            schedules = scheduleLookup.TryGetValue(section.ClassSectionId, out var list) ? list : new List<ScheduleInfo>()
        });

        return new { options };
    }

    private sealed class ScheduleInfo
    {
        public string Title { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? Location { get; set; }
        public string? OnlineUrl { get; set; }
    }

    private async Task<object> HandleSimulatePlanAsync(AIChatToolCallDto toolCall, int userId)
    {
        var args = JsonSerializer.Deserialize<PlanSimulationArgs>(toolCall.ArgumentsJson, _serializerOptions);
        if (args == null || args.UserId != userId)
        {
            return new { error = "Unauthorized." };
        }

        var result = await _enrollmentService.SimulatePlanAsync(args.UserId, args.SemesterId, args.PlannedSectionIds ?? Array.Empty<int>());
        return new { result.Success, result.Message, result.Data };
    }

    private static decimal? CalculateFinalScore(IReadOnlyList<DataAccess.Entities.GradeItem> items, IReadOnlyList<DataAccess.Entities.GradeEntry> entries)
    {
        if (items.Count == 0)
        {
            return null;
        }

        var weightedItems = items.Where(item => item.Weight.HasValue).ToList();
        var hasWeights = weightedItems.Count > 0;
        var entryLookup = entries
            .Where(entry => entry.Score.HasValue)
            .ToDictionary(entry => entry.GradeItemId, entry => entry);

        decimal total = 0m;
        decimal weightSum = 0m;
        var scoredCount = 0;

        foreach (var item in items)
        {
            if (!entryLookup.TryGetValue(item.GradeItemId, out var entry))
            {
                continue;
            }

            var normalized = item.MaxScore > 0
                ? (entry.Score!.Value / item.MaxScore) * 10m
                : entry.Score!.Value;

            if (hasWeights)
            {
                if (!item.Weight.HasValue)
                {
                    continue;
                }

                total += normalized * item.Weight.Value;
                weightSum += item.Weight.Value;
            }
            else
            {
                total += normalized;
                scoredCount++;
            }
        }

        if (hasWeights)
        {
            if (weightSum <= 0)
            {
                return null;
            }

            return Math.Round(total / weightSum, 2);
        }

        if (scoredCount == 0)
        {
            return null;
        }

        return Math.Round(total / scoredCount, 2);
    }

    private sealed class ScoreSummaryArgs
    {
        public int UserId { get; set; }
        public int SemesterId { get; set; }
    }

    private sealed class EnrollmentArgs
    {
        public int UserId { get; set; }
        public int SemesterId { get; set; }
    }

    private sealed class SemesterArgs
    {
        public int SemesterId { get; set; }
    }

    private sealed class CourseOptionsArgs
    {
        public int UserId { get; set; }
        public int SemesterId { get; set; }
        public CourseFilterArgs? Filters { get; set; }
    }

    private sealed class CourseFilterArgs
    {
        public string? Keyword { get; set; }
    }

    private sealed class PlanSimulationArgs
    {
        public int UserId { get; set; }
        public int SemesterId { get; set; }
        public IReadOnlyList<int>? PlannedSectionIds { get; set; }
    }

    private sealed class ScoreSummaryItem
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? FinalScore { get; set; }
    }
}
