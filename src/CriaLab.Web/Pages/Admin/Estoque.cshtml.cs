using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class EstoqueModel : PageModel
{
    public IActionResult OnGet(string? aba) =>
        RedirectToPage("/Admin/Patrimonio", new { aba = string.IsNullOrWhiteSpace(aba) ? "filamentos" : aba });
}
