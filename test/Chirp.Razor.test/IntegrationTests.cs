using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Chirp.Razor;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Chirp.Razor.test;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _connection;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
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

        _client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CheepDBContext>();
            dbContext.Database.EnsureCreated();
            //DbInitializer.SeedDatabase(dbContext);
        }
    }

    public void Dispose()
    {
        _connection.Close();
    }

    [Fact]
    public async Task GetCheepsReturnsCheeps()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CheepDBContext>();
            dbContext.Database.EnsureDeleted(); // Clear previous data
            dbContext.Database.EnsureCreated(); // Recreate the database

            var author = new Author() { AuthorId = 1, Cheeps = null, Email = "mymail", Name = "testPerson" };
        
            var cheep = new Cheep
            {
                CheepId = 1,
                Author = author,
                AuthorId = author.AuthorId,
                Text = "Hello! I hope this goes through",
                TimeStamp = DateTimeOffset.FromUnixTimeSeconds(1728497189).UtcDateTime // Ensure this matches the format in your model
            };

            dbContext.Authors.Add(author);
            await dbContext.SaveChangesAsync();

            dbContext.Cheeps.Add(cheep);
            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/cheeps");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();
        cheeps.Should().NotBeNull();
        cheeps.Should().ContainSingle(c => c.Text == "Hello! I hope this goes through");
    }
}