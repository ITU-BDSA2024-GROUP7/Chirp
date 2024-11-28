using System.ComponentModel.DataAnnotations;
using Chirp.Core;
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
    public string CurrentAuthor { get; set; } = string.Empty;
    public AuthorDTO userAuthor { get; set; }

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
        
        if (User.Identity != null && User.Identity.Name == author) 
        {
            Cheeps = await _service.GetPrivateCheeps(page, author);
        } else {
            Cheeps = await _service.GetCheepsFromAuthor(author, page); 
        }

        TotalPageNumber = await _service.GetTotalPageNumber(CurrentAuthor) == 0 ? 1 : await _service.GetTotalPageNumber(CurrentAuthor);
        
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var currentUserName = User.Identity.Name;
            userAuthor = await _service.FindAuthorByName(currentUserName);
        }
        
        return Page();
    }
    
    [BindProperty]
    public int pageNumber { get; set; }
    [BindProperty]
    [Required(ErrorMessage = "At least write something before you click me....")]
    [StringLength(160, ErrorMessage = "Maximum length is {1}")]
    public string CheepText { get; set; } = string.Empty; 
    public async Task<IActionResult> OnPost()
    {
        string? currentAuthor = RouteData.Values["author"]?.ToString();
        CurrentAuthor = currentAuthor;

        userAuthor = await _service.FindAuthorByName(User.Identity.Name);
        
        PageNumber = pageNumber;
        TotalPageNumber = await _service.GetTotalPageNumber(CurrentAuthor) == 0 ? 1 : await _service.GetTotalPageNumber(CurrentAuthor);
        
        if (!ModelState.IsValid) // Check if the model state is invalid
        {
            // Ensure Cheeps and other required properties are populated
            if (User.Identity != null && User.Identity.Name == CurrentAuthor) 
            {
                Cheeps = await _service.GetPrivateCheeps(PageNumber, CurrentAuthor);
            } else {
                Cheeps = await _service.GetCheepsFromAuthor(CurrentAuthor, PageNumber); 
            }
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

        // Returns to the authors timeline
        // return RedirectToPage("UserTimeline", new { author = currentAuthor, page = 1 });
        
        // Returns to the users timeline
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
            PageNumber = pageNumber;
            var userAuthor = User.Identity.Name; // Get the user's name
            await _service.FollowAuthor(userAuthor, followedAuthorName);
            
        }
        return Redirect($"/{User.Identity.Name}?page={PageNumber}");
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
            PageNumber = pageNumber;
            var userAuthor = User.Identity.Name; // Get the user's name
            await _service.UnfollowAuthor(userAuthor, followedAuthor);
            
        }
        return Redirect($"/{User.Identity.Name}?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostLikeMethod(int cheepId, string currentAuthorPageName)
    {
        await _service.HandleLike(User.Identity.Name, cheepId);
        
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostDislikeMethod(int cheepId, string currentAuthorPageName)
    {
        await _service.HandleDislike(User.Identity.Name, cheepId);
        
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
    }
}