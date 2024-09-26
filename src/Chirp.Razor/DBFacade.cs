using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;

namespace Chirp.Razor;

public class DBFacade
{
    private readonly string connectionString;
    private readonly IFileProvider embeddedProvider;

    public DBFacade()
    {
        string sqlDBFilePath = Path.Combine("tmp", "chirp.db");
        
        // Get CHIRPDB Environment Variable
        string chirpDbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");

        // Check if CHIRPDBPATH is set.
        if (string.IsNullOrEmpty(chirpDbPath))
        {
            string tempDir = Path.GetTempPath();
            chirpDbPath = Path.Combine(tempDir, "mychirp.db");
            Environment.SetEnvironmentVariable("CHIRPDBPATH", chirpDbPath,EnvironmentVariableTarget.Machine);
            
        }
        
        
        connectionString = $"Data Source={chirpDbPath}";
        embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        string directory = Path.GetDirectoryName(connectionString);
        // Ensure that the directory exists
        if (!Directory.Exists(directory))
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Ensure that the file exists
        if (!File.Exists(sqlDBFilePath))
        {
            // Create empty db file at filepath
            using (var writer = new StreamWriter(sqlDBFilePath, false));
        }
        
        // Populate database
        PopulateDatabase();
    }

    private void PopulateDatabase()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var schema = ReadEmbeddedSqlFile("schema.sql");
        var dump = ReadEmbeddedSqlFile("dump.sql");
        
        ExecuteCommand(connection, schema);
        ExecuteCommand(connection, dump);
    }

    private string ReadEmbeddedSqlFile(string fileName)
    {
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
        command.Parameters.AddWithValue("@Author", author);
        var query = @$"SELECT u.username, m.text, m.pub_date 
                        FROM message m 
                        JOIN user u ON u.user_id = m.author_id 
                        WHERE u.username=@Author;";
            // test query SELECT u.username,m.text,m.pub_date FROM message m JOIN user u ON u.user_id = m.author_id WHERE u.username = 'Helge';
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
    



