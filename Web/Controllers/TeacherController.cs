using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Teacher Dashboard";
            return View();
        }
    }
}
