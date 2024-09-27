using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public int pageNumber { get; set; }
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
        if (!(page is int) || page <= 0)
        {
            page = 1;
        }
        
        pageNumber = page;
        Cheeps = _service.GetCheeps(page);
        return Page();
    }
}