using BusinessLogic.DTOs.AI;

namespace BusinessLogic.Interfaces.AI
{
    public interface IOpenAiService
    {
        //Task<AiAnalysisResult> AnalyzeAsync(AiStudentDataDTO data);
        //  Task<AiChatResponseDTO> ChatAsync(AiStudentDataDTO student, string message);
        //  Task<string> ChatAsync(string systemPrompt, string userMessage);
        Task<AiAnalysisResult> AnalyzeAsync(AiStudentDataDTO data);
        Task<string> ChatAsync(AiStudentDataDTO data, string message);

    }

}
