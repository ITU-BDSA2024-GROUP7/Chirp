using System.ComponentModel.DataAnnotations;
using Chirp.Core;
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
    public string CurrentAuthor;
    public UserTimelineModel(CheepService service)
    {
        _service = service;
    }
    public Author userAuthor { get; set; }

    // Runs when the site is loaded (Request Method: GET)
    public async Task<IActionResult> OnGet(string author, [FromQuery] int page)
    {
        if (page <= 0)
        {
            page = 1;
        }

        CurrentAuthor = author;
        PageNumber = page;
        // Cheeps = await _service.GetCheepsFromAuthor(author, page);
        Cheeps = await _service.GetCheeps(page);
        TotalPageNumber = await _service.GetTotalPageNumber(author);
        
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var currentUserName = User.Identity.Name;
            userAuthor = await _service.FindAuthorByName(currentUserName);
        }
        
        return Page();
    }
    
    [BindProperty]
    [Required(ErrorMessage = "At least write something before you click me....")]
    [StringLength(160, ErrorMessage = "Maximum length is {1}")]
    [Display(Name = "Message Text")]
    public string CheepText { get; set; }
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid) // Check if the model state is invalid
        {
            // Ensure Cheeps and other required properties are populated
            Cheeps = await _service.GetCheepsFromAuthor(CurrentAuthor, PageNumber);
            TotalPageNumber = await _service.GetTotalPageNumber();
            return Page(); // Return the page with validation messages
        }
        
        if (User.Identity.IsAuthenticated)
        {
            var authorName = User.Identity.Name;
            var authorEmail = User.Identity.Name;

            // Create the new CheepDTO
            var cheepDTO = new CheepDTO
            {
                Author = new AuthorDTO
                {
                    Name = authorName, // this needs to be changed to user names going forward
                    Email = authorEmail 
                },
                Text = CheepText,
                FormattedTimeStamp = DateTime.UtcNow.ToString() // Or however you want to format this
            };

            await _service.CreateCheep(cheepDTO);
        }

        return RedirectToPage("UserTimeline", new { author = User.Identity.Name, page = 1 });
    }
    
    /// <summary>
    /// Follows an author
    /// </summary>
    /// <param name="followedAuthorName"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostFollowMethod(string followedAuthorName)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) // Check if the user is authenticated
        {
            var userAuthor = User.Identity.Name; // Get the user's name
            await _service.FollowAuthor(userAuthor, followedAuthorName);
            
        }
        return RedirectToPage("UserTimeline", new { author = User.Identity.Name, page = 1 });
    }

    /// <summary>
    /// Unfollows an author
    /// </summary>
    /// <param name="followedAuthor"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostUnfollowMethod(string followedAuthor)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) // Check if the user is authenticated
        {
            var userAuthor = User.Identity.Name; // Get the user's name
            await _service.UnfollowAuthor(userAuthor, followedAuthor);
            
        }

        return RedirectToPage("UserTimeline", new { author = User.Identity.Name, page = 1 });
    }
}