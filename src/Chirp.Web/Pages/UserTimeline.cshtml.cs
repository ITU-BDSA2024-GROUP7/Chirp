using System.ComponentModel.DataAnnotations;
using Chirp.Core;
using System.Globalization;
using System.Text.RegularExpressions;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Chirp.Web.Pages.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheepDTO = Chirp.Core.DTOs.CheepDTO;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    public readonly CheepService _service;
    public int PageNumber { get; set; }
    public int TotalPageNumber { get; set; }
    public int AuthorKarma { get; set; }
    public SharedChirpViewModel SharedViewModel { get; set; } = new SharedChirpViewModel();
    public required List<CheepDTO> Cheeps { get; set; }
    public required List<String> FollowingList { get; set; } 
    public required List<String> FollowingMeList { get; set; }
    public Dictionary<int, List<string>> TopReactions { get; set; } = new Dictionary<int, List<string>>();

    public string CurrentAuthor { get; set; } = string.Empty;
    public AuthorDTO? PageAuthor { get; set; }
    public AuthorDTO? UserAuthor { get; set; }

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
        PageAuthor = await _service.FindAuthorByName(author);
        
        if (PageAuthor == null)
        {
            return RedirectToPage("Public", new { page = 1 });
        }
        
        PageAuthor = await _service.FindAuthorByName(author);
        
        PageNumber = page;
        AuthorKarma = await _service.GetKarmaForAuthor(author);
        
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
            if (currentUserName != null) UserAuthor = await _service.FindAuthorByName(currentUserName);
        }
        
        // var followAuthorDto = await _service.FindAuthorByName(author);
        // FollowList = followAuthorDto.AuthorsFollowed as List<string>;
        FollowingList = await _service.GetFollowedAuthors(author);
        FollowingMeList = await _service.GetFollowingAuthors(author);
        
        foreach (var cheep in Cheeps)
        {
            TopReactions[cheep.CheepId] = await _service.GetTopReactions(cheep.CheepId);
        }
        
        return Page();
    }
    
    [BindProperty]
    public int pageNumber { get; set; }
    [BindProperty]
    [Required(ErrorMessage = "At least write something before you click me....")]
    [StringLength(160, ErrorMessage = "Maximum length is {1}")]
    public string CheepText { get; set; } = string.Empty; 
    [BindProperty]
    public IFormFile? CheepImage { get; set; }
    public async Task<IActionResult> OnPost()
    {
        string? currentAuthor = RouteData.Values["author"]?.ToString();
        if (currentAuthor != null) CurrentAuthor = currentAuthor;

        UserAuthor = await _service.FindAuthorByName(User.Identity?.Name ?? string.Empty);
        
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
                string? imageBase64 = null;
                if (CheepImage != null && CheepImage.Length > 0)
                {
                    imageBase64 = await _service.HandleImageUpload(CheepImage);
                }
                
                // Create the new CheepDTO
                var cheepDTO = new CheepDTO
                {
                    Author = new AuthorDTO
                    {
                        Name = authorName, // this needs to be changed to usernames going forward
                        Email = authorEmail
                    },
                    Text = CheepText,
                    ImageReference = imageBase64!,
                    FormattedTimeStamp =
                        DateTime.UtcNow.ToString(CultureInfo.CurrentCulture) // Or however you want to format this
                };

                await _service.CreateCheep(cheepDTO);
            }
        }

        // Returns to the authors timeline
        // return RedirectToPage("UserTimeline", new { author = currentAuthor, page = 1 });
        
        // Returns to the users timeline
        return RedirectToPage("UserTimeline", new { author = User.Identity!.Name, page = 1 });
    }
    
    /// <summary>
    /// Follows an author
    /// </summary>
    /// <param name="followedAuthorName"></param>
    /// <param name="currentAuthorPageName"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostFollowMethod(string followedAuthorName, string currentAuthorPageName)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) // Check if the user is authenticated
        {
            PageNumber = pageNumber;
            var UserAuthor = User.Identity.Name; // Get the user's name
            if (UserAuthor != null)
            {
                await _service.FollowAuthor(UserAuthor, followedAuthorName);
            }
            
        }
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
    }

    /// <summary>
    /// Unfollows an author
    /// </summary>
    /// <param name="followedAuthor"></param>
    /// <param name="currentAuthorPageName"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostUnfollowMethod(string followedAuthor, string currentAuthorPageName)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated) // Check if the user is authenticated
        {
            PageNumber = pageNumber;
            var UserAuthor = User.Identity.Name; // Get the user's name

            if (UserAuthor != null)
            {
                await _service.UnfollowAuthor(UserAuthor, followedAuthor);
            }
        }
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostLikeMethod(int cheepId, string currentAuthorPageName, string? emoji = null)
    {
        await _service.HandleLike(User.Identity!.Name!, cheepId, emoji);
        
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostDislikeMethod(int cheepId, string currentAuthorPageName, string? emoji = null)
    {
        await _service.HandleDislike(User.Identity!.Name!, cheepId, emoji);
        
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostDeleteMethod(int cheepId, string currentAuthorPageName)
    {
        await _service.DeleteCheep(cheepId);
        
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
    }
}