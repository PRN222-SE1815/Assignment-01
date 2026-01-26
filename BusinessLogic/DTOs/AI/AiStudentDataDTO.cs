using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.DTOs.AI
{
    public class AiStudentDataDTO
    {
        // Định danh tối thiểu
        public int StudentId { get; set; }
        public string StudentName { get; set; }

        // Học tập cốt lõi
        public double GPA { get; set; }
        public string Major { get; set; }
        public int EnrollmentYear { get; set; }

        // Danh sách môn + điểm
        public List<CourseScoreDTO> Scores { get; set; }
        public List<AiScheduleDTO> Schedules { get; set; }
        public List<CalendarEventDto> CalendarEvents { get; set; } = new();
    }
}
