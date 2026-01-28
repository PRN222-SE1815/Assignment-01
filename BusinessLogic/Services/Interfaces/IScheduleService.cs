using BusinessLogic.DTOs.Request;
using BusinessLogic.DTOs.Response;

namespace BusinessLogic.Services.Interfaces;

public interface IScheduleService
{
    Task<IReadOnlyList<ScheduleOccurrenceDto>> GetStudentScheduleAsync(ScheduleQueryRequest request);
    Task<IReadOnlyList<ScheduleOccurrenceDto>> GetTeacherScheduleAsync(ScheduleQueryRequest request);
    Task<bool> DetectConflictsAsync(int studentId, int classSectionId, int semesterId);
    Task<IReadOnlyList<AdminScheduleEventDto>> GetScheduleEventsAsync(int classSectionId);
    Task<AdminScheduleEventDto?> GetScheduleEventDetailAsync(long scheduleEventId);
    Task<OperationResult> CreateScheduleEventAsync(CreateScheduleEventRequest request);
    Task<OperationResult> UpdateScheduleEventAsync(UpdateScheduleEventRequest request);
    Task<OperationResult> UpdateScheduleStatusAsync(UpdateScheduleStatusRequest request);
    Task<OperationResult> CreateOverrideAsync(CreateScheduleOverrideRequest request);
}
