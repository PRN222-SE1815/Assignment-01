using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces
{
    public interface ICourseScheduleService
    {
        Task<List<CalendarEventDto>> GetStudentCalendarAsync(int studentUserId, DateOnly fromDate, DateOnly toDate);
        Task<List<CalendarEventDto>> GetTeacherCalendarAsync(int teacherUserId, DateOnly fromDate, DateOnly toDate);
    }
}

