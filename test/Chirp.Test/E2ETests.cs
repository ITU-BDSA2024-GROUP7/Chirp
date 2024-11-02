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
    private const string StartupProjectPath = "C:/Users/nik/OneDrive/Documents/GitHub/Chirp/src/Chirp.Web/Chirp.Web.csproj"; // Update this path
    private Process? _appProcess;
    private IBrowser? browser;
    private IBrowserContext? context;
    private IPage? page;

    BrowserTypeLaunchOptions browserTypeLaunchOptions = new BrowserTypeLaunchOptions
    {
        Headless = false,
    };
    
    // Can be used in a test to save the cookies (so that a user doesn't have to login in a test for example)
    BrowserNewContextOptions browserNewContextOptions = new BrowserNewContextOptions
    {
        IgnoreHTTPSErrors = true,
        StorageStatePath = "state.json"
    };

    [SetUp]
    public async Task Setup()
    {
        browser = await Playwright.Chromium.LaunchAsync(browserTypeLaunchOptions);

        context = await browser.NewContextAsync();

        page = await context.NewPageAsync();
        
        await page.GotoAsync(AppUrl);
    }
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        
        // Start the ASP.NET application
        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{StartupProjectPath}\"",
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
    public async Task AuthenticatedUserCanCreateCheepFromPublicAndPrivateTimeline()
    {
        await page.GotoAsync(AppUrl);

        await page.Locator("#CheepMessage").ClickAsync();

        await page.Locator("#CheepMessage").FillAsync("Testing public timeline");

        await page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await Expect(page.Locator("#messagelist")).ToContainTextAsync("Testing public timeline");

        await page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();

        await page.Locator("#CheepMessage").ClickAsync();

        await page.Locator("#CheepMessage").FillAsync("Testing private timeline");

        await page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await Expect(page.Locator("#messagelist")).ToContainTextAsync("Testing private timeline");
    }
    
    [Test]
    [Category("End2End")]
    public async Task GetIndexPageAndCorrectContent()
    {
        if (Page == null) throw new InvalidOperationException("Page is not initialized");

        await Page.GotoAsync($"{AppUrl}/");
        
        var publicTimelineHeader = page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" });
        var headerText = await publicTimelineHeader.InnerTextAsync();
        headerText.Should().Be("Public Timeline");
        await Expect(publicTimelineHeader).ToBeVisibleAsync();
    }
}