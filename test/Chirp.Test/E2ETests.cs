using System.Net;
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
}