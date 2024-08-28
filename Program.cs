
if (args[0] == "read")
{
    try
    {
        using (StreamReader sr = new StreamReader("chirp_cli_db.csv"))
        {
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                Console.WriteLine(line);
                Console.WriteLine("break");
            }
        }
        
    }
    catch (Exception e){

        Console.WriteLine("Error");
    }
}