using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheepDTO = Chirp.Core.DTOs.CheepDTO;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly CheepService _service;
    public int PageNumber { get; set; }
    public int TotalPageNumber { get; set; }
    
    [BindProperty]
    public string Text  { get; set; }

    public required List<CheepDTO> Cheeps { get; set; }

    public PublicModel(CheepService service)
    {
        _service = service;
    }
    /// <summary>
    /// Gets cheeps and stores them in a list, when the page is loaded
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnGet([FromQuery] int page)
    {
        if (!(page is int) || page <= 0)
        {
            page = 1;
        }
        
        PageNumber = page;
        Cheeps = await _service.GetCheeps(page);
        TotalPageNumber = await _service.GetTotalPageNumber();
        
        return Page();
    }
    public async Task<IActionResult> OnPost(CheepDTO cheepDTO)
    {
        if (User.Identity.IsAuthenticated)
        {
            var AuthorName = User.Identity.Name;
            await _service.CreateCheep(cheepDTO, AuthorName);
        }

        return RedirectToPage("Public", new { page = 1 });
    }
}