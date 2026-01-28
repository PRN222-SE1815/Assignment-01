using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Models.Student;

public class AIChatIndexViewModel
{
    public string? SelectedPurpose { get; set; }
    public IReadOnlyList<SelectListItem> Purposes { get; set; } = Array.Empty<SelectListItem>();
}
