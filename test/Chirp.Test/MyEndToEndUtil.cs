using System.Diagnostics;

namespace Chirp.Test;

public static class MyEndToEndUtil
{
    public static async Task<Process> StartServer()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet", 
            Arguments = "run --project Chirp/src/Chirp.Web/Chirp.Web.csproj", // Run the web app
            RedirectStandardOutput = true, // Allows to read standard output
            RedirectStandardError = true, // Allows to read error messages
            UseShellExecute = false, // Used to make redirection work. Process is started from the executable and not through a shell
            CreateNoWindow = true // Prevents creation of a window
        };
        
        var process = new Process { StartInfo = startInfo };
        
        // Starts the server
        process.Start();
        
        // Optional if a delay is needed when starting the server
        await Task.Delay(2000);
        
        return process;
    }
}