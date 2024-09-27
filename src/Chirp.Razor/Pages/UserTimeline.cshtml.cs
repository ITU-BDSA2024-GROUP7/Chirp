using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public int pageNumber { get; set; }
    public List<CheepViewModel> Cheeps { get; set; }

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
    }

    // Runs when site is loaded (Request Method:GET)
    public ActionResult OnGet(string author, [FromQuery] int page)
    {
        if (!(page is int) || page <= 0)
        {
            page = 1;
        }
        
        pageNumber = page;
        Cheeps = _service.GetCheepsFromAuthor(author, page);
        return Page();
    }
}
