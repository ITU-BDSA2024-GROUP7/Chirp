namespace Chirp.Test;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
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

        _client = _factory.CreateClient();
        // Ensures the database was created
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CheepDBContext>();
            dbContext.Database.EnsureCreated();
        }
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
                TimeStamp = DateTimeOffset.FromUnixTimeSeconds(1728497189).UtcDateTime
            };

            dbContext.Authors.Add(author);
            await dbContext.SaveChangesAsync();

            dbContext.Cheeps.Add(cheep);
            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/cheeps");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cheeps = await response.Content.ReadFromJsonAsync<List<Core.DTOs.CheepDTO>>();
        cheeps.Should().NotBeNull();
        cheeps.Should().ContainSingle(c => c.Text == "Hello! I hope this goes through");
    }
    
    [Fact]
    public async Task GetCheepsReturnsCheepsForSpecificAuthor()
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
                Text = "Test Message",
                TimeStamp = DateTimeOffset.FromUnixTimeSeconds(1728497189).UtcDateTime
            };

            dbContext.Authors.Add(author);
            await dbContext.SaveChangesAsync();

            dbContext.Cheeps.Add(cheep);
            await dbContext.SaveChangesAsync();
        }

        // Fetch all cheeps and see if author is amongst them
        
        var response = await _client.GetAsync("/cheeps");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    
        var cheeps = await response.Content.ReadFromJsonAsync<List<Core.DTOs.CheepDTO>>();
        cheeps.Should().NotBeNull();
    
        // Check whether a cheep with author was returned
        cheeps.Should().ContainSingle(c =>  c.Author.Name == "testPerson");
    }
    
    // Is called after all tests finish
    public void Dispose()
    {
        _connection?.Close();   // Closes the connection.
        _connection?.Dispose(); // Disposes the connection object.
    }
}