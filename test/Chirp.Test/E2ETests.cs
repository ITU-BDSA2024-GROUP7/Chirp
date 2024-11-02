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
        
        await _page.GotoAsync(AppUrl);
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
    
    [Test]
    [Category("End2End")]
    public async Task GetIndexPageAndCorrectContent()
    {
        
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        await Page.GotoAsync($"{AppUrl}/");
        
        var publicTimelineHeader = _page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" });
        var headerText = await publicTimelineHeader.InnerTextAsync();
        headerText.Should().Be("Public Timeline");
        await Expect(publicTimelineHeader).ToBeVisibleAsync();
    }
    
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

    [Test]
    [Category("End2End")]
    public async Task RegisterAndLoginTest()
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
        var logoutButton = _page.GetByRole(AriaRole.Link, new() { Name = "Logout [testuser@gmail.com]" });

        // Verify that the button contains the correct text
        var logoutButtonText = await logoutButton.InnerTextAsync();
        logoutButtonText.Should().Be("Logout [testuser@gmail.com]");
        
        // Removing the test user
        await _page.GetByRole(AriaRole.Link, new() { Name = "Manage account" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await _page.GetByPlaceholder("Please enter your password.").ClickAsync();
        await _page.GetByPlaceholder("Please enter your password.").FillAsync("Test@12345");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Delete data and close my" }).ClickAsync();

    }
}