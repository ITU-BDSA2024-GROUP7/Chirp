
if (args[0] == "read")
{
    try
    {
        using (StreamReader sr = new StreamReader("chirp_cli_db.csv"))
        {
            string line;
            sr.ReadLine();
            while ((line = sr.ReadLine()) != null)
            {
                string[] status = line.Split('"');
                status[0] = status[0].Trim(',');
                status[2] = status[2].Trim(',');
                Console.WriteLine(status[0] + " @ " + status[2] + " " + status[1]);
            }
        }
        
    }
    catch (Exception e){
        Console.WriteLine("Error");
    }
}

if (args[0] == "cheep")
{
    string message = args[1];
    string author = Environment.MachineName;
    string date = DateTime.Now.ToString("yyyyMMdd");
    
    string csv = author + ",\"" + message + "\"," + date;
    Console.WriteLine(csv);
    StreamWriter sw = new StreamWriter("chirp_cli_db.csv");
    sw.WriteLine(csv);
}