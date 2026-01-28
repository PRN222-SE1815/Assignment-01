using BusinessLogic.DTOs.Response;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements;

public sealed class StudentGradeService : IStudentGradeService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IGradebookRepository _gradebookRepository;
    private readonly IGradeItemRepository _gradeItemRepository;
    private readonly IGradeEntryRepository _gradeEntryRepository;

    public StudentGradeService(
        IEnrollmentRepository enrollmentRepository,
        IGradebookRepository gradebookRepository,
        IGradeItemRepository gradeItemRepository,
        IGradeEntryRepository gradeEntryRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _gradebookRepository = gradebookRepository;
        _gradeItemRepository = gradeItemRepository;
        _gradeEntryRepository = gradeEntryRepository;
    }

    public async Task<IReadOnlyList<GradebookDto>> GetMyGradeSectionsAsync(int studentId)
    {
        var enrollments = await _enrollmentRepository.GetStudentEnrollmentsForGradesAsync(studentId, GetGradebookStatuses());
        if (enrollments.Count == 0)
        {
            return Array.Empty<GradebookDto>();
        }

        var classSectionIds = enrollments
            .Select(enrollment => enrollment.ClassSectionId)
            .Distinct()
            .ToList();

        var gradebooks = await _gradebookRepository.GetByClassSectionIdsAsync(classSectionIds);
        return gradebooks.Select(MapGradebook).ToList();
    }

    public async Task<GradebookMatrixDto?> GetMyGradeDetailsAsync(int studentId, int classSectionId)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentBySectionAsync(studentId, classSectionId);
        if (enrollment == null)
        {
            return null;
        }

        var gradebook = await _gradebookRepository.GetByClassSectionIdAsync(classSectionId);
        if (gradebook == null)
        {
            return null;
        }

        if (gradebook.Status == GradeBookStatus.DRAFT.ToString())
        {
            return null;
        }

        var items = await _gradeItemRepository.GetByGradeBookIdAsync(gradebook.GradeBookId);
        var entries = await _gradeEntryRepository.GetByGradeBookIdAsync(gradebook.GradeBookId);
        var roster = await _enrollmentRepository.GetRosterBySectionAsync(classSectionId, GetGradebookStatuses());

        var studentEnrollment = roster.FirstOrDefault(r => r.StudentId == studentId);
        if (studentEnrollment == null)
        {
            return null;
        }

        var studentEntries = entries
            .Where(entry => entry.EnrollmentId == studentEnrollment.EnrollmentId)
            .ToList();

        var studentRow = new StudentRowDto
        {
            EnrollmentId = studentEnrollment.EnrollmentId,
            StudentId = studentEnrollment.StudentId,
            StudentCode = studentEnrollment.Student.StudentCode,
            StudentName = studentEnrollment.Student.StudentNavigation.FullName,
            FinalScore = CalculateFinalScore(items, studentEntries)
        };

        return new GradebookMatrixDto
        {
            Gradebook = MapGradebook(gradebook),
            Items = items.Select(MapItem).ToList(),
            Students = new List<StudentRowDto> { studentRow },
            Entries = studentEntries.Select(MapEntry).ToList()
        };
    }

    private static IReadOnlyList<string> GetGradebookStatuses()
    {
        return new[]
        {
            EnrollmentStatus.ENROLLED.ToString(),
            EnrollmentStatus.WITHDRAWN.ToString(),
            EnrollmentStatus.COMPLETED.ToString()
        };
    }

    private static GradebookDto MapGradebook(GradeBook gradebook)
    {
        return new GradebookDto
        {
            GradeBookId = gradebook.GradeBookId,
            ClassSectionId = gradebook.ClassSectionId,
            CourseId = gradebook.ClassSection.CourseId,
            CourseCode = gradebook.ClassSection.Course.CourseCode,
            CourseName = gradebook.ClassSection.Course.CourseName,
            SectionCode = gradebook.ClassSection.SectionCode,
            SemesterId = gradebook.ClassSection.SemesterId,
            SemesterName = gradebook.ClassSection.Semester.SemesterName,
            Status = gradebook.Status,
            Version = gradebook.Version,
            PublishedAt = gradebook.PublishedAt,
            LockedAt = gradebook.LockedAt,
            CreatedAt = gradebook.CreatedAt,
            UpdatedAt = gradebook.UpdatedAt
        };
    }

    private static GradeItemDto MapItem(GradeItem item)
    {
        return new GradeItemDto
        {
            GradeItemId = item.GradeItemId,
            GradeBookId = item.GradeBookId,
            ItemName = item.ItemName,
            MaxScore = item.MaxScore,
            Weight = item.Weight,
            IsRequired = item.IsRequired,
            SortOrder = item.SortOrder,
            CreatedAt = item.CreatedAt
        };
    }

    private static GradeEntryCellDto MapEntry(GradeEntry entry)
    {
        return new GradeEntryCellDto
        {
            GradeEntryId = entry.GradeEntryId,
            GradeItemId = entry.GradeItemId,
            EnrollmentId = entry.EnrollmentId,
            Score = entry.Score,
            UpdatedBy = entry.UpdatedBy,
            UpdatedAt = entry.UpdatedAt
        };
    }

    private static decimal? CalculateFinalScore(IReadOnlyList<GradeItem> items, IReadOnlyList<GradeEntry> entries)
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
}
