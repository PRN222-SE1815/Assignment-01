using BusinessLogic.DTOs.Request;
using BusinessLogic.DTOs.Response;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using System.Text.Json;

namespace BusinessLogic.Services.Implements;

public sealed class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ISemesterRepository _semesterRepository;
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPublisher _notificationPublisher;

    public ScheduleService(
        IScheduleRepository scheduleRepository,
        IEnrollmentRepository enrollmentRepository,
        ISemesterRepository semesterRepository,
        IClassSectionRepository classSectionRepository,
        INotificationRepository notificationRepository,
        INotificationPublisher notificationPublisher)
    {
        _scheduleRepository = scheduleRepository;
        _enrollmentRepository = enrollmentRepository;
        _semesterRepository = semesterRepository;
        _classSectionRepository = classSectionRepository;
        _notificationRepository = notificationRepository;
        _notificationPublisher = notificationPublisher;
    }

    public async Task<IReadOnlyList<AdminScheduleEventDto>> GetScheduleEventsAsync(int classSectionId)
    {
        var classSection = await _classSectionRepository.GetSectionDetailAsync(classSectionId);
        if (classSection == null)
        {
            return Array.Empty<AdminScheduleEventDto>();
        }

        var rangeStartUtc = CreateRangeStartUtc(classSection.Semester.StartDate);
        var rangeEndUtc = CreateRangeEndUtc(classSection.Semester.EndDate);

        var events = await _scheduleRepository.GetScheduleEventsBySectionIdsAsync(new[] { classSectionId }, new[]
        {
            ScheduleEventStatus.DRAFT.ToString(),
            ScheduleEventStatus.PUBLISHED.ToString(),
            ScheduleEventStatus.RESCHEDULED.ToString(),
            ScheduleEventStatus.CANCELLED.ToString(),
            ScheduleEventStatus.COMPLETED.ToString(),
            ScheduleEventStatus.ARCHIVED.ToString()
        }, rangeStartUtc, rangeEndUtc);

        return events.Select(e => new AdminScheduleEventDto
        {
            ScheduleEventId = e.ScheduleEventId,
            ClassSectionId = e.ClassSectionId,
            SectionCode = e.ClassSection.SectionCode,
            CourseName = e.ClassSection.Course.CourseName,
            Title = e.Title,
            StartAtUtc = e.StartAt,
            EndAtUtc = e.EndAt,
            Status = e.Status,
            Timezone = e.Timezone,
            Location = e.Location,
            OnlineUrl = e.OnlineUrl,
            TeacherId = e.TeacherId,
            TeacherName = e.Teacher?.TeacherNavigation?.FullName ?? e.ClassSection.Teacher.TeacherNavigation?.FullName,
            RecurrenceId = e.RecurrenceId,
            RecurrenceRule = e.Recurrence?.RRule,
            RecurrenceStartDate = e.Recurrence?.StartDate,
            RecurrenceEndDate = e.Recurrence?.EndDate
        }).ToList();
    }

    public async Task<IReadOnlyList<ScheduleOccurrenceDto>> GetStudentScheduleAsync(ScheduleQueryRequest request)
    {
        var semester = request.SemesterId > 0
            ? await _semesterRepository.GetSemesterAsync(request.SemesterId)
            : await _semesterRepository.GetActiveSemesterAsync();
        if (semester == null)
        {
            return Array.Empty<ScheduleOccurrenceDto>();
        }

        var rangeStart = request.RangeStart ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var rangeEnd = request.RangeEnd ?? rangeStart.AddDays(42);
        if (rangeEnd < rangeStart)
        {
            rangeEnd = rangeStart;
        }

        var enrollments = await _enrollmentRepository.GetStudentEnrollmentsAsync(
            request.StudentId,
            semester.SemesterId,
            new[] { EnrollmentStatus.ENROLLED.ToString() });

        var sectionCodeLookup = enrollments
            .GroupBy(e => e.ClassSectionId)
            .ToDictionary(g => g.Key, g => g.First().ClassSection.SectionCode);

        var sectionIds = enrollments.Select(e => e.ClassSectionId).Distinct().ToList();
        if (sectionIds.Count == 0)
        {
            return Array.Empty<ScheduleOccurrenceDto>();
        }

        var rangeStartUtc = CreateRangeStartUtc(rangeStart);
        var rangeEndUtc = CreateRangeEndUtc(rangeEnd);

        var scheduleEvents = await _scheduleRepository.GetScheduleEventsBySectionIdsAsync(sectionIds, new[]
        {
            ScheduleEventStatus.PUBLISHED.ToString(),
            ScheduleEventStatus.RESCHEDULED.ToString()
        }, rangeStartUtc, rangeEndUtc);

        var recurrenceIds = scheduleEvents
            .Where(se => se.RecurrenceId.HasValue)
            .Select(se => se.RecurrenceId!.Value)
            .Distinct()
            .ToList();

        var overrides = await _scheduleRepository.GetOverridesByRecurrenceIdsAsync(recurrenceIds, rangeStart, rangeEnd);
        var occurrences = BuildOccurrences(scheduleEvents, overrides, rangeStart, rangeEnd);

        foreach (var occurrence in occurrences)
        {
            if (sectionCodeLookup.TryGetValue(occurrence.ClassSectionId, out var code))
            {
                occurrence.SectionCode = code;
            }
        }

        return occurrences.OrderBy(o => o.StartAtUtc).ToList();
    }

    public async Task<IReadOnlyList<ScheduleOccurrenceDto>> GetTeacherScheduleAsync(ScheduleQueryRequest request)
    {
        var semester = request.SemesterId > 0
            ? await _semesterRepository.GetSemesterAsync(request.SemesterId)
            : await _semesterRepository.GetActiveSemesterAsync();
        if (semester == null)
        {
            return Array.Empty<ScheduleOccurrenceDto>();
        }

        var rangeStart = request.RangeStart ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var rangeEnd = request.RangeEnd ?? rangeStart.AddDays(42);
        if (rangeEnd < rangeStart)
        {
            rangeEnd = rangeStart;
        }

        var rangeStartUtc = CreateRangeStartUtc(rangeStart);
        var rangeEndUtc = CreateRangeEndUtc(rangeEnd);

        var scheduleEvents = await _scheduleRepository.GetScheduleEventsByTeacherAsync(request.TeacherId, new[]
        {
            ScheduleEventStatus.PUBLISHED.ToString(),
            ScheduleEventStatus.RESCHEDULED.ToString()
        }, rangeStartUtc, rangeEndUtc);

        if (scheduleEvents.Count == 0)
        {
            return Array.Empty<ScheduleOccurrenceDto>();
        }

        var recurrenceIds = scheduleEvents
            .Where(se => se.RecurrenceId.HasValue)
            .Select(se => se.RecurrenceId!.Value)
            .Distinct()
            .ToList();

        var overrides = await _scheduleRepository.GetOverridesByRecurrenceIdsAsync(recurrenceIds, rangeStart, rangeEnd);
        var occurrences = BuildOccurrences(scheduleEvents, overrides, rangeStart, rangeEnd);

        return occurrences.OrderBy(o => o.StartAtUtc).ToList();
    }

    public async Task<AdminScheduleEventDto?> GetScheduleEventDetailAsync(long scheduleEventId)
    {
        var e = await _scheduleRepository.GetScheduleEventAsync(scheduleEventId);
        if (e == null)
        {
            return null;
        }

        return new AdminScheduleEventDto
        {
            ScheduleEventId = e.ScheduleEventId,
            ClassSectionId = e.ClassSectionId,
            SectionCode = e.ClassSection.SectionCode,
            CourseName = e.ClassSection.Course.CourseName,
            Title = e.Title,
            StartAtUtc = e.StartAt,
            EndAtUtc = e.EndAt,
            Status = e.Status,
            Timezone = e.Timezone,
            Location = e.Location,
            OnlineUrl = e.OnlineUrl,
            TeacherId = e.TeacherId,
            TeacherName = e.Teacher?.TeacherNavigation?.FullName ?? e.ClassSection.Teacher.TeacherNavigation?.FullName,
            RecurrenceId = e.RecurrenceId,
            RecurrenceRule = e.Recurrence?.RRule,
            RecurrenceStartDate = e.Recurrence?.StartDate,
            RecurrenceEndDate = e.Recurrence?.EndDate
        };
    }

    public async Task<bool> DetectConflictsAsync(int studentId, int classSectionId, int semesterId)
    {
        var semester = await _semesterRepository.GetSemesterAsync(semesterId);
        if (semester == null)
        {
            return false;
        }

        var existingEnrollments = await _enrollmentRepository.GetStudentEnrollmentsAsync(
            studentId,
            semesterId,
            new[] { EnrollmentStatus.ENROLLED.ToString() });

        var existingSectionIds = existingEnrollments
            .Select(e => e.ClassSectionId)
            .Where(id => id != classSectionId)
            .Distinct()
            .ToList();

        var targetSectionIds = new List<int> { classSectionId };

        if (existingSectionIds.Count == 0)
        {
            return false;
        }

        var rangeStartUtc = CreateRangeStartUtc(semester.StartDate);
        var rangeEndUtc = CreateRangeEndUtc(semester.EndDate);

        var existingEvents = await _scheduleRepository.GetScheduleEventsBySectionIdsAsync(existingSectionIds, new[]
        {
            ScheduleEventStatus.PUBLISHED.ToString(),
            ScheduleEventStatus.RESCHEDULED.ToString()
        }, rangeStartUtc, rangeEndUtc);

        var targetScheduleEvents = await _scheduleRepository.GetScheduleEventsBySectionIdsAsync(targetSectionIds, new[]
        {
            ScheduleEventStatus.PUBLISHED.ToString(),
            ScheduleEventStatus.RESCHEDULED.ToString()
        }, rangeStartUtc, rangeEndUtc);

        if (targetScheduleEvents.Count == 0)
        {
            return false;
        }

        var recurrenceIds = existingEvents
            .Concat(targetScheduleEvents)
            .Where(se => se.RecurrenceId.HasValue)
            .Select(se => se.RecurrenceId!.Value)
            .Distinct()
            .ToList();

        var overrides = await _scheduleRepository.GetOverridesByRecurrenceIdsAsync(recurrenceIds, semester.StartDate, semester.EndDate);
        var existingOccurrences = BuildOccurrences(existingEvents, overrides, semester.StartDate, semester.EndDate);
        var targetOccurrences = BuildOccurrences(targetScheduleEvents, overrides, semester.StartDate, semester.EndDate);

        foreach (var target in targetOccurrences)
        {
            foreach (var existing in existingOccurrences)
            {
                if (target.StartAtUtc < existing.EndAtUtc && target.EndAtUtc > existing.StartAtUtc)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async Task<OperationResult> CreateScheduleEventAsync(CreateScheduleEventRequest request)
    {
        if (request.EndAtUtc <= request.StartAtUtc)
        {
            return OperationResult.Failed("End time must be after start time.");
        }

        var classSection = await _classSectionRepository.GetSectionForRegistrationAsync(request.ClassSectionId);
        if (classSection == null)
        {
            return OperationResult.Failed("Class section not found.");
        }

        Recurrence? recurrence = null;
        if (!string.IsNullOrWhiteSpace(request.RecurrenceRule))
        {
            if (!request.RecurrenceStartDate.HasValue || !request.RecurrenceEndDate.HasValue)
            {
                return OperationResult.Failed("Recurrence start and end dates are required.");
            }

            if (request.RecurrenceEndDate.Value < request.RecurrenceStartDate.Value)
            {
                return OperationResult.Failed("Recurrence end date must be after start date.");
            }

            recurrence = new Recurrence
            {
                RRule = request.RecurrenceRule.Trim(),
                StartDate = request.RecurrenceStartDate.Value,
                EndDate = request.RecurrenceEndDate.Value,
                CreatedAt = DateTime.UtcNow
            };

            await _scheduleRepository.CreateRecurrenceAsync(recurrence);
        }

        var scheduleEvent = new ScheduleEvent
        {
            ClassSectionId = request.ClassSectionId,
            Title = request.Title.Trim(),
            StartAt = request.StartAtUtc,
            EndAt = request.EndAtUtc,
            Timezone = request.Timezone.Trim(),
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            OnlineUrl = string.IsNullOrWhiteSpace(request.OnlineUrl) ? null : request.OnlineUrl.Trim(),
            TeacherId = request.TeacherId,
            Status = ScheduleEventStatus.DRAFT.ToString(),
            RecurrenceId = recurrence?.RecurrenceId,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _scheduleRepository.CreateScheduleEventAsync(scheduleEvent);
        await InsertChangeLogAsync(scheduleEvent, "CREATE", null, request.CreatedBy, null);
        return OperationResult.Ok("Schedule event created.");
    }

    public async Task<OperationResult> UpdateScheduleStatusAsync(UpdateScheduleStatusRequest request)
    {
        var scheduleEvent = await _scheduleRepository.GetScheduleEventAsync(request.ScheduleEventId);
        if (scheduleEvent == null)
        {
            return OperationResult.Failed("Schedule event not found.");
        }

        var originalStatus = scheduleEvent.Status;

        scheduleEvent.Status = request.Status.Trim();
        scheduleEvent.UpdatedBy = request.UpdatedBy;
        scheduleEvent.UpdatedAt = DateTime.UtcNow;

        await _scheduleRepository.UpdateScheduleEventAsync(scheduleEvent);
        var changeType = request.Status == ScheduleEventStatus.PUBLISHED.ToString()
            ? "PUBLISH"
            : request.Status == ScheduleEventStatus.CANCELLED.ToString()
                ? "CANCEL"
                : "UPDATE";

        var oldSnapshot = SnapshotScheduleEvent(scheduleEvent, originalStatus);
        await InsertChangeLogAsync(scheduleEvent, changeType, oldSnapshot, request.UpdatedBy, request.Reason);
        await NotifyScheduleChangeAsync(scheduleEvent, changeType, request.Reason);
        return OperationResult.Ok("Schedule event updated.");
    }

    public async Task<OperationResult> UpdateScheduleEventAsync(UpdateScheduleEventRequest request)
    {
        if (request.EndAtUtc <= request.StartAtUtc)
        {
            return OperationResult.Failed("End time must be after start time.");
        }

        var scheduleEvent = await _scheduleRepository.GetScheduleEventAsync(request.ScheduleEventId);
        if (scheduleEvent == null)
        {
            return OperationResult.Failed("Schedule event not found.");
        }

        var originalSnapshot = SnapshotScheduleEvent(scheduleEvent);

        scheduleEvent.Title = request.Title.Trim();
        scheduleEvent.StartAt = request.StartAtUtc;
        scheduleEvent.EndAt = request.EndAtUtc;
        scheduleEvent.Timezone = request.Timezone.Trim();
        scheduleEvent.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        scheduleEvent.OnlineUrl = string.IsNullOrWhiteSpace(request.OnlineUrl) ? null : request.OnlineUrl.Trim();
        scheduleEvent.TeacherId = request.TeacherId;
        scheduleEvent.UpdatedBy = request.UpdatedBy;
        scheduleEvent.UpdatedAt = DateTime.UtcNow;

        await _scheduleRepository.UpdateScheduleEventAsync(scheduleEvent);
        await InsertChangeLogAsync(scheduleEvent, "UPDATE", originalSnapshot, request.UpdatedBy, request.Reason);
        await NotifyScheduleChangeAsync(scheduleEvent, "UPDATE", request.Reason);

        return OperationResult.Ok("Schedule event updated.");
    }

    public async Task<OperationResult> CreateOverrideAsync(CreateScheduleOverrideRequest request)
    {
        var scheduleEvent = await _scheduleRepository.GetScheduleEventAsync(request.ScheduleEventId);
        if (scheduleEvent == null)
        {
            return OperationResult.Failed("Schedule event not found.");
        }

        var recurrence = await _scheduleRepository.GetRecurrenceAsync(request.RecurrenceId);
        if (recurrence == null)
        {
            return OperationResult.Failed("Recurrence not found.");
        }

        if (request.OverrideType == ScheduleEventOverrideType.RESCHEDULE.ToString())
        {
            if (!request.NewStartAtUtc.HasValue || !request.NewEndAtUtc.HasValue)
            {
                return OperationResult.Failed("New start and end times are required for reschedule.");
            }

            if (request.NewEndAtUtc <= request.NewStartAtUtc)
            {
                return OperationResult.Failed("Override end time must be after start time.");
            }
        }

        var overrideEntity = new ScheduleEventOverride
        {
            RecurrenceId = request.RecurrenceId,
            OriginalDate = request.OriginalDate,
            OverrideType = request.OverrideType.Trim(),
            NewStartAt = request.NewStartAtUtc,
            NewEndAt = request.NewEndAtUtc,
            NewLocation = string.IsNullOrWhiteSpace(request.NewLocation) ? null : request.NewLocation.Trim(),
            NewTeacherId = request.NewTeacherId,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _scheduleRepository.UpsertScheduleOverrideAsync(overrideEntity);
        var changeType = request.OverrideType == ScheduleEventOverrideType.CANCEL.ToString() ? "CANCEL" : "UPDATE";
        var actorUserId = request.ActorUserId > 0
            ? request.ActorUserId
            : scheduleEvent.UpdatedBy ?? scheduleEvent.CreatedBy;
        await InsertChangeLogAsync(scheduleEvent, changeType, null, actorUserId, request.Reason);
        await NotifyScheduleChangeAsync(
            scheduleEvent,
            changeType,
            request.Reason,
            request.OriginalDate,
            request.NewStartAtUtc,
            request.NewEndAtUtc,
            request.NewTeacherId);
        return OperationResult.Ok("Schedule override created.");
    }

    private static List<ScheduleOccurrenceDto> BuildOccurrences(
        IReadOnlyCollection<ScheduleEvent> events,
        IReadOnlyCollection<ScheduleEventOverride> overrides,
        DateOnly rangeStart,
        DateOnly rangeEnd)
    {
        var overrideLookup = overrides
            .GroupBy(o => (o.RecurrenceId, o.OriginalDate))
            .ToDictionary(g => g.Key, g => g.First());

        var occurrences = new List<ScheduleOccurrenceDto>();

        foreach (var scheduleEvent in events)
        {
            if (scheduleEvent.RecurrenceId == null || scheduleEvent.Recurrence == null)
            {
                var eventStartDate = DateOnly.FromDateTime(scheduleEvent.StartAt);
                var eventEndDate = DateOnly.FromDateTime(scheduleEvent.EndAt);
                if (eventEndDate < rangeStart || eventStartDate > rangeEnd)
                {
                    continue;
                }

                occurrences.Add(MapOccurrence(scheduleEvent, scheduleEvent.StartAt, scheduleEvent.EndAt, false));
                continue;
            }

            var recurrence = scheduleEvent.Recurrence;
            var rule = ParseWeeklyRule(recurrence.RRule);
            var recurrenceStart = recurrence.StartDate;
            var recurrenceEnd = rule.Until.HasValue && rule.Until.Value < recurrence.EndDate
                ? rule.Until.Value
                : recurrence.EndDate;

            var effectiveStart = recurrenceStart > rangeStart ? recurrenceStart : rangeStart;
            var effectiveEnd = recurrenceEnd < rangeEnd ? recurrenceEnd : rangeEnd;
            if (effectiveEnd < effectiveStart)
            {
                continue;
            }

            var days = rule.ByDay.Count == 0
                ? new HashSet<DayOfWeek> { scheduleEvent.StartAt.DayOfWeek }
                : rule.ByDay;

            for (var date = effectiveStart; date <= effectiveEnd; date = date.AddDays(1))
            {
                if (!days.Contains(date.DayOfWeek))
                {
                    continue;
                }

                var weeksSinceStart = (date.DayNumber - recurrenceStart.DayNumber) / 7;
                if (weeksSinceStart % rule.Interval != 0)
                {
                    continue;
                }

                var baseStart = CreateUtcDateTime(date, scheduleEvent.StartAt);
                var baseEnd = CreateUtcDateTime(date, scheduleEvent.EndAt);

                if (overrideLookup.TryGetValue((recurrence.RecurrenceId, date), out var overrideEntry))
                {
                    if (overrideEntry.OverrideType == ScheduleEventOverrideType.CANCEL.ToString())
                    {
                        continue;
                    }

                    var overrideStart = overrideEntry.NewStartAt ?? baseStart;
                    var overrideEnd = overrideEntry.NewEndAt ?? baseEnd;

                    occurrences.Add(MapOccurrence(scheduleEvent, overrideStart, overrideEnd, true, overrideEntry));
                    continue;
                }

                occurrences.Add(MapOccurrence(scheduleEvent, baseStart, baseEnd, false));
            }
        }

        return occurrences;
    }

    private static ScheduleOccurrenceDto MapOccurrence(ScheduleEvent scheduleEvent, DateTime startAtUtc, DateTime endAtUtc, bool isOverride, ScheduleEventOverride? overrideEntry = null)
    {
        var effectiveStatus = overrideEntry?.OverrideType == ScheduleEventOverrideType.CANCEL.ToString()
            ? ScheduleEventStatus.CANCELLED.ToString()
            : isOverride
                ? ScheduleEventStatus.RESCHEDULED.ToString()
                : scheduleEvent.Status;

        return new ScheduleOccurrenceDto
        {
            OccurrenceId = $"{scheduleEvent.ScheduleEventId}-{startAtUtc:yyyyMMddHHmm}",
            ScheduleEventId = scheduleEvent.ScheduleEventId,
            ClassSectionId = scheduleEvent.ClassSectionId,
            SectionCode = scheduleEvent.ClassSection.SectionCode,
            CourseName = scheduleEvent.ClassSection.Course.CourseName,
            Title = scheduleEvent.Title,
            TeacherName = overrideEntry?.NewTeacher?.TeacherNavigation?.FullName
                ?? scheduleEvent.Teacher?.TeacherNavigation?.FullName
                ?? scheduleEvent.ClassSection.Teacher.TeacherNavigation?.FullName,
            OccurrenceDate = DateOnly.FromDateTime(startAtUtc),
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            Timezone = scheduleEvent.Timezone,
            Location = overrideEntry?.NewLocation ?? scheduleEvent.Location,
            OnlineUrl = scheduleEvent.OnlineUrl,
            TeacherId = overrideEntry?.NewTeacherId ?? scheduleEvent.TeacherId,
            Status = effectiveStatus,
            IsOverride = isOverride,
            Reason = overrideEntry?.Reason
        };
    }

    private static DateTime CreateUtcDateTime(DateOnly date, DateTime referenceUtc)
    {
        return DateTime.SpecifyKind(
            new DateTime(date.Year, date.Month, date.Day, referenceUtc.Hour, referenceUtc.Minute, referenceUtc.Second),
            DateTimeKind.Utc);
    }

    private static DateTime CreateRangeStartUtc(DateOnly date)
    {
        return CreateUtcDateTime(date, new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    private static DateTime CreateRangeEndUtc(DateOnly date)
    {
        return CreateUtcDateTime(date, new DateTime(1, 1, 1, 23, 59, 59, DateTimeKind.Utc));
    }

    private async Task InsertChangeLogAsync(ScheduleEvent scheduleEvent, string changeType, string? oldSnapshotJson, int actorUserId, string? reason)
    {
        var newSnapshot = SnapshotScheduleEvent(scheduleEvent);

        var log = new ScheduleChangeLog
        {
            ScheduleEventId = scheduleEvent.ScheduleEventId,
            ActorUserId = actorUserId,
            ChangeType = changeType,
            OldJson = oldSnapshotJson,
            NewJson = newSnapshot,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _scheduleRepository.InsertChangeLogAsync(log);
    }

    private async Task NotifyScheduleChangeAsync(
        ScheduleEvent scheduleEvent,
        string changeType,
        string? reason,
        DateOnly? overrideDate = null,
        DateTime? overrideStartAtUtc = null,
        DateTime? overrideEndAtUtc = null,
        int? overrideTeacherId = null)
    {
        var recipientIds = await _enrollmentRepository.GetEnrolledUserIdsForSectionAsync(
            scheduleEvent.ClassSectionId,
            new[] { EnrollmentStatus.ENROLLED.ToString() });

        int? teacherId = overrideTeacherId ?? scheduleEvent.TeacherId;
        teacherId ??= scheduleEvent.ClassSection.TeacherId;

        var occurrenceDate = overrideDate ?? DateOnly.FromDateTime(scheduleEvent.StartAt);
        var fromStartUtc = overrideDate.HasValue
            ? CreateUtcDateTime(overrideDate.Value, scheduleEvent.StartAt)
            : scheduleEvent.StartAt;
        var fromEndUtc = overrideDate.HasValue
            ? CreateUtcDateTime(overrideDate.Value, scheduleEvent.EndAt)
            : scheduleEvent.EndAt;
        var toStartUtc = overrideStartAtUtc ?? fromStartUtc;
        var toEndUtc = overrideEndAtUtc ?? fromEndUtc;
        var trimmedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        var notificationType = changeType == "PUBLISH" ? "SCHEDULE_PUBLISHED" : "SCHEDULE_CHANGED";

        if (recipientIds.Count > 0)
        {
            var studentPayload = BuildScheduleNotificationPayload(
                scheduleEvent,
                changeType,
                occurrenceDate,
                fromStartUtc,
                fromEndUtc,
                toStartUtc,
                toEndUtc,
                trimmedReason,
                "/StudentSchedule/Index");

            await CreateAndPublishNotificationAsync(notificationType, studentPayload, recipientIds);
        }

        if (teacherId.HasValue && teacherId.Value > 0)
        {
            var teacherPayload = BuildScheduleNotificationPayload(
                scheduleEvent,
                changeType,
                occurrenceDate,
                fromStartUtc,
                fromEndUtc,
                toStartUtc,
                toEndUtc,
                trimmedReason,
                "/TeacherSchedule/Index");

            await CreateAndPublishNotificationAsync(notificationType, teacherPayload, new[] { teacherId.Value });
        }
    }

    private static ScheduleNotificationPayloadDto BuildScheduleNotificationPayload(
        ScheduleEvent scheduleEvent,
        string changeType,
        DateOnly occurrenceDate,
        DateTime fromStartUtc,
        DateTime fromEndUtc,
        DateTime? toStartUtc,
        DateTime? toEndUtc,
        string? reason,
        string linkRoute)
    {
        return new ScheduleNotificationPayloadDto
        {
            ClassSectionId = scheduleEvent.ClassSectionId,
            ScheduleEventId = scheduleEvent.ScheduleEventId,
            OccurrenceDate = occurrenceDate,
            FromStartAtUtc = fromStartUtc,
            FromEndAtUtc = fromEndUtc,
            ToStartAtUtc = toStartUtc,
            ToEndAtUtc = toEndUtc,
            Reason = reason,
            LinkRoute = linkRoute,
            ChangeType = changeType
        };
    }

    private async Task CreateAndPublishNotificationAsync(
        string notificationType,
        ScheduleNotificationPayloadDto payload,
        IReadOnlyCollection<int> recipientUserIds)
    {
        var notification = new Notification
        {
            NotificationType = notificationType,
            PayloadJson = JsonSerializer.Serialize(payload),
            Status = NotificationStatus.PENDING.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.CreateNotificationAsync(notification, recipientUserIds);
        await _notificationPublisher.PublishScheduleNotificationAsync(recipientUserIds, payload);
    }

    private static string SnapshotScheduleEvent(ScheduleEvent scheduleEvent, string? statusOverride = null)
    {
        var snapshot = new
        {
            scheduleEvent.ScheduleEventId,
            scheduleEvent.ClassSectionId,
            scheduleEvent.Title,
            scheduleEvent.StartAt,
            scheduleEvent.EndAt,
            scheduleEvent.Timezone,
            scheduleEvent.Location,
            scheduleEvent.OnlineUrl,
            scheduleEvent.TeacherId,
            Status = statusOverride ?? scheduleEvent.Status,
            scheduleEvent.RecurrenceId
        };

        return JsonSerializer.Serialize(snapshot);
    }

    private static WeeklyRule ParseWeeklyRule(string rrule)
    {
        var rule = new WeeklyRule();
        if (string.IsNullOrWhiteSpace(rrule))
        {
            return rule;
        }

        var segments = rrule.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var segment in segments)
        {
            var parts = segment.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                continue;
            }

            var key = parts[0].ToUpperInvariant();
            var value = parts[1].ToUpperInvariant();
            switch (key)
            {
                case "INTERVAL":
                    if (int.TryParse(value, out var interval) && interval > 0)
                    {
                        rule.Interval = interval;
                    }
                    break;
                case "UNTIL":
                    if (DateOnly.TryParseExact(value, "yyyyMMdd", out var until))
                    {
                        rule.Until = until;
                    }
                    break;
                case "BYDAY":
                    var days = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var day in days)
                    {
                        if (TryMapDayOfWeek(day, out var dayOfWeek))
                        {
                            rule.ByDay.Add(dayOfWeek);
                        }
                    }
                    break;
            }
        }

        return rule;
    }

    private static bool TryMapDayOfWeek(string value, out DayOfWeek dayOfWeek)
    {
        dayOfWeek = DayOfWeek.Monday;
        return value switch
        {
            "MO" => (dayOfWeek = DayOfWeek.Monday) == DayOfWeek.Monday,
            "TU" => (dayOfWeek = DayOfWeek.Tuesday) == DayOfWeek.Tuesday,
            "WE" => (dayOfWeek = DayOfWeek.Wednesday) == DayOfWeek.Wednesday,
            "TH" => (dayOfWeek = DayOfWeek.Thursday) == DayOfWeek.Thursday,
            "FR" => (dayOfWeek = DayOfWeek.Friday) == DayOfWeek.Friday,
            "SA" => (dayOfWeek = DayOfWeek.Saturday) == DayOfWeek.Saturday,
            "SU" => (dayOfWeek = DayOfWeek.Sunday) == DayOfWeek.Sunday,
            _ => false
        };
    }

    private sealed class WeeklyRule
    {
        public int Interval { get; set; } = 1;
        public DateOnly? Until { get; set; }
        public HashSet<DayOfWeek> ByDay { get; } = new();
    }
}
