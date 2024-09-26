using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;

namespace Chirp.Razor;

public class DBFacade
{
    private readonly string connectionString;
    private readonly IFileProvider embeddedProvider;
    
    public DBFacade(string sqlDBFilePath)
    {
        Console.WriteLine($"DBFacade constructor called with path: {sqlDBFilePath}");
        
        // string sqlDBFilePath = Path.Combine("tmp", "chirp.db");
        connectionString = $"Data Source={sqlDBFilePath}";
        embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

        // Ensure that the directory exists
        string directory = Path.GetDirectoryName(sqlDBFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Ensure that the file exists
        if (!File.Exists(sqlDBFilePath))
        {
            // Create empty db file at filepath
            //using (var writer = new StreamWriter(sqlDBFilePath, false));
            Console.WriteLine($"Creating database: {sqlDBFilePath}");
            using (var stream = File.Create(sqlDBFilePath));
        }
        
        // Populate database
        PopulateDatabase(); 
    }

    private void PopulateDatabase()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        
        var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        Console.WriteLine("Here are the embedded resources: ");
        foreach (var name in resourceNames)
        {
            Console.WriteLine(name);
        }

        Console.WriteLine("Attempting to read schema and dump using: Chirp.Razor.schema.sql and Chirp.Razor.dump.sql");
        var schema = ReadEmbeddedSqlFile("Chirp.Razor.schema.sql");
        var dump = ReadEmbeddedSqlFile("Chirp.Razor.dump.sql");
        
        Console.WriteLine("Schema SQL:");
        Console.WriteLine(schema);
        Console.WriteLine("Dump SQL:");
        Console.WriteLine(dump);
        
        ExecuteCommand(connection, schema);
        ExecuteCommand(connection, dump);
    }

    private string ReadEmbeddedSqlFile(string fileName)
    {
        Console.WriteLine("Reading embedded sql file with path " + fileName);
        using var embedded = embeddedProvider.GetFileInfo(fileName).CreateReadStream();
        using var reader = new StreamReader(embedded);
        return reader.ReadToEnd();
    }
    
    private void ExecuteCommand(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
    
    
    //metode retrive entire list
    public List<CheepViewModel> RetriveAllCheeps()
    {
        var cheeps = new List<CheepViewModel>();
        
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        //creates query
        using var command = connection.CreateCommand();
        var query = "SELECT u.username,m.text,m.pub_date FROM message m JOIN user u ON u.user_id = m.author_id;";
        command.CommandText = query;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            cheeps.Add(new CheepViewModel(reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        }
        //Returns a list of cheeps
        return cheeps;
    }
    
    
    //retrive chirps from author
    public List<CheepViewModel> RetriveCheepFromAuthor(string author)
    {
        var cheeps = new List<CheepViewModel>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        //creates query
        using var command = connection.CreateCommand();
        var query = @$"SELECT u.username, m.text, m.pub_date 
                        FROM message m 
                        JOIN user u ON u.user_id = m.author_id 
                        WHERE u.username=@Author;";
            // test query SELECT u.username,m.text,m.pub_date FROM message m JOIN user u ON u.user_id = m.author_id WHERE u.username = 'Helge';
        command.Parameters.AddWithValue("@Author", author);
        command.CommandText = query;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            cheeps.Add(new CheepViewModel(reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        }
        //Returns a list of all cheeps from a certain author
        return cheeps;
    }
}
    



