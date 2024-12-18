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
    private readonly CheepService _cheepService;
    private readonly AuthorService _authorService;
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

    public UserTimelineModel(CheepService cheepService, AuthorService authorService)
    {
        _cheepService = cheepService;
        _authorService = authorService;
    }
    /// <summary>
    /// A method for returning a timestamp that is displayed as "posted 2 hours ago"
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns>Time</returns>
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
    // Runs when the site is loaded (Request Method: GET)
    public async Task<IActionResult> OnGet(string author, [FromQuery] int page)
    {
        if (page <= 0)
        {
            page = 1;
        }
    
        CurrentAuthor = author;
        PageAuthor = await _authorService.FindAuthorByName(author);
        
        if (PageAuthor == null)
        {
            return RedirectToPage("Public", new { page = 1 });
        }
        
        PageAuthor = await _authorService.FindAuthorByName(author);
        
        PageNumber = page;
        AuthorKarma = await _authorService.GetKarmaForAuthor(author);
        
        if (User.Identity != null && User.Identity.Name == author) 
        {
            Cheeps = await _cheepService.GetPrivateCheeps(page, author);
        } else {
            Cheeps = await _cheepService.GetCheepsFromAuthor(author, page); 
        }

        TotalPageNumber = await _cheepService.GetTotalPageNumber(CurrentAuthor) == 0 ? 1 : await _cheepService.GetTotalPageNumber(CurrentAuthor);
        
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var currentUserName = User.Identity.Name;
            if (currentUserName != null) UserAuthor = await _authorService.FindAuthorByName(currentUserName);
        }
        
        // var followAuthorDto = await _service.FindAuthorByName(author);
        // FollowList = followAuthorDto.AuthorsFollowed as List<string>;
        FollowingList = await _authorService.GetFollowedAuthors(author);
        FollowingMeList = await _authorService.GetFollowingAuthors(author);
        
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
    [StringLength(160, ErrorMessage = "Maximum length is {1}")]
    public string CheepText { get; set; } = string.Empty; 
    [BindProperty]
    public IFormFile? CheepImage { get; set; }
    public async Task<IActionResult> OnPost()
    {
        string? currentAuthor = RouteData.Values["author"]?.ToString();
        if (currentAuthor != null)
        {
            CurrentAuthor = currentAuthor;
            PageAuthor = await _authorService.FindAuthorByName(currentAuthor);
        }
        
        UserAuthor = await _authorService.FindAuthorByName(User.Identity?.Name ?? string.Empty);
        
        PageNumber = pageNumber;
        TotalPageNumber = await _cheepService.GetTotalPageNumber(CurrentAuthor) == 0 ? 1 : await _cheepService.GetTotalPageNumber(CurrentAuthor);
        
        if (!ModelState.IsValid) // Check if the model state is invalid
        {
            
            // Ensure Cheeps and other required properties are populated
            if (User.Identity != null && User.Identity.Name == CurrentAuthor) 
            {
                Cheeps = await _cheepService.GetPrivateCheeps(PageNumber, CurrentAuthor);
            } else {
                Cheeps = await _cheepService.GetCheepsFromAuthor(CurrentAuthor, PageNumber); 
            }
            
            FollowingList = await _authorService.GetFollowedAuthors(CurrentAuthor);
            FollowingMeList = await _authorService.GetFollowingAuthors(CurrentAuthor);
            
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
                        Name = authorName, // this needs to be changed to usernames going forward
                        Email = authorEmail
                    },
                    Text = CheepText,
                    ImageReference = imageBase64!,
                    FormattedTimeStamp =
                        DateTime.UtcNow.ToString(CultureInfo.CurrentCulture) // Or however you want to format this
                };

                await _cheepService.CreateCheep(cheepDTO);
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
                await _authorService.FollowAuthor(UserAuthor, followedAuthorName);
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
                await _authorService.UnfollowAuthor(UserAuthor, followedAuthor);
            }
        }
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostLikeMethod(int cheepId, string currentAuthorPageName, string? emoji = null)
    {
        await _cheepService.HandleLike(User.Identity!.Name!, cheepId, emoji);
        
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostDislikeMethod(int cheepId, string currentAuthorPageName, string? emoji = null)
    {
        await _cheepService.HandleDislike(User.Identity!.Name!, cheepId, emoji);
        
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
    }
    
    public async Task<IActionResult> OnPostDeleteMethod(int cheepId, string currentAuthorPageName)
    {
        await _cheepService.DeleteCheep(cheepId);
        
        return Redirect($"/{currentAuthorPageName}?page={PageNumber}");
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