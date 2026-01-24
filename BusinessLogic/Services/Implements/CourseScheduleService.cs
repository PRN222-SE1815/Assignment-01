using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements
{
    public class CourseScheduleService : ICourseScheduleService
    {
        private readonly ICourseScheduleRepository _scheduleRepository;
        private readonly ICourseRepository _courseRepository;
        private static readonly string[] CourseColors = new[]
        {
            "#4facfe", "#00f2fe", "#43e97b", "#38f9d7",
            "#fa709a", "#fee140", "#30cfd0", "#330867",
            "#667eea", "#764ba2", "#f093fb", "#f5576c"
        };

        public CourseScheduleService(
            ICourseScheduleRepository scheduleRepository,
            ICourseRepository courseRepository)
        {
            _scheduleRepository = scheduleRepository;
            _courseRepository = courseRepository;
        }

        public async Task<List<CalendarEventDto>> GetStudentCalendarAsync(int studentUserId, DateOnly fromDate, DateOnly toDate)
        {
            var schedules = await _scheduleRepository.GetSchedulesByStudentUserIdAsync(studentUserId);
            return ExpandSchedulesToEvents(schedules, fromDate, toDate);
        }

        public async Task<List<CalendarEventDto>> GetTeacherCalendarAsync(int teacherUserId, DateOnly fromDate, DateOnly toDate)
        {
            var courseIds = await _courseRepository.GetCourseIdsByTeacherUserIdAsync(teacherUserId);
            var schedules = await _scheduleRepository.GetSchedulesByCourseIdsAsync(courseIds);
            return ExpandSchedulesToEvents(schedules, fromDate, toDate);
        }

        private List<CalendarEventDto> ExpandSchedulesToEvents(List<CourseSchedule> schedules, DateOnly fromDate, DateOnly toDate)
        {
            var events = new List<CalendarEventDto>();

            foreach (var schedule in schedules)
            {
                // Find the effective start and end dates
                var effectiveStart = fromDate > schedule.StartDate ? fromDate : schedule.StartDate;
                var effectiveEnd = toDate < schedule.EndDate ? toDate : schedule.EndDate;

                if (effectiveStart > effectiveEnd)
                    continue;

                // Generate all occurrences
                var currentDate = effectiveStart;
                while (currentDate <= effectiveEnd)
                {
                    if ((int)currentDate.DayOfWeek == schedule.DayOfWeek)
                    {
                        var startDateTime = currentDate.ToDateTime(schedule.StartTime);
                        var endDateTime = currentDate.ToDateTime(schedule.EndTime);

                        events.Add(new CalendarEventDto
                        {
                            Id = $"{schedule.CourseScheduleId}_{currentDate:yyyyMMdd}",
                            Title = $"{schedule.Course.CourseCode} - {schedule.Course.CourseName}",
                            Start = startDateTime,
                            End = endDateTime,
                            BackgroundColor = GetCourseColor(schedule.CourseId),
                            BorderColor = GetCourseColor(schedule.CourseId),
                            ExtendedProps = new Dictionary<string, object>
                            {
                                { "courseId", schedule.CourseId },
                                { "courseCode", schedule.Course.CourseCode },
                                { "courseName", schedule.Course.CourseName },
                                { "location", schedule.Location ?? "TBA" },
                                { "teacherName", schedule.Course.Teacher?.User?.FullName ?? "N/A" },
                                { "note", schedule.Note ?? "" }
                            }
                        });
                    }

                    currentDate = currentDate.AddDays(1);
                }
            }

            return events.OrderBy(e => e.Start).ToList();
        }

        private string GetCourseColor(int courseId)
        {
            // Generate consistent color based on course ID
            return CourseColors[courseId % CourseColors.Length];
        }
    }
}


