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

public class CheepCommentModel : PageModel
{
    private readonly CheepService _service;
    public int PageNumber { get; set; }
    public int TotalPageNumber { get; set; }
    public required List<CommentDTO> Comments { get; set; }
    public string CurrentAuthor { get; set; } = string.Empty;
    public int CheepId { get; set; }
    public AuthorDTO userAuthor { get; set; }
    public CheepDTO OriginalCheep { get; set; }

    public SharedChirpViewModel SharedChirpView { get; set; } = new SharedChirpViewModel();

    public CheepCommentModel(CheepService service)
    {
        _service = service;
    }
    // Runs when the site is loaded (Request Method: GET)
    public async Task<IActionResult> OnGet(int cheepId, [FromQuery] int page)
    {
        if (page <= 0)
        {
            page = 1;
        }

        CheepId = cheepId;
        PageNumber = page;
        
        // get list of comments from cheep

        //TotalPageNumber = await _service.GetTotalPageNumber(CurrentAuthor) == 0 ? 1 : await _service.GetTotalPageNumber(CurrentAuthor);
        
        OriginalCheep = await _service.GetCheepFromId(cheepId);
        if (OriginalCheep == null)
        {
            return NotFound();
        }
        userAuthor = await _service.FindAuthorByName(User.Identity.Name);
        Comments = await _service.GetCommentsByCheepId(cheepId);
        PageNumber = pageNumber;
        

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
        
        PageNumber = pageNumber;
        TotalPageNumber = await _service.GetTotalPageNumber(CurrentAuthor) == 0 ? 1 : await _service.GetTotalPageNumber(CurrentAuthor);
        
        if (!ModelState.IsValid) // Check if the model state is invalid
        {
            // Ensure Cheeps and other required properties are populated
            OriginalCheep = await _service.GetCheepFromId(CheepId);
            Comments = await _service.GetCommentsByCheepId(CheepId);
            userAuthor = await _service.FindAuthorByName(User.Identity.Name);
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
        return Page();
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
        return Page();
    }
    
    public async Task<IActionResult> OnPostAddCommentToCheep(int cheepId, string text)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var authorName = User.Identity.Name;

            if (authorName != null)
            {
                await _service.AddCommentToCheep(await _service.GetCheepFromId(cheepId), text, User.Identity.Name);
            }
        }

        return RedirectToPage(new { cheepId, page = PageNumber });
    }
}