using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheepDTO = Chirp.Core.DTOs.CheepDTO;

namespace Chirp.Web.Pages;

public class CheepCommentModel : PageModel
{
    private readonly CheepService _cheepService;
    private readonly AuthorService _authorService;
    public int PageNumber { get; set; }
    public int TotalPageNumber { get; set; }
    public required List<CommentDTO> Comments { get; set; }
    public int CheepId { get; set; }
    public AuthorDTO? UserAuthor { get; set; }
    public CheepDTO? OriginalCheep { get; set; }
    
    [BindProperty]
    [Required(ErrorMessage = "At least write something before you click me....")]
    [StringLength(160, ErrorMessage = "Maximum length is {1} characters")]
    public string CommentText { get; set; } = string.Empty;
    
    public CheepCommentModel(CheepService cheepService, AuthorService authorService)
    {
        _cheepService = cheepService;
        _authorService = authorService;

    }
    /// <summary>
    /// Returns a timestamp displayed in the style as ie "posted 2 hours ago"
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
        
        OriginalCheep = await _cheepService.GetCheepFromId(cheepId);
        if (OriginalCheep == null)
        {
            return NotFound();
        }
        if (User.Identity!.IsAuthenticated)
        {
            UserAuthor = await _authorService.FindAuthorByName(User.Identity.Name!);
        }
        Comments = await _cheepService.GetCommentsByCheepId(cheepId);
        PageNumber = pageNumber;
        

        return Page();
    }
    
    [BindProperty]
    public int pageNumber { get; set; }

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
        return Redirect(Request.Headers["Referer"].ToString());
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
            var UserAuthor = User.Identity.Name; // Get the user's name
            if (UserAuthor != null) await _authorService.UnfollowAuthor(UserAuthor, followedAuthor);
            
        }
        return Redirect(Request.Headers["Referer"].ToString());
    }
    
    public async Task<IActionResult> OnPostAddCommentToCheep(int cheepId)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var authorName = User.Identity.Name;
            var cheepDtoId = await _cheepService.GetCheepFromId(cheepId);
            if (authorName != null && cheepDtoId != null && !string.IsNullOrEmpty(CommentText))
            {
                await _cheepService.AddCommentToCheep(cheepDtoId, CommentText, authorName);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Comment text cannot be empty.");
                // Ensure Cheeps and other required properties are populated
                OriginalCheep = await _cheepService.GetCheepFromId(cheepId);
                Comments = await _cheepService.GetCommentsByCheepId(cheepId);
                UserAuthor = await _authorService.FindAuthorByName(User.Identity.Name!);
                return Page(); // Return the page with validation messages
            }
        }

        return Redirect(Request.Headers["Referer"].ToString());
    }
    public async Task<IActionResult> OnPostDeleteMethod(int CommentId)
    {
        Console.WriteLine("Comment ID: " + CommentId);
        await _cheepService.DeleteComment(CommentId);
        
        return Redirect(Request.Headers["Referer"].ToString());
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