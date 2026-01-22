using BusinessLogic.Interfaces.AI;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.DTOs.AI;

namespace Web.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentAnalysisService _service;

        public StudentController(IStudentAnalysisService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Analysis(int id)
        {
            var result = await _service.AnalyzeStudent(id);

            var vm = new StudentAiViewModel
            {
                StudentId = id,
                Analysis = result
            };

            return View(vm);
        }

    }

}
