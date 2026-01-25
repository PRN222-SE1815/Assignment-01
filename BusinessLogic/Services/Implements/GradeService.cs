using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements;

public class GradeService : IGradeService
{
    private readonly IGradeRepository _gradeRepo;

    public GradeService(IGradeRepository gradeRepo)
    {
        _gradeRepo = gradeRepo;
    }

    public async Task<List<GradeResponse>> GetAllAsync()
    {
        var data = await _gradeRepo.GetAllAsync();
        return data.Select(MapToResponse).ToList();
    }

    public async Task<GradeResponse?> GetByIdAsync(int id)
    {
        var entity = await _gradeRepo.GetByIdAsync(id);
        return entity == null ? null : MapToResponse(entity);
    }

    public async Task<List<EnrollmentOptionResponse>> GetEnrollmentOptionsAsync()
    {
        var enrollments = await _gradeRepo.GetEnrollmentsAsync();
        return enrollments.Select(e => new EnrollmentOptionResponse
        {
            EnrollmentId = e.EnrollmentId,
            DisplayText = $"{e.Student.User.FullName} - {e.Course.CourseName}"
        }).ToList();
    }

    public async Task<int> CreateAsync(GradeUpsertRequest request)
    {
        var entity = new Grade
        {
            EnrollmentId = request.EnrollmentId,
            Assignment = request.Assignment,
            Midterm = request.Midterm,
            Final = request.Final,
            Total = CalculateTotal(request.Assignment, request.Midterm, request.Final)
        };

        await _gradeRepo.CreateAsync(entity);
        return entity.GradeId;
    }

    public async Task<bool> UpdateAsync(int id, GradeUpsertRequest request)
    {
        var entity = await _gradeRepo.GetByIdAsync(id);
        if (entity == null) return false;

        entity.EnrollmentId = request.EnrollmentId;
        entity.Assignment = request.Assignment;
        entity.Midterm = request.Midterm;
        entity.Final = request.Final;
        entity.Total = CalculateTotal(request.Assignment, request.Midterm, request.Final);

        await _gradeRepo.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _gradeRepo.GetByIdAsync(id);
        if (entity == null) return false;

        await _gradeRepo.DeleteAsync(entity);
        return true;
    }

    private static decimal? CalculateTotal(decimal? a, decimal? m, decimal? f)
    {
        if (!a.HasValue || !m.HasValue || !f.HasValue) return null;
        return Math.Round((a.Value + m.Value + f.Value) / 3m, 2);
    }

    private static GradeResponse MapToResponse(Grade g)
    {
        return new GradeResponse
        {
            GradeId = g.GradeId,
            EnrollmentId = g.EnrollmentId,
            CourseId = g.Enrollment.CourseId,
            StudentName = g.Enrollment.Student.User.FullName,
            CourseName = g.Enrollment.Course.CourseName,
            Assignment = g.Assignment,
            Midterm = g.Midterm,
            Final = g.Final,
            Total = g.Total
        };
    }
    public async Task<List<CourseOptionResponse>> GetCourseOptionsAsync()
    {
        var courses = await _gradeRepo.GetCoursesAsync();
        return courses.Select(c => new CourseOptionResponse
        {
            CourseId = c.CourseId,
            CourseName = c.CourseName
        }).ToList();
    }

    public async Task<List<EnrollmentOptionResponse>> GetEnrollmentOptionsByCourseAsync(int courseId)
    {
        var enrollments = await _gradeRepo.GetEnrollmentsByCourseAsync(courseId);

        return enrollments.Select(e => new EnrollmentOptionResponse
        {
            EnrollmentId = e.EnrollmentId,
            DisplayText = e.Student.User.FullName                                                     
        }).ToList();
    }

}
