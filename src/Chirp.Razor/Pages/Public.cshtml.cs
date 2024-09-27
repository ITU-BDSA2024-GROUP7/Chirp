using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepViewModel> Cheeps { get; set; }

    public PublicModel(ICheepService service)
    {
        _service = service;
    }
    /// <summary>
    /// Gets cheeps and stores them in a list, when the page is loaded
    /// </summary>
    /// <returns></returns>
    public ActionResult OnGet([FromQuery] int page)
    {
        Cheeps = _service.GetCheeps(page);
        return Page();
    }
    /// <summary>
    /// Gets cheeps based on a query to get the Cheeps dedicated to the given page (or just the default start page), when the page is loaded
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    // public ActionResult OnGet([FromQuery] int? page)
    // {
    //     int startPage = page ?? 1;
    //     Cheeps = _service.GetCheeps();
    //     return Page();
    // }
}
