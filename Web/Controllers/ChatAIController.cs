using BusinessLogic.Interfaces.AI;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatAIController : ControllerBase
    {
        private readonly IStudentAnalysisService _service;
        private readonly IStudentRepository _studentRepository;

        public ChatAIController(
            IStudentAnalysisService service,
            IStudentRepository studentRepository)
        {
            _service = service;
            _studentRepository = studentRepository;
        }

        // GIỐNG HỆT ChatController
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] string message)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            // LẤY STUDENT THEO USER ĐĂNG NHẬP
            var student = await _studentRepository.GetStudentByUserIdAsync(userId);
            if (student == null)
                return Forbid(); // user không phải student (role ≠ 3)

            // GỌI AI CHAT
            var reply = await _service.ChatWithStudent(student.StudentId, message);

            return Ok(new { reply });
        }
    }
}
