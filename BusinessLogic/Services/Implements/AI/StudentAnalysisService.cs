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

        public async Task<AiAnalysisResult> AnalyzeStudent(int studentId)
        {
            var student = _db.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Grades)
                .FirstOrDefault(s => s.StudentId == studentId);

            if (student == null)
                throw new Exception("Student not found");

            var scores = student.Enrollments
                .SelectMany(e => e.Grades)
                .Where(g => g.Total.HasValue)
                .Select(g => g.Total.Value)
                .ToList();

            double gpa = scores.Any() ? (double)scores.Average() : 0;

            var dto = new AiStudentDataDTO
            {
                StudentName = student.User?.FullName ?? "Unknown",
                GPA = gpa,
                Scores = student.Enrollments
                    .SelectMany(e => e.Grades, (e, g) => new { e, g })
                    .Where(x => x.g.Total.HasValue)
                    .Select(x => new CourseScoreDTO
                    {
                        Course = x.e.Course.CourseName,
                        Score = (double)x.g.Total.Value
                    })
                    .ToList()
            };

            return await _ai.AnalyzeAsync(dto);
        }

        public async Task<string> ChatWithStudent(int studentId, string message)
        {
            var student = _db.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Grades)
                .FirstOrDefault(s => s.StudentId == studentId);

            if (student == null)
                return "Student not found.";

            var scores = student.Enrollments
                .SelectMany(e => e.Grades, (e, g) => new { e, g })
                .Where(x => x.g.Total.HasValue)
                .Select(x => new CourseScoreDTO
                {
                    Course = x.e.Course.CourseName,
                    Score = (double)x.g.Total.Value
                })
                .ToList();

            var gpa = scores.Any() ? scores.Average(s => s.Score) : 0;

            var dto = new AiStudentDataDTO
            {
                StudentName = student.User?.FullName ?? "Unknown",
                GPA = gpa,
                Scores = scores
            };

            return await _ai.ChatAsync(dto, message);
        }




    }
}
