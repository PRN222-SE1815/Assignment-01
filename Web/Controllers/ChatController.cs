using BusinessLogic.Interfaces.AI;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IStudentAnalysisService _service;

        public ChatController(IStudentAnalysisService service)
        {
            _service = service;
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Chat(int id, [FromBody] string message)
        {
            var reply = await _service.ChatWithStudent(id, message);
            return Ok(new { reply });
        }
    }
}