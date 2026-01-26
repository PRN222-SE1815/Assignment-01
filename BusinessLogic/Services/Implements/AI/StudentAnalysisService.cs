using BusinessLogic.DTOs.AI;
using BusinessLogic.DTOs.Responses;
using BusinessLogic.Interfaces.AI;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.Implements.AI
{
    public class StudentAnalysisService : IStudentAnalysisService
    {
        private readonly SchoolManagementDbContext _db;
        private readonly IOpenAiService _ai;

        public StudentAnalysisService(SchoolManagementDbContext db, IOpenAiService ai)
        {
            _db = db;
            _ai = ai;
        }

        // ================= BUILD AI DTO (SCORES + GPA + CALENDAR) =================
        private AiStudentDataDTO BuildAiStudentDto(Student student)
        {
            // ===== BUILD SCORES =====
            var courseScores = student.Enrollments
               .SelectMany(e => e.Grades, (e, g) => new CourseScoreDTO
               {
                   CourseId = e.Course.CourseId,
                   Course = e.Course.CourseName,
                   Credits = e.Course.Credits ?? 0,

                   Assignment = g.Assignment.HasValue ? (double?)g.Assignment.Value : null,
                   Midterm = g.Midterm.HasValue ? (double?)g.Midterm.Value : null,
                   Final = g.Final.HasValue ? (double?)g.Final.Value : null,
                   Total = g.Total.HasValue ? (double?)g.Total.Value : null
               })
               .Where(x => x.Total.HasValue)
               .ToList();

            var gpa = courseScores.Any()
                ? courseScores.Average(x => x.Total!.Value)
                : 0;

            // ===== BUILD CALENDAR EVENTS (THEO ĐÚNG COURSE SCHEDULE TABLE) =====
            var calendarEvents = student.Enrollments
                .SelectMany(e => e.Course.CourseSchedules.Select(cs =>
                {
                    // Convert DateOnly + TimeOnly -> DateTime
                    var startDateTime = cs.StartDate.ToDateTime(cs.StartTime);
                    var endDateTime = cs.StartDate.ToDateTime(cs.EndTime);

                    return new CalendarEventDto
                    {
                        Id = $"{e.Course.CourseId}-{cs.CourseScheduleId}",
                        Title = e.Course.CourseName,
                        Start = startDateTime,
                        End = endDateTime,

                        ExtendedProps = new Dictionary<string, object>
                        {
                { "Location", cs.Location },
                { "DayOfWeek", cs.DayOfWeek },
                { "Credits", e.Course.Credits ?? 0 }
                        }
                    };
                }))
                .ToList();


            return new AiStudentDataDTO
            {
                StudentName = student.User?.FullName ?? "Unknown",
                GPA = gpa,
                Scores = courseScores,
                CalendarEvents = calendarEvents
            };
        }


        // ================= RULE-BASED ANALYSIS =================
        public async Task<AiAnalysisResult> AnalyzeStudent(int studentId)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.CourseSchedules)   // LOAD LỊCH HỌC
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Grades)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                throw new Exception("Student not found");

            var dto = BuildAiStudentDto(student);

            return await _ai.AnalyzeAsync(dto);
        }

        // ================= CHAT WITH AI (SCORES + SCHEDULE) =================
        public async Task<string> ChatWithStudent(int studentId, string message)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.CourseSchedules)   // LOAD LỊCH HỌC
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Grades)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return "Student not found.";

            var dto = BuildAiStudentDto(student);

            return await _ai.ChatAsync(dto, message);
        }
    }
}
