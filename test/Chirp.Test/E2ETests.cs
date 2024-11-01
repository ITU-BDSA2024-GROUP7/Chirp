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
    private const string StartupProjectPath = "F:/Udvikling/CSharp/Chirp/src/Chirp.Web/Chirp.Web.csproj"; // Update this path
    private Process? _appProcess;
    
    bool isSetup = false;

    BrowserTypeLaunchOptions browserTypeLaunchOptions = new BrowserTypeLaunchOptions
    {
        Headless = false,
    };

    BrowserNewContextOptions browserNewContextOptions = new BrowserNewContextOptions
    {
        IgnoreHTTPSErrors = true,
        StorageStatePath = "state.json"
    };
    
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
        await Task.Delay(5000); // Adjust the delay if needed, or implement a better wait mechanism
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
    
    [SetUp]
    public async Task SetUp()
    {
        if (isSetup) return;

        await using var browser = await Playwright.Chromium.LaunchAsync(browserTypeLaunchOptions);

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
        });

        var page = await context.NewPageAsync();

        await page.GotoAsync("http://localhost:5273/");

        isSetup = true;
    }

    // [Test]
    // [Category("End2End")]
    // public async Task AuthenticatedUserCanCreateCheepFromPublicAndPrivateTimeline()
    // {
    //     await using var browser = await Playwright.Chromium.LaunchAsync(browserTypeLaunchOptions);
    //
    //     var context = await browser.NewContextAsync(browserNewContextOptions);
    //
    //     var page = await context.NewPageAsync();
    //
    //     await page.GotoAsync("http://localhost:5273/");
    //
    //     await page.Locator("#CheepMessage").ClickAsync();
    //
    //     await page.Locator("#CheepMessage").FillAsync("Testing public timeline");
    //
    //     await page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
    //
    //     await Expect(page.Locator("#messagelist")).ToContainTextAsync("Testing public timeline");
    //
    //     await page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
    //
    //     await page.Locator("#CheepMessage").ClickAsync();
    //
    //     await page.Locator("#CheepMessage").FillAsync("Testing private timeline");
    //
    //     await page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
    //
    //     await Expect(page.Locator("#messagelist")).ToContainTextAsync("Testing private timeline");
    // }

    [Test]
    [Category("End2End")]
    public async Task publictimeline()
    {
        await using var browser = await Playwright.Chromium.LaunchAsync(browserTypeLaunchOptions);
        
        var context = await browser.NewContextAsync();

        var page = await context.NewPageAsync();
        await page.GotoAsync("http://localhost:5273/");
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" })).ToBeVisibleAsync();

    }
}