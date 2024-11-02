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
    private const string AppUrl = "http://localhost:5273/";
    private string _startupProjectPath;
    private Process? _appProcess;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    readonly BrowserTypeLaunchOptions browserTypeLaunchOptions = new BrowserTypeLaunchOptions
    {
        Headless = true,
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
        await _page!.GotoAsync("http://localhost:5273/");
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" })).ToBeVisibleAsync();
    }

    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task PublicTimelineLoadingCheeps()
    {
        await _page!.GotoAsync("http://localhost:5273/");
        await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
    }
    
    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task PublicTimelineNextAndPreviousPage()
    {
        await _page!.GotoAsync("http://localhost:5273/");
        
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
        await _page!.GotoAsync("http://localhost:5273/");
        
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
    public async Task DoesPrivateTimelineContainAdrianTest()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        // Go to Adrian's page
        await _page.GotoAsync("http://localhost:5273/Adrian");
        
        var timelineHeader = _page.GetByRole(AriaRole.Heading, new() { Name = "Adrian's Timeline" });
        var headerText = await timelineHeader.InnerTextAsync();
        headerText.Should().Be("Adrian's Timeline");
        await Expect(timelineHeader).ToBeVisibleAsync();
    }
    
    // Verify that clicking on a user goes to their timeline
    
    // Check for presence of cheeps for some author
    
    // Check for no cheeps on user timeline with no cheeps
    
    // Check back button goes to public timeline 

    // Next and previous page 
    
    // First and last page
    
    //---------------------------------- REGISTER PAGE TESTS ----------------------------------
    
    // Registration page loads successfully (Expect the registration form)
    
    // Successfully registration with valid inputs
    
    // Registration without @ in email
    
    // Registration with password not living up to constraint (min 8 characters)
    
    // Registration with password not living up to constraints (a symbol)
    
    // Registration with password not living up to constraints (one big letter? cant remember if constraint exists)
    
    
    // This test should be reworked to only test register
    [Test]
    [Category("End2End")]
    public async Task RegisterTest()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        await _page.GotoAsync("http://localhost:5273/");
        
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
    
    // Login with invalid credentials 
    
    // Check 'register as a new user' redirects to registration page.
    
    //---------------------------------- LOGOUT PAGE TESTS ----------------------------------
    
    // Logout page load successfully (check for logout button)
    
    // The logout button logs user out (check for no authentication and redirect)
    
    //---------------------------------- MANAGE ACCOUNT TESTS ----------------------------------
    
    // Manage page loads successfully
}