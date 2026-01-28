using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Admin;

[Authorize(Roles = nameof(UserRole.ADMIN))]
public class AdminHomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("~/Views/Admin/Index.cshtml");
    }
}
