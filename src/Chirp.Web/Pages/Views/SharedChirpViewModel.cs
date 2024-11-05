using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages.Views;

public class SharedChirpViewModel
{
    public string? CheepText { get; set; }
}