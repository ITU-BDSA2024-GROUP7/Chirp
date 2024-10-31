using Chirp.Core.DTOs;
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
    public string Text { get; set; }

    public required List<CheepDTO> Cheeps { get; set; }

    public PublicModel(CheepService service)
    {
        _service = service;
    }

    /// <summary>
    /// Gets cheeps and stores them in a list when the page is loaded.
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

    public async Task<IActionResult> OnPost()
    {
        if (User.Identity.IsAuthenticated)
        {
            var authorName = User.Identity.Name;
            var authorEmail = User.Identity.Name;

            // Create the new CheepDTO
            var cheepDTO = new CheepDTO
            {
                Author = new AuthorDTO
                {
                    Name = authorName,
                    Email = authorEmail // This might need to come from your user store or authentication provider
                },
                Text = Text,
                FormattedTimeStamp = DateTime.UtcNow.ToString() // Or however you want to format this
            };

            await _service.CreateCheep(cheepDTO, authorName);
        }

        return RedirectToPage("Public", new { page = 1 });
    }
}