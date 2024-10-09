using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly CheepService _service;
    public int pageNumber { get; set; }
    public List<CheepDTO> Cheeps { get; set; }

    public UserTimelineModel(CheepService service)
    {
        _service = service;
    }

    // Runs when site is loaded (Request Method:GET)
    public async Task<IActionResult> OnGet(string author, [FromQuery] int page)
    {
        if (!(page is int) || page <= 0)
        {
            page = 1;
        }
        
        pageNumber = page;
        Cheeps = await _service.GetCheepsFromAuthor(author, page);
        return Page();
    }
}
