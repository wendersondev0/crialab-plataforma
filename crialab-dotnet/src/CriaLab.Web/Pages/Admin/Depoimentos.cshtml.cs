using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class DepoimentosModel : PageModel
{
    public IActionResult OnGet() =>
        RedirectToPage("/Admin/Atendimento", new { aba = "depoimentos" });
}
