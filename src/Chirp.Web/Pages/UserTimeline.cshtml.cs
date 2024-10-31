﻿using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheepDTO = Chirp.Core.DTOs.CheepDTO;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly CheepService _service;
    public int PageNumber { get; set; }
    public int TotalPageNumber { get; set; }
    
    [BindProperty]
    public string Text  { get; set; }
    public required List<CheepDTO> Cheeps { get; set; }

    public UserTimelineModel(CheepService service)
    {
        _service = service;
    }

    // Runs when site is loaded (Request Method:GET)
    public async Task<IActionResult> OnGet(string author, [FromQuery] int page)
    {
        if (!(page is int) || page <= 0)
        {
            page = 1;
        }
        
        PageNumber = page;
        Cheeps = await _service.GetCheepsFromAuthor(author, page);
        TotalPageNumber = await _service.GetTotalPageNumber(author);
        return Page();
    }
    public async Task<IActionResult> OnPost(CheepDTO cheepDTO)
    {
        if (User.Identity.IsAuthenticated)
        {
            var AuthorName = User.Identity.Name;
            await _service.CreateCheep(cheepDTO, AuthorName);
        }

        return RedirectToPage("UserTimeline", new { author = User.Identity.Name, page = 1 });
    }
    
    
    
    
}