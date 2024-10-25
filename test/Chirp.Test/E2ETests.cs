using System.Net;
using System.Text.RegularExpressions;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Chirp.Web;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chirp.Test;

public class E2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _connection;

    public E2ETests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Used to remove if a context of the database exist (but only for these tests specifically in memory)
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
    }

    [Fact]
    public async Task GetIndexPageAndCorrectContent()
    {
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync("/");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Chirp!");
    }

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
    
    [Fact]
    public async Task LoginUserTest()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true 
        });
        
        
        
        var getResponse = await client.GetAsync("/Identity/Account/Register");
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var testEmail = "testuser@gmail.com";
        var testPassword = "Test@12345";
        
        // Step 2: Parse the anti-forgery token from the page content
        var tokenValue = ExtractAntiForgeryToken(getContent);
        
        // create user
        var registerData = new Dictionary<string, string>
        {
            {"Input.Email", testEmail},
            {"Input.Password", testPassword}, 
            {"Input.ConfirmPassword",testPassword},
            { "__RequestVerificationToken", tokenValue},
            {"returnUrl","/"}
        };
        await client.PostAsync("/Identity/Account/Register", new FormUrlEncodedContent(registerData));
        
        var loginPage = await client.GetAsync("/Identity/Account/Login");
        var loginToken = ExtractAntiForgKeyLogin(await loginPage.Content.ReadAsStringAsync());
        
        var loginData = new Dictionary<string, string>()
        {
            { "Input.Email", testEmail },
            { "Input.Password", testPassword },
            { "__RequestVerificationToken", tokenValue },
            { "Input.RememberMe", "false"},
            { "returnUrl", "/" }
        };
        var confirmationLink = $"/Identity/Account/RegisterConfirmation?email={testEmail}";
        var confirmationResponse = await client.GetAsync(confirmationLink);
        
        
        // Act
        var homePage = await client.GetAsync("/");
        homePage.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await homePage.Content.ReadAsStringAsync();
        // Assert
        
        
        content.Should().Contain($"logout [{testEmail}]");
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

    private string ExtractAntiForgKeyLogin(string content)
    {
        var match = Regex.Match(content, @"\<input\s+name=""__RequestVerificationToken""\s+type=""hidden""\s+value=""([^""]+)""");
        return match.Success ? match.Groups[1].Value : throw new Exception("Anti-forgery token not found");
    }
}