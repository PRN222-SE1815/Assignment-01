
namespace BusinessLogic.DTOs.AI
{
    public class AiStudentDataDTO
    {
        public string StudentName { get; set; }
        public double GPA { get; set; }
        public List<CourseScoreDTO> Scores { get; set; }
    }

}
