using BusinessLogic.DTOs.AI;

namespace BusinessLogic.Interfaces.AI
{
    public interface IStudentAnalysisService
    {
        Task<AiAnalysisResult> AnalyzeStudent(int studentId);
        //Task<AiChatResponseDTO> ChatWithStudent(int studentId, string message);
        Task<string> ChatWithStudent(int studentId, string message);
    }

}
