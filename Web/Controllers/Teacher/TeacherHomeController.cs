using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Teacher;

[Authorize(Roles = nameof(UserRole.TEACHER))]
public class TeacherHomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("~/Views/Teacher/Index.cshtml");
    }
}
