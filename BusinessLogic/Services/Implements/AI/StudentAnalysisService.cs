using BusinessLogic.DTOs.AI;
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

        private AiStudentDataDTO BuildAiStudentDto(Student student)
        {
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

            return new AiStudentDataDTO
            {
                StudentName = student.User?.FullName ?? "Unknown",
                GPA = gpa,
                Scores = courseScores
            };
        }

        public async Task<AiAnalysisResult> AnalyzeStudent(int studentId)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Grades)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                throw new Exception("Student not found");

            var dto = BuildAiStudentDto(student);

            return await _ai.AnalyzeAsync(dto);
        }

        public async Task<string> ChatWithStudent(int studentId, string message)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
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
