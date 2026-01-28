using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Student;

[Authorize(Roles = nameof(UserRole.STUDENT))]
public class StudentHomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("~/Views/Student/Index.cshtml");
    }
}
