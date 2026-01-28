using BusinessLogic.DTOs.Request;
using BusinessLogic.DTOs.Response;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements;

public sealed class EnrollmentService : IEnrollmentService
{
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ISemesterRepository _semesterRepository;
    private readonly IPrerequisiteRepository _prerequisiteRepository;
    private readonly IScheduleService _scheduleService;
    private readonly IChatService _chatService;

    public EnrollmentService(
        IClassSectionRepository classSectionRepository,
        IEnrollmentRepository enrollmentRepository,
        ISemesterRepository semesterRepository,
        IPrerequisiteRepository prerequisiteRepository,
        IScheduleService scheduleService,
        IChatService chatService)
    {
        _classSectionRepository = classSectionRepository;
        _enrollmentRepository = enrollmentRepository;
        _semesterRepository = semesterRepository;
        _prerequisiteRepository = prerequisiteRepository;
        _scheduleService = scheduleService;
        _chatService = chatService;
    }

    public async Task<IReadOnlyList<ClassSectionDto>> GetOpenSectionsAsync(int? semesterId = null)
    {
        var resolvedSemesterId = semesterId;
        if (!resolvedSemesterId.HasValue)
        {
            var activeSemester = await _semesterRepository.GetActiveSemesterAsync();
            if (activeSemester == null)
            {
                return Array.Empty<ClassSectionDto>();
            }

            resolvedSemesterId = activeSemester.SemesterId;
        }

        var sections = await _classSectionRepository.GetOpenSectionsAsync(resolvedSemesterId.Value);
        return sections.Select(MapSection).ToList();
    }

    public async Task<IReadOnlyList<EnrollmentDto>> GetMyCoursesAsync(int studentId, int? semesterId = null)
    {
        var resolvedSemesterId = semesterId;
        if (!resolvedSemesterId.HasValue)
        {
            var activeSemester = await _semesterRepository.GetActiveSemesterAsync();
            if (activeSemester == null)
            {
                return Array.Empty<EnrollmentDto>();
            }

            resolvedSemesterId = activeSemester.SemesterId;
        }

        var statuses = new[]
        {
            EnrollmentStatus.ENROLLED.ToString(),
            EnrollmentStatus.WAITLIST.ToString(),
            EnrollmentStatus.DROPPED.ToString(),
            EnrollmentStatus.WITHDRAWN.ToString(),
            EnrollmentStatus.COMPLETED.ToString(),
            EnrollmentStatus.CANCELED.ToString()
        };

        var enrollments = await _enrollmentRepository.GetStudentEnrollmentsAsync(studentId, resolvedSemesterId.Value, statuses);
        return enrollments.Select(MapEnrollment).ToList();
    }

    private async Task<ValidationResult> ValidateRegistrationAsync(int studentId, ClassSection classSection, Semester semester, int? overrideCredits)
    {
        // Check if section is open
        if (!classSection.IsOpen)
        {
            return ValidationResult.Fail("This class section is closed for registration.");
        }

        // Check registration period
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (semester.RegistrationEndDate.HasValue && today > semester.RegistrationEndDate.Value)
        {
            return ValidationResult.Fail("Registration period has ended.");
        }

        // Check if already registered for same course in same semester
        var alreadyEnrolled = await _enrollmentRepository.ExistsEnrollmentAsync(
            studentId, classSection.CourseId, classSection.SemesterId, 
            new[] { EnrollmentStatus.ENROLLED.ToString(), EnrollmentStatus.WAITLIST.ToString() });
        if (alreadyEnrolled)
        {
            return ValidationResult.Fail("You are already registered for this course in this semester.");
        }

        // Check prerequisites
        var prerequisiteCourseIds = await _prerequisiteRepository.GetPrerequisiteCourseIdsAsync(classSection.CourseId);
        if (prerequisiteCourseIds.Count > 0)
        {
            var passedCourseIds = await _prerequisiteRepository.GetPassedCourseIdsAsync(studentId);
            var unmet = prerequisiteCourseIds.Except(passedCourseIds).ToList();
            if (unmet.Count > 0)
            {
                return ValidationResult.Fail("Prerequisites not met for this course.");
            }
        }

        // Check time conflicts with currently enrolled classes
        var hasConflict = await _scheduleService.DetectConflictsAsync(
            studentId, classSection.ClassSectionId, classSection.SemesterId);
        if (hasConflict)
        {
            return ValidationResult.Fail("This class section conflicts with your existing schedule.");
        }

        // Check credit limit
        var currentCredits = overrideCredits ?? await _enrollmentRepository.GetTotalCreditsAsync(
            studentId, classSection.SemesterId, new[] { EnrollmentStatus.ENROLLED.ToString() });
        var maxCredits = semester.MaxCredits;
        if (currentCredits + classSection.Course.Credits > maxCredits)
        {
            return ValidationResult.Fail($"Adding this course would exceed the maximum credit limit ({maxCredits} credits).");
        }

        // Determine status based on capacity
        if (classSection.CurrentEnrollment >= classSection.MaxCapacity)
        {
            return ValidationResult.Success(EnrollmentStatus.WAITLIST.ToString());
        }

        return ValidationResult.Success(EnrollmentStatus.ENROLLED.ToString());
    }

    private sealed class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? DesiredStatus { get; private set; }

        private ValidationResult() { }

        public static ValidationResult Fail(string message)
            => new() { IsValid = false, ErrorMessage = message };

        public static ValidationResult Success(string status)
            => new() { IsValid = true, DesiredStatus = status };
    }

    public async Task<OperationResult> RegisterAsync(RegisterCourseRequest request)
    {
        var classSection = await _classSectionRepository.GetSectionForRegistrationAsync(request.ClassSectionId);
        if (classSection == null)
        {
            return OperationResult.Failed("Class section not found.");
        }

        var semester = classSection.Semester;
        if (semester == null)
        {
            return OperationResult.Failed("Semester data is missing for this class section.");
        }
        var validation = await ValidateRegistrationAsync(request.StudentId, classSection, semester, null);
        if (!validation.IsValid)
        {
            return OperationResult.Failed(validation.ErrorMessage ?? "Unable to register for this class section.");
        }

        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            ClassSectionId = classSection.ClassSectionId,
            SemesterId = classSection.SemesterId,
            CourseId = classSection.CourseId,
            CreditsSnapshot = classSection.Course.Credits,
            Status = validation.DesiredStatus,
            EnrolledAt = DateTime.UtcNow
        };

        var enrolled = await _enrollmentRepository.RegisterEnrollmentAsync(enrollment, validation.DesiredStatus == EnrollmentStatus.ENROLLED.ToString());
        if (!enrolled && validation.DesiredStatus == EnrollmentStatus.ENROLLED.ToString())
        {
            enrollment.Status = EnrollmentStatus.WAITLIST.ToString();
            var waitlisted = await _enrollmentRepository.RegisterEnrollmentAsync(enrollment, false);
            if (!waitlisted)
            {
                return OperationResult.Failed("Unable to register at this time. Please try again.");
            }

            return OperationResult.Ok("The class section is full. You have been added to the waitlist.");
        }

        
        if (enrollment.Status == EnrollmentStatus.ENROLLED.ToString())
        {
            await _chatService.EnsureClassChatMembershipAsync(classSection.ClassSectionId, request.StudentId);
        }

        return OperationResult.Ok(validation.DesiredStatus == EnrollmentStatus.WAITLIST.ToString()
            ? "The class section is full. You have been added to the waitlist."
            : "Enrollment successful.");
    }

    public async Task<OperationResult<PlanSimulationResultDto>> SimulatePlanAsync(int studentId, int semesterId, IReadOnlyList<int> plannedSectionIds)
    {
        if (plannedSectionIds.Count == 0)
        {
            return OperationResult<PlanSimulationResultDto>.Failed("No planned sections provided.");
        }

        var semester = await _semesterRepository.GetSemesterAsync(semesterId);
        if (semester == null)
        {
            return OperationResult<PlanSimulationResultDto>.Failed("Semester not found.");
        }

        var currentCredits = await _enrollmentRepository.GetTotalCreditsAsync(studentId, semesterId, new[]
        {
            EnrollmentStatus.ENROLLED.ToString()
        });

        var results = new List<PlanSimulationSectionDto>();
        var runningCredits = currentCredits;

        foreach (var sectionId in plannedSectionIds.Distinct())
        {
            var section = await _classSectionRepository.GetSectionForRegistrationAsync(sectionId);
            if (section == null)
            {
                results.Add(new PlanSimulationSectionDto
                {
                    ClassSectionId = sectionId,
                    IsValid = false,
                    ErrorMessage = "Class section not found."
                });
                continue;
            }

            var validation = await ValidateRegistrationAsync(studentId, section, semester, runningCredits);
            var result = new PlanSimulationSectionDto
            {
                ClassSectionId = section.ClassSectionId,
                CourseId = section.CourseId,
                CourseCode = section.Course.CourseCode,
                CourseName = section.Course.CourseName,
                SectionCode = section.SectionCode,
                Status = validation.DesiredStatus ?? string.Empty,
                IsValid = validation.IsValid,
                ErrorMessage = validation.ErrorMessage
            };

            results.Add(result);

            if (validation.IsValid && validation.DesiredStatus == EnrollmentStatus.ENROLLED.ToString())
            {
                runningCredits += section.Course.Credits;
            }
        }

        return OperationResult<PlanSimulationResultDto>.Ok(new PlanSimulationResultDto
        {
            SemesterId = semesterId,
            TotalCredits = runningCredits,
            Sections = results
        });
    }

    public async Task<OperationResult> DropAsync(DropRequest request)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(request.EnrollmentId);
        if (enrollment == null || enrollment.StudentId != request.StudentId)
        {
            return OperationResult.Failed("Enrollment not found.");
        }

        if (enrollment.Semester.AddDropDeadline.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (today > enrollment.Semester.AddDropDeadline.Value)
            {
                return OperationResult.Failed("The add/drop deadline has passed.");
            }
        }

        if (enrollment.Status != EnrollmentStatus.ENROLLED.ToString() && enrollment.Status != EnrollmentStatus.WAITLIST.ToString())
        {
            return OperationResult.Failed("Only active enrollments can be dropped.");
        }

        var shouldDecrement = enrollment.Status == EnrollmentStatus.ENROLLED.ToString();
        await _enrollmentRepository.UpdateEnrollmentStatusAsync(enrollment.EnrollmentId, EnrollmentStatus.DROPPED.ToString(), shouldDecrement);
        return OperationResult.Ok("Course dropped successfully.");
    }

    public async Task<OperationResult> WithdrawAsync(WithdrawRequest request)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(request.EnrollmentId);
        if (enrollment == null || enrollment.StudentId != request.StudentId)
        {
            return OperationResult.Failed("Enrollment not found.");
        }

        if (enrollment.Semester.WithdrawalDeadline.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (today > enrollment.Semester.WithdrawalDeadline.Value)
            {
                return OperationResult.Failed("The withdrawal deadline has passed.");
            }
        }

        if (enrollment.Status != EnrollmentStatus.ENROLLED.ToString())
        {
            return OperationResult.Failed("Only enrolled courses can be withdrawn.");
        }

        await _enrollmentRepository.UpdateEnrollmentStatusAsync(enrollment.EnrollmentId, EnrollmentStatus.WITHDRAWN.ToString(), false);
        return OperationResult.Ok("Course withdrawn successfully.");
    }

    private static ClassSectionDto MapSection(ClassSection section)
    {
        return new ClassSectionDto
        {
            ClassSectionId = section.ClassSectionId,
            CourseId = section.CourseId,
            CourseCode = section.Course.CourseCode,
            CourseName = section.Course.CourseName,
            SemesterId = section.SemesterId,
            SemesterName = section.Semester.SemesterName,
            SectionCode = section.SectionCode,
            TeacherName = section.Teacher.TeacherNavigation?.FullName,
            Credits = section.Course.Credits,
            IsOpen = section.IsOpen,
            MaxCapacity = section.MaxCapacity,
            CurrentEnrollment = section.CurrentEnrollment,
            Room = section.Room,
            OnlineUrl = section.OnlineUrl
        };
    }

    private static EnrollmentDto MapEnrollment(Enrollment enrollment)
    {
        return new EnrollmentDto
        {
            EnrollmentId = enrollment.EnrollmentId,
            ClassSectionId = enrollment.ClassSectionId,
            CourseId = enrollment.CourseId,
            CourseCode = enrollment.Course.CourseCode,
            CourseName = enrollment.Course.CourseName,
            SemesterId = enrollment.SemesterId,
            SemesterName = enrollment.Semester.SemesterName,
            SectionCode = enrollment.ClassSection.SectionCode,
            TeacherName = enrollment.ClassSection.Teacher.TeacherNavigation?.FullName,
            CreditsSnapshot = enrollment.CreditsSnapshot,
            Status = enrollment.Status
        };
    }
}
