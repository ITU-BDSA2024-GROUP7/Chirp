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

public class E2ETests : PageTest
{
    private WebApplicationFactory<Program> _factory;
    private SqliteConnection _connection;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _browserContext;
    private IPage? _page;
    private SqliteConnection _sqliteConnection;
    private string _serverUrl = string.Empty;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        // Setup SQLite connection
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Setup WebApplicationFactory
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<CheepDBContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<CheepDBContext>(options =>
                    {
                        options.UseSqlite(_connection);
                    });
                });
            });

        // Start the server and get the URL
        var client = _factory.CreateClient();
        _serverUrl = "http://localhost:5273";

        // Setup Playwright
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // Set to false for debugging
        });
    }

    [SetUp]
    public async Task SetUpContext()
    {
        if (_browser == null)
        {
            throw new InvalidOperationException("Browser is not initialized");
        }

        _browserContext = await _browser.NewContextAsync();
        _page = await _browser.NewPageAsync();
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (_browserContext != null)
        {
            await _browser.CloseAsync();
        }

        if (_page != null)
        {
            await _page.CloseAsync();
        }
    }

    [OneTimeTearDown]
    public async Task TearDownPlaywright()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }

        if (_playwright != null)
        {
            _playwright.Dispose();
        }

        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }

        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }
    }

    [Test]
    public async Task GetIndexPageAndCorrectContent()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");

        await _page.GotoAsync($"{_serverUrl}/");

        var content = await _page.TextContentAsync("body");
        NUnit.Framework.Assert.That(content, Does.Contain("Chirp"));
    }

    

    [Test]
    public async Task Get_TestPersonPage()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        await _page.GotoAsync($"{_serverUrl}/Testperson");
        
        var content = await _page.TextContentAsync("body");
        NUnit.Framework.Assert.That(content, Does.Contain(""));
    }
    
    [Test]
    public async Task DoesPrivateTimelineContainAdrianTest()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        // Go to Adrian's page
        await _page.GotoAsync($"{_serverUrl}/Adrian");
        
        // Look if Adrian has the chosen cheep
        var content = await _page.TextContentAsync("body");
        NUnit.Framework.Assert.That(content, Does.Contain("Adrian"));
        NUnit.Framework.Assert.That(content, Does.Contain("Hej, velkommen til kurset"));
    }
    

    [Test]
    public async Task RegisterNewUserTest()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");

        await _page.GotoAsync($"{_serverUrl}/Identity/Account/Register");
        
        // Fill in the data 
        await _page.FillAsync("input[name='Input.Email']", "testuser@gmail.com");
        await _page.FillAsync("input[name='Input.Password']", "Test@12345");
        await _page.FillAsync("input[name='Input.ConfirmPassword']", "Test@12345");
        
        // Submitting the form
        await _page.ClickAsync("button[type='submit']");
        
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var content = await _page.TextContentAsync("body");
        NUnit.Framework.Assert.That(content, Does.Contain("Register"));
    }
    
}