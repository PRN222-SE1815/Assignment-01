using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IScheduleRepository
{
    Task<IReadOnlyList<ScheduleEvent>> GetScheduleEventsBySectionIdsAsync(
        IReadOnlyCollection<int> classSectionIds,
        IReadOnlyCollection<string> statuses,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc);
    Task<IReadOnlyList<ScheduleEvent>> GetScheduleEventsByTeacherAsync(
        int teacherId,
        IReadOnlyCollection<string> statuses,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc);
    Task<IReadOnlyList<ScheduleEventOverride>> GetOverridesByRecurrenceIdsAsync(
        IReadOnlyCollection<int> recurrenceIds,
        DateOnly rangeStart,
        DateOnly rangeEnd);
    Task<IReadOnlyList<ScheduleChangeLog>> GetChangeLogsAsync(long scheduleEventId);
    Task<ScheduleEvent?> GetScheduleEventAsync(long scheduleEventId);
    Task CreateScheduleEventAsync(ScheduleEvent scheduleEvent);
    Task UpdateScheduleEventAsync(ScheduleEvent scheduleEvent);
    Task UpsertScheduleOverrideAsync(ScheduleEventOverride scheduleOverride);
    Task<Recurrence?> GetRecurrenceAsync(int recurrenceId);
    Task<IReadOnlyList<Recurrence>> GetRecurrencesByIdsAsync(IReadOnlyCollection<int> recurrenceIds);
    Task CreateRecurrenceAsync(Recurrence recurrence);
    Task InsertChangeLogAsync(ScheduleChangeLog changeLog);
}
