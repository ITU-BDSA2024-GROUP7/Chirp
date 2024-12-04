using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Chirp.Web.Pages.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheepDTO = Chirp.Core.DTOs.CheepDTO;

namespace Chirp.Web.Pages;

public class PopularTimelineModel : PageModel
{
    private readonly CheepService _service;
    public int PageNumber { get; set; }
    public int TotalPageNumber { get; set; }
    public AuthorDTO UserAuthor { get; set; }
    public bool ShowPopularCheeps { get; set; }
    public required List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    public Dictionary<int, List<string>> TopReactions { get; set; } = new Dictionary<int, List<string>>();

    public SharedChirpViewModel SharedChirpView { get; set; } = new SharedChirpViewModel();

    public PopularTimelineModel(CheepService service)
    {
        _service = service;
    }
    
    /// <summary>
    /// Gets cheeps and stores them in a list when the page is loaded.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnGet([FromQuery] int page)
    {
        if (page <= 0)
        {
            page = 1;
        }

        PageNumber = page;
        Cheeps = await _service.GetPopularCheeps(page);
        TotalPageNumber = await _service.GetTotalPageNumberForPopular();


        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var currentUserName = User.Identity.Name;
            if (currentUserName != null) UserAuthor = await _service.FindAuthorByName(currentUserName);
        }
        
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
    [StringLength(160, ErrorMessage = "Maximum length is {1} characters")]
    public string CheepText { get; set; } = string.Empty;
    [BindProperty]
    public IFormFile? CheepImage { get; set; }
    public async Task<IActionResult> OnPost()
    {
        
        if (!ModelState.IsValid) // Check if the model state is invalid
        {
            // Gets current page number
            PageNumber = pageNumber;
            
            // Ensure Cheeps and other required properties are populated
            Cheeps = await _service.GetCheeps(PageNumber);
            
            TotalPageNumber = await _service.GetTotalPageNumberForPopular();
            
            var currentUserName = User.Identity!.Name;
            if (currentUserName != null) UserAuthor = await _service.FindAuthorByName(currentUserName);

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
                        Name = authorName, // this needs to be changed to user names going forward
                        Email = authorEmail
                    },
                    Text = CheepText,
                    ImageReference = imageBase64!,
                    FormattedTimeStamp = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture) // Or however you want to format this
                };

                await _service.CreateCheep(cheepDTO);
            }
        }

        return RedirectToPage("popular", new { page = 1 });
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
            if (userAuthor != null) await _service.FollowAuthor(userAuthor, followedAuthorName);
        }
        return Redirect($"/popular?page={PageNumber}");
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
            if (userAuthor != null) await _service.UnfollowAuthor(userAuthor, followedAuthor);
        }
        return Redirect($"/popular?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostLikeMethod(int cheepId, string? emoji = null)
    {
        await _service.HandleLike(User.Identity.Name, cheepId, emoji);
        
        return Redirect($"/popular?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostDislikeMethod(int cheepId, string? emoji = null)
    {
        await _service.HandleDislike(User.Identity.Name, cheepId, emoji);
        
        return Redirect($"/popular?page={PageNumber}");
    }

    public async Task<IActionResult> OnPostShowPopularCheeps(int pageNumber)
    {
        PageNumber = pageNumber;
        ShowPopularCheeps = true;

        Cheeps = await _service.GetPopularCheeps(pageNumber);
        TotalPageNumber = await _service.GetTotalPageNumberForPopular();

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var currentUserName = User.Identity.Name;
            if (currentUserName != null) UserAuthor = await _service.FindAuthorByName(currentUserName);
        }

        return Page();
    }
    
    public string ConvertLinksToAnchors(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Regular expression to detect URLs
        var regex = new Regex(@"((http|https):\/\/)?(www\.)?[a-zA-Z0-9-]+\.[a-zA-Z]{2,}(\S*[^.,\s])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        // Replace URLs with anchor tags
        return regex.Replace(text, match => $"<a href=\"{match.Value}\" target=\"_blank\">{match.Value}</a>");
    }


}