using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class ScheduleRepository : IScheduleRepository
{
    private readonly SchoolManagementDbContext _context;

    public ScheduleRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ScheduleEvent>> GetScheduleEventsBySectionIdsAsync(
        IReadOnlyCollection<int> classSectionIds,
        IReadOnlyCollection<string> statuses,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc)
    {
        if (classSectionIds.Count == 0)
        {
            return Array.Empty<ScheduleEvent>();
        }

        return await _context.ScheduleEvents
            .AsNoTracking()
            .Include(se => se.Recurrence)
            .Include(se => se.ClassSection)
                .ThenInclude(cs => cs.Course)
            .Include(se => se.ClassSection)
                .ThenInclude(cs => cs.Teacher)
                    .ThenInclude(t => t.TeacherNavigation)
            .Where(se => classSectionIds.Contains(se.ClassSectionId)
                         && statuses.Contains(se.Status)
                         && (se.RecurrenceId != null || (se.StartAt <= rangeEndUtc && se.EndAt >= rangeStartUtc)))
            .OrderBy(se => se.StartAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ScheduleEvent>> GetScheduleEventsByTeacherAsync(
        int teacherId,
        IReadOnlyCollection<string> statuses,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc)
    {
        return await _context.ScheduleEvents
            .AsNoTracking()
            .Include(se => se.Recurrence)
            .Include(se => se.ClassSection)
                .ThenInclude(cs => cs.Course)
            .Include(se => se.ClassSection)
                .ThenInclude(cs => cs.Teacher)
                    .ThenInclude(t => t.TeacherNavigation)
            .Where(se => statuses.Contains(se.Status)
                         && (se.TeacherId == teacherId || se.ClassSection.TeacherId == teacherId)
                         && (se.RecurrenceId != null || (se.StartAt <= rangeEndUtc && se.EndAt >= rangeStartUtc)))
            .OrderBy(se => se.StartAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ScheduleEventOverride>> GetOverridesByRecurrenceIdsAsync(
        IReadOnlyCollection<int> recurrenceIds,
        DateOnly rangeStart,
        DateOnly rangeEnd)
    {
        if (recurrenceIds.Count == 0)
        {
            return Array.Empty<ScheduleEventOverride>();
        }

        return await _context.ScheduleEventOverrides
            .AsNoTracking()
            .Where(o => recurrenceIds.Contains(o.RecurrenceId)
                        && o.OriginalDate >= rangeStart
                        && o.OriginalDate <= rangeEnd)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ScheduleChangeLog>> GetChangeLogsAsync(long scheduleEventId)
    {
        return await _context.ScheduleChangeLogs
            .AsNoTracking()
            .Where(l => l.ScheduleEventId == scheduleEventId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public Task<ScheduleEvent?> GetScheduleEventAsync(long scheduleEventId)
    {
        return _context.ScheduleEvents
            .Include(se => se.Recurrence)
            .Include(se => se.ClassSection)
                .ThenInclude(cs => cs.Course)
            .Include(se => se.ClassSection)
                .ThenInclude(cs => cs.Teacher)
                    .ThenInclude(t => t.TeacherNavigation)
            .SingleOrDefaultAsync(se => se.ScheduleEventId == scheduleEventId);
    }

    public async Task CreateScheduleEventAsync(ScheduleEvent scheduleEvent)
    {
        _context.ScheduleEvents.Add(scheduleEvent);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateScheduleEventAsync(ScheduleEvent scheduleEvent)
    {
        _context.ScheduleEvents.Update(scheduleEvent);
        await _context.SaveChangesAsync();
    }

    public async Task UpsertScheduleOverrideAsync(ScheduleEventOverride scheduleOverride)
    {
        var existing = await _context.ScheduleEventOverrides
            .SingleOrDefaultAsync(o => o.RecurrenceId == scheduleOverride.RecurrenceId
                                       && o.OriginalDate == scheduleOverride.OriginalDate);

        if (existing == null)
        {
            _context.ScheduleEventOverrides.Add(scheduleOverride);
        }
        else
        {
            existing.OverrideType = scheduleOverride.OverrideType;
            existing.NewStartAt = scheduleOverride.NewStartAt;
            existing.NewEndAt = scheduleOverride.NewEndAt;
            existing.NewLocation = scheduleOverride.NewLocation;
            existing.NewTeacherId = scheduleOverride.NewTeacherId;
            existing.Reason = scheduleOverride.Reason;
        }

        await _context.SaveChangesAsync();
    }

    public Task<Recurrence?> GetRecurrenceAsync(int recurrenceId)
    {
        return _context.Recurrences.AsNoTracking().SingleOrDefaultAsync(r => r.RecurrenceId == recurrenceId);
    }

    public async Task<IReadOnlyList<Recurrence>> GetRecurrencesByIdsAsync(IReadOnlyCollection<int> recurrenceIds)
    {
        if (recurrenceIds.Count == 0)
        {
            return Array.Empty<Recurrence>();
        }

        return await _context.Recurrences
            .AsNoTracking()
            .Where(r => recurrenceIds.Contains(r.RecurrenceId))
            .ToListAsync();
    }

    public async Task CreateRecurrenceAsync(Recurrence recurrence)
    {
        _context.Recurrences.Add(recurrence);
        await _context.SaveChangesAsync();
    }

    public async Task InsertChangeLogAsync(ScheduleChangeLog changeLog)
    {
        _context.ScheduleChangeLogs.Add(changeLog);
        await _context.SaveChangesAsync();
    }
}
