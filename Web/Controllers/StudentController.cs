using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Student Dashboard";
            return View();
        }
    }
}
