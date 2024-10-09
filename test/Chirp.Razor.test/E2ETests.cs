using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Chirp.Razor;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.test;

public class E2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public E2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetIndexPageAndCorrectContent()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<CheepDBContext>().UseSqlite(connection);
        
        using var context = new CheepDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync();
        
        ICheepRepository repository = new CheepRepository(context);
        
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
    public async Task DoesPublicTimelineContainHelgeTest()
    {
        var client = _factory.CreateClient();
    
        var response = await client.GetAsync("/");
    
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
    
        // Check for the actual content on the page
        content.Should().Contain("Helge");
        content.Should().Contain("Hello, BDSA students!");
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

}