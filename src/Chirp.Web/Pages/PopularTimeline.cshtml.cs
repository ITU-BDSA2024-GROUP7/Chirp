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
    private readonly CheepService _cheepService;
    private readonly AuthorService _authorService;
    public int PageNumber { get; set; }
    public int TotalPageNumber { get; set; }
    public AuthorDTO? UserAuthor { get; set; }
    public bool ShowPopularCheeps { get; set; }
    public required List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    public Dictionary<int, List<string>> TopReactions { get; set; } = new Dictionary<int, List<string>>();

    public SharedChirpViewModel SharedChirpView { get; set; } = new SharedChirpViewModel();

    public PopularTimelineModel(CheepService cheepService, AuthorService authorService)
    {
        _cheepService = cheepService;
        _authorService = authorService;
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
        Cheeps = await _cheepService.GetPopularCheeps(page);
        TotalPageNumber = await _cheepService.GetTotalPageNumberForPopular();


        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var currentUserName = User.Identity.Name;
            if (currentUserName != null) UserAuthor = await _authorService.FindAuthorByName(currentUserName);
        }
        
        foreach (var cheep in Cheeps)
        {
            TopReactions[cheep.CheepId] = await _cheepService.GetTopReactions(cheep.CheepId);
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
            Cheeps = await _cheepService.GetCheeps(PageNumber);
            
            TotalPageNumber = await _cheepService.GetTotalPageNumberForPopular();
            
            var currentUserName = User.Identity!.Name;
            if (currentUserName != null) UserAuthor = await _authorService.FindAuthorByName(currentUserName);

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
                    imageBase64 = await _cheepService.HandleImageUpload(CheepImage);
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

                await _cheepService.CreateCheep(cheepDTO);
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
            if (userAuthor != null) await _authorService.FollowAuthor(userAuthor, followedAuthorName);
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
            if (userAuthor != null) await _authorService.UnfollowAuthor(userAuthor, followedAuthor);
        }
        return Redirect($"/popular?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostLikeMethod(int cheepId, string? emoji = null)
    {
        await _cheepService.HandleLike(User.Identity!.Name!, cheepId, emoji);
        
        return Redirect($"/popular?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostDislikeMethod(int cheepId, string? emoji = null)
    {
        await _cheepService.HandleDislike(User.Identity!.Name!, cheepId, emoji);
        
        return Redirect($"/popular?page={PageNumber}");
    }

    public async Task<IActionResult> OnPostShowPopularCheeps(int pageNumber)
    {
        PageNumber = pageNumber;
        ShowPopularCheeps = true;

        Cheeps = await _cheepService.GetPopularCheeps(pageNumber);
        TotalPageNumber = await _cheepService.GetTotalPageNumberForPopular();

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var currentUserName = User.Identity.Name;
            if (currentUserName != null) UserAuthor = await _authorService.FindAuthorByName(currentUserName);
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
    
    public string GetFormattedTimeStamp(string timeStamp)
    {
        if (!DateTime.TryParse(timeStamp, out DateTime timeStampDateTime))
        {
            return "Invalid date";
        }
        // Format the timestamp
        var CurrentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
        
        var timeDifference = CurrentTime - timeStampDateTime;
        
        if (timeDifference.TotalSeconds < 60)
        {
            return "just now";
        }
        else if (timeDifference.TotalMinutes < 60)
        {
            if ((int)timeDifference.TotalMinutes == 1)
            {
                return $"{(int)timeDifference.TotalMinutes} minute ago";
            }
            return $"{(int)timeDifference.TotalMinutes} minutes ago";
        }
        else if (timeDifference.TotalHours < 24)
        {
            if ((int)timeDifference.TotalHours == 1)
            {
                return $"{(int)timeDifference.TotalHours} hour ago";
            }
            return $"{(int)timeDifference.TotalHours} hours ago";
        }
        else if (timeDifference.TotalDays < 30)
        {
            if ((int)timeDifference.TotalDays == 1)
            {
                return $"{(int)timeDifference.TotalDays} day ago";
            }
            return $"{(int)timeDifference.TotalDays} days ago";
        }
        else if (timeDifference.TotalDays < 365)
        {
            if ((int)(timeDifference.TotalDays / 30) == 1)
            {
                return $"{(int)(timeDifference.TotalDays / 30)} month ago";
            }
            return $"{(int)(timeDifference.TotalDays / 30)} months ago";
        }
        else
        {
            if ((int)(timeDifference.TotalDays / 365) == 1)
            {
                return $"{(int)(timeDifference.TotalDays / 365)} year ago";
            }
            return $"{(int)(timeDifference.TotalDays / 365)} years ago";
        }
    }


}