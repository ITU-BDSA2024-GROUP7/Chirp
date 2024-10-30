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
using Program = Chirp.Web.Program;

namespace Chirp.Test;

public class E2ETests : PageTest
{
    private Process _serverProcess;
    private string _baseUrl; 

    [SetUp]
    public async Task Init()
    {
        // Start the server
        _serverProcess = await MyEndToEndUtil.StartServer();

        // Set base URL
        _baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5273"; // Default URL if not set
        
    }

    [TearDown]
    public async Task Cleanup()
    {
        _serverProcess.Kill();
        _serverProcess.Dispose();
    }
    
    [Test]
    public async Task GetIndexPageAndCorrectContent()
    {
        NUnit.Framework.Assert.IsNotNull(Page, "Page instance is null.");
        await Page.GotoAsync(_baseUrl);

        var content = await Page.ContentAsync();
        content.Should().Contain("Chirp!");
    }
    
    
    
    /*
    [Fact]
    public async Task Get_TestPersonPage()
    {
        var client = _factory.CreateClient();
    
        var response = await client.GetAsync("/Testperson");
    
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
    
        // Check for the actual content on the page
        content.Should().Contain("There are no cheeps so far.");
    }
    
    [Fact]
    public async Task DoesPrivateTimelineContainAdrianTest()
    {
        var client = _factory.CreateClient();
    
        var response = await client.GetAsync("/Adrian");
    
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
    
        // Check for the actual content on the page
        content.Should().Contain("Adrian");
        content.Should().Contain("Hej, velkommen til kurset");
    }

    [Fact]
    public async Task RegisterNewUserTest()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var getResponse = await client.GetAsync("/Identity/Account/Register");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        

        // Step 2: Parse the anti-forgery token from the page content
        var tokenValue = ExtractAntiForgeryToken(getContent);
        
        var registerData = new Dictionary<string, string>
        {
            {"Input.Email","testuser@gmail.com"},
            {"Input.Password","Test@12345"}, 
            {"Input.ConfirmPassword","Test@12345"},
            { "__RequestVerificationToken", tokenValue},
            {"returnUrl","/"}
        };
        
        // Act
        var response = await client.PostAsync("/Identity/Account/Register", new FormUrlEncodedContent(registerData));
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Expecting HTTP 200
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Register confirmation"); 
    }
    
    // Helper method to extract anti forgery token
    private string ExtractAntiForgeryToken(string htmlContent)
    {
        // Updated regex pattern for finding the anti-forgery token value
        var match = Regex.Match(htmlContent, @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            throw new InvalidOperationException("Anti-forgery token not found");
        }
        return match.Groups[1].Value;
    }
    */
}