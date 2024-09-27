using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Chirp.Razor;

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
    
        var response = await client.GetAsync("/Testperson?page=1");
    
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
    
        // Check for the actual content on the page
        content.Should().Contain("There are no cheeps so far.");
    }
}