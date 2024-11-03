using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Assert = Xunit.Assert;
using Program = Chirp.Web.Program;

namespace Chirp.Test;


[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class E2ETests : PageTest
{
    private const string AppUrl = "http://localhost:5273";
    private string _startupProjectPath;
    private Process? _appProcess;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    readonly BrowserTypeLaunchOptions browserTypeLaunchOptions = new BrowserTypeLaunchOptions
    {
        Headless = false,
    };
    
    [SetUp]
    public async Task Setup()
    {
        Console.WriteLine(_startupProjectPath);
        _browser = await Playwright.Chromium.LaunchAsync(browserTypeLaunchOptions);
        
        _context = await _browser.NewContextAsync();

        _page = await _context.NewPageAsync();
        
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
    }
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var solutionDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\.."));

        // Construct the path to your project
        _startupProjectPath = Path.Combine(solutionDirectory, "src", "Chirp.Web", "Chirp.Web.csproj");
        
        // Start the ASP.NET application
        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{_startupProjectPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            }
        };

        _appProcess.Start();

        // Wait for the application to start
        await Task.Delay(5000); // Adjust delay if needed
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Stop the ASP.NET application
        if (_appProcess != null && !_appProcess.HasExited)
        {
            _appProcess.Kill();
            _appProcess.Dispose();
        }
    }
    
    //---------------------------------- HELPER METHODS ----------------------------------
    // Register
    // Login
    // Logout
    
    //---------------------------------- PUBLIC TIMELINE TESTS ----------------------------------
    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task LoadPublicTimeline()
    {
        await _page!.GotoAsync($"{AppUrl}");
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" })).ToBeVisibleAsync();
    }

    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task PublicTimelineLoadingCheeps()
    {
        await _page!.GotoAsync($"{AppUrl}");
        await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
    }
    
    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task PublicTimelineNextAndPreviousPage()
    {
        await _page!.GotoAsync($"{AppUrl}");
        
        // If there is a next page button
        if (await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).CountAsync() > 0)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "<", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
        }
    }

    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task PublicTimelineFirstAndLastPage()
    {
        await _page!.GotoAsync($"{AppUrl}");
        
        // If there is a next page button
        if (await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).CountAsync() > 0)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = ">>", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "<<", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
        }
    }
    
    //---------------------------------- USER TIMELINE TESTS ----------------------------------
    [Test]
    [Category("End2End")]
    public async Task DoesUserTimelinePageSuccessfullyLoad()
    {
        // Go to Adrian's page
        await _page!.GotoAsync($"{AppUrl}/Adrian");
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Adrian's Timeline" })).ToBeVisibleAsync();
    }
    
    // Verify that clicking on a user goes to their timeline
    [Test]
    [Category("End2End")]
    public async Task GoToUserTimelineFromUsername()
    {
        await _page!.GotoAsync($"{AppUrl}");
        
        var firstMessageLink = _page.Locator("#messagelist > li:first-child a");

        var name = await firstMessageLink.InnerTextAsync();
        
        await firstMessageLink.ClickAsync();
        
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = $"{name}'s Timeline" })).ToBeVisibleAsync();
        
    }
    
    // Check for presence of cheeps for some author
    [Test]
    [Category("End2End")]
    public async Task PresenceOfCheeps()
    {
        // Go to Adrian's page
        await _page!.GotoAsync($"{AppUrl}/Adrian");
        await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
    }

    // Check for no cheeps on user timeline with no cheeps
    [Test]
    [Category("End2End")]
    public async Task NoCheepsOnUserTimeline()
    {
        // Go to a user page with no cheeps
        await _page!.GotoAsync($"{AppUrl}/UserWithNoCheeps");
        await Expect(_page.GetByText("There are no cheeps so far.")).ToBeVisibleAsync();
    }
    
    // Check back button goes to public timeline
    [Test]
    [Category("End2End")]
    public async Task BackButtonGoesToPublicTimeline()
    {
        // Go to Adrian's page
        await _page!.GotoAsync($"{AppUrl}/Adrian");
        
        // Click on the back button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Back" }).ClickAsync();
        
        // Check if the public timeline is visible
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" })).ToBeVisibleAsync();
    }
    
    // Check next and previous buttons on user timeline
    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task UserTimelineNextAndPreviousPage()
    {
        await _page!.GotoAsync($"{AppUrl}/Jacqualine%20Gilcoine");
        
        // If there is a next page button
        if (await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).CountAsync() > 0)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "<", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
        }
    }
    
    // Check first and last page buttons on user timeline
    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task UserTimelineFirstAndLastPage()
    {
        await _page!.GotoAsync($"{AppUrl}/Jacqualine%20Gilcoine");
        
        // If there is a next page button
        if (await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).CountAsync() > 0)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = ">>", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "<<", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
        }
    }
    
    //---------------------------------- REGISTER PAGE TESTS ----------------------------------
    
    // Registration page loads successfully (Expect the registration form)
    [Test]
    [Category("End2End")]
    public async Task RegisterPageLoads()
    {
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Register");
        
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Create a new account." })).ToBeVisibleAsync();
        await Expect(_page.Locator("#registerForm div").Filter(new() { HasText = "Email" })).ToBeVisibleAsync();
        await Expect(_page.Locator("#registerForm div").Nth(1)).ToBeVisibleAsync();
        await Expect(_page.Locator("#registerForm div").Filter(new() { HasText = "Confirm Password" })).ToBeVisibleAsync();
    }
    
    // Successfully registration with valid inputs
    
    // Registration without @ in email
    [Test]
    [Category("End2End")]
    public async Task RegisterWithoutAtInEmail()
    {
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Register");
        await _page.GetByPlaceholder("name@example.com").FillAsync("emailwithoutat");
        await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("MyBadAccount");
        await _page.GetByLabel("Confirm Password").FillAsync("MyBadAccount");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        // Check for validation message
        var emailInput = _page.GetByPlaceholder("name@example.com");
        var validationMessage = await emailInput.EvaluateAsync<string>("el => el.validationMessage");
        validationMessage.Should().Be("Mailadressen skal indeholde et \"@\". \"emailwithoutat\" mangler et \"@\".");
    }

    // Registration with password not living up to constraint (at least one non alphanumeric character)
    [Test]
    [Category("End2End")]
    public async Task RegisterWithNoAlphanumericCharacter()
    {
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Register");
        await _page.GetByPlaceholder("name@example.com").FillAsync("my@mail.com");
        await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("BadPassword1234");
        await _page.GetByLabel("Confirm Password").FillAsync("BadPassword1234");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(_page.GetByText("Passwords must have at least one non alphanumeric character.")).ToBeVisibleAsync();
    }

    // Registration with password not living up to constraints (at least one digit ('0'-'9'))
    [Test]
    [Category("End2End")]
    public async Task RegisterWithNoDigit()
    {
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Register");
        await _page.GetByPlaceholder("name@example.com").FillAsync("my@mail.com");
        await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("BadPassword!");
        await _page.GetByLabel("Confirm Password").FillAsync("BadPassword!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(_page.GetByText("Passwords must have at least one digit ('0'-'9').")).ToBeVisibleAsync();
    }
    
    // Registration with password not living up to constraints (at least one uppercase ('A'-'Z'))
    [Test]
    [Category("End2End")]
    public async Task RegisterWithNoUppercase()
    {
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Register");
        await _page.GetByPlaceholder("name@example.com").FillAsync("my@mail.com");
        await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("badpassword1234!");
        await _page.GetByLabel("Confirm Password").FillAsync("badpassword1234!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(_page.GetByText("Passwords must have at least one uppercase ('A'-'Z').")).ToBeVisibleAsync();
    }
    
    // This test should be reworked to only test register
    [Test]
    [Category("End2End")]
    public async Task RegisterTest()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        await _page.GotoAsync($"{AppUrl}");
        
        // Clicks on register button
        await _page.GetByRole(AriaRole.Link, new() { Name = "register" }).ClickAsync();
        
        // Arrived at register page, and put in email and password
        await _page.GetByPlaceholder("name@example.com").ClickAsync();
        await _page.GetByPlaceholder("name@example.com").FillAsync("testuser@gmail.com");
        await _page.GetByLabel("Password", new() { Exact = true }).ClickAsync();
        await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("Test@12345");
        await _page.GetByLabel("Confirm Password").ClickAsync();
        await _page.GetByLabel("Confirm Password").FillAsync("Test@12345");
        
        // Clicks on the register button to register the account
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        
        // Confirms the registration by clicking on the confirm button
        await _page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" }).ClickAsync();
        
        // Person has correctly registered if email is confirmed
        await Expect(_page.GetByText("Thank you for confirming your")).ToBeVisibleAsync();
        
        // Goes to login page
        await _page.GetByRole(AriaRole.Link, new() { Name = "login" }).ClickAsync();
        
        // Fills in information
        await _page.GetByPlaceholder("name@example.com").ClickAsync();
        await _page.GetByPlaceholder("name@example.com").FillAsync("testuser@gmail.com");
        await _page.GetByPlaceholder("password").ClickAsync();
        await _page.GetByPlaceholder("password").FillAsync("Test@12345");
        
        // Clicks on log in button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        
        // User arrived at the homepage and should now see a logout button with their email attached
        await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "Logout [testuser@gmail.com" })).ToBeVisibleAsync();
        
        // Removing the test user
        await _page.GetByRole(AriaRole.Link, new() { Name = "Manage account" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await _page.GetByPlaceholder("Please enter your password.").ClickAsync();
        await _page.GetByPlaceholder("Please enter your password.").FillAsync("Test@12345");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Delete data and close my" }).ClickAsync();

    }
    
    //---------------------------------- LOGIN PAGE TESTS ----------------------------------
    
    // Login page loads successfully (check for login form)
    
    // Test successfully login
    
    // Test link to own user timeline
    
    // Login with invalid credentials 
    
    // Check 'register as a new user' redirects to registration page.
    
    //---------------------------------- LOGOUT PAGE TESTS ----------------------------------
    
    // Logout page load successfully (check for logout button)
    
    // The logout button logs user out (check for no authentication and redirect)
    
    //---------------------------------- MANAGE ACCOUNT TESTS ----------------------------------
    
    // Manage page loads successfully
}