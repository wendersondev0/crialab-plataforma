using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class ImpressoesModel : PageModel
{
    public IActionResult OnGet() =>
        RedirectToPage("/Admin/Impressao", new { aba = "fila" });
}
