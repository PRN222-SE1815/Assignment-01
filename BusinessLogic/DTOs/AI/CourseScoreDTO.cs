
namespace BusinessLogic.DTOs.AI
{
    public class CourseScoreDTO
    {
        public int CourseId { get; set; }
        public string Course { get; set; }
        public int Credits { get; set; }

        public double? Assignment { get; set; }
        public double? Midterm { get; set; }
        public double? Final { get; set; }
        public double? Total { get; set; }

        // Shortcut để AI dùng nhanh
        public double Score => Total ?? 0;
    }

}
