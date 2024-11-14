using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Chirp.Web.Pages.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheepDTO = Chirp.Core.DTOs.CheepDTO;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly CheepService _service;
    public int PageNumber { get; set; }
    public int TotalPageNumber { get; set; }
    public SharedChirpViewModel SharedViewModel { get; set; } = new SharedChirpViewModel();
    public required List<CheepDTO> Cheeps { get; set; }
    public string CurrentAuthor = string.Empty;
    public UserTimelineModel(CheepService service)
    {
        _service = service;
    }

    // Runs when the site is loaded (Request Method: GET)
    public async Task<IActionResult> OnGet(string author, [FromQuery] int page)
    {
        if (page <= 0)
        {
            page = 1;
        }

        CurrentAuthor = author;
        PageNumber = page;
        Cheeps = await _service.GetCheepsFromAuthor(author, page);
        TotalPageNumber = await _service.GetTotalPageNumber(author);
        return Page();
    }
    
    [BindProperty]
    [Required(ErrorMessage = "At least write something before you click me....")]
    [StringLength(160, ErrorMessage = "Maximum length is {1}")]
    public string CheepText { get; set; } = string.Empty; 
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid) // Check if the model state is invalid
        {
            // Ensure Cheeps and other required properties are populated
            Cheeps = await _service.GetCheepsFromAuthor(CurrentAuthor, PageNumber);
            TotalPageNumber = await _service.GetTotalPageNumber();
            return Page(); // Return the page with validation messages
        }
        
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var authorName = User.Identity.Name;
            var authorEmail = User.Identity.Name;

            if (authorName != null && authorEmail != null)
            {
                // Create the new CheepDTO
                var cheepDTO = new CheepDTO
                {
                    Author = new AuthorDTO
                    {
                        Name = authorName, // this needs to be changed to user names going forward
                        Email = authorEmail
                    },
                    Text = CheepText,
                    FormattedTimeStamp =
                        DateTime.UtcNow.ToString(CultureInfo.CurrentCulture) // Or however you want to format this
                };

                await _service.CreateCheep(cheepDTO);
            }
        }

        if (User.Identity != null)
        {
            return RedirectToPage("UserTimeline", new { author = User.Identity.Name, page = 1 });
        } else {
            return RedirectToPage("Public", new { page = 1 });
        }
    }
}