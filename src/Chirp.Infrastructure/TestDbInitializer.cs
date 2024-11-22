using Chirp.Core;
using Chirp.Infrastructure.Repositories;

public static class TestDbInitializer
{
    public static void SeedDatabase(CheepDBContext chirpContext)
    {
        if (!(chirpContext.Authors.Any() && chirpContext.Cheeps.Any()))
        {
            var a1 = new Author() { AuthorId = 1, Name = "Tony Stark", Email = "Tony.Stark@avengers.com", Cheeps = new List<Cheep>(), AuthorsFollowed = new List<string>() };
            var a2 = new Author() { AuthorId = 2, Name = "Natasha Romanoff", Email = "Natasha.Romanoff@shield.gov", Cheeps = new List<Cheep>(), AuthorsFollowed = new List<string>() };
            var a3 = new Author() { AuthorId = 3, Name = "Peter Parker", Email = "Peter.Parker@dailybugle.com", Cheeps = new List<Cheep>(), AuthorsFollowed = new List<string>() };
            var a4 = new Author() { AuthorId = 4, Name = "Steve Rogers", Email = "Steve.Rogers@avengers.com", Cheeps = new List<Cheep>(), AuthorsFollowed = new List<string>() };
            var a5 = new Author() { AuthorId = 5, Name = "Wanda Maximoff", Email = "Wanda.Maximoff@sokovia.net", Cheeps = new List<Cheep>(), AuthorsFollowed = new List<string>() };
            var authors = new List<Author>() { a1, a2, a3, a4, a5 };

            var c1 = new Cheep() { CheepId = 1, AuthorId = a1.AuthorId, Author = a1, Text = "Building suits is easy. Wearing them during summer? Not so much.", TimeStamp = DateTime.Parse("2023-08-01 10:00:00") };
            var c2 = new Cheep() { CheepId = 2, AuthorId = a1.AuthorId, Author = a1, Text = "Just solved world peace. You're welcome.", TimeStamp = DateTime.Parse("2023-08-02 11:15:00") };
            var c3 = new Cheep() { CheepId = 3, AuthorId = a1.AuthorId, Author = a1, Text = "Friday says I'm witty. Thanks, Friday.", TimeStamp = DateTime.Parse("2023-08-03 12:20:00") };
            var c4 = new Cheep() { CheepId = 4, AuthorId = a1.AuthorId, Author = a1, Text = "Pepper told me to log off. So here I am.", TimeStamp = DateTime.Parse("2023-08-04 13:25:00") };
            var c5 = new Cheep() { CheepId = 5, AuthorId = a1.AuthorId, Author = a1, Text = "Did someone say shawarma? Let's assemble.", TimeStamp = DateTime.Parse("2023-08-05 14:30:00") };

            var c6 = new Cheep() { CheepId = 6, AuthorId = a2.AuthorId, Author = a2, Text = "Stealth missions are easy. Avoiding people afterward? Not so much.", TimeStamp = DateTime.Parse("2023-08-06 09:10:00") };
            var c7 = new Cheep() { CheepId = 7, AuthorId = a2.AuthorId, Author = a2, Text = "Trust no one. Except me, obviously.", TimeStamp = DateTime.Parse("2023-08-07 10:15:00") };
            var c8 = new Cheep() { CheepId = 8, AuthorId = a2.AuthorId, Author = a2, Text = "Spiders are everywhere. You're welcome, Peter.", TimeStamp = DateTime.Parse("2023-08-08 11:20:00") };
            var c9 = new Cheep() { CheepId = 9, AuthorId = a2.AuthorId, Author = a2, Text = "Stuck in traffic. I miss the helicarrier.", TimeStamp = DateTime.Parse("2023-08-09 12:25:00") };
            var c10 = new Cheep() { CheepId = 10, AuthorId = a2.AuthorId, Author = a2, Text = "Got a red wig tip? DM me.", TimeStamp = DateTime.Parse("2023-08-10 13:30:00") };

            var c11 = new Cheep() { CheepId = 11, AuthorId = a3.AuthorId, Author = a3, Text = "My spider-sense is tingling... someone just liked my post!", TimeStamp = DateTime.Parse("2023-08-11 08:05:00") };
            var c12 = new Cheep() { CheepId = 12, AuthorId = a3.AuthorId, Author = a3, Text = "J. Jonah Jameson is at it again. #FakeNews", TimeStamp = DateTime.Parse("2023-08-12 09:10:00") };
            var c13 = new Cheep() { CheepId = 13, AuthorId = a3.AuthorId, Author = a3, Text = "Saving New York is exhausting. Pizza anyone?", TimeStamp = DateTime.Parse("2023-08-13 10:15:00") };
            var c14 = new Cheep() { CheepId = 14, AuthorId = a3.AuthorId, Author = a3, Text = "Friendly neighborhood Spider-Man reporting for duty!", TimeStamp = DateTime.Parse("2023-08-14 11:20:00") };
            var c15 = new Cheep() { CheepId = 15, AuthorId = a3.AuthorId, Author = a3, Text = "Web shooters: reloaded. Villains: beware.", TimeStamp = DateTime.Parse("2023-08-15 12:25:00") };

            var c16 = new Cheep() { CheepId = 16, AuthorId = a4.AuthorId, Author = a4, Text = "I could do this all day.", TimeStamp = DateTime.Parse("2023-08-16 08:00:00") };
            var c17 = new Cheep() { CheepId = 17, AuthorId = a4.AuthorId, Author = a4, Text = "The future is bright, but I'm stuck in the 40s playlist.", TimeStamp = DateTime.Parse("2023-08-17 09:15:00") };
            var c18 = new Cheep() { CheepId = 18, AuthorId = a4.AuthorId, Author = a4, Text = "Shield maintenance is no joke. Ask Tony.", TimeStamp = DateTime.Parse("2023-08-18 10:20:00") };
            var c19 = new Cheep() { CheepId = 19, AuthorId = a4.AuthorId, Author = a4, Text = "Woke up feeling patriotic today. #StarsAndStripes", TimeStamp = DateTime.Parse("2023-08-19 11:25:00") };
            var c20 = new Cheep() { CheepId = 20, AuthorId = a4.AuthorId, Author = a4, Text = "Falcon keeps stealing my lines. Classic.", TimeStamp = DateTime.Parse("2023-08-20 12:30:00") };

            var c21 = new Cheep() { CheepId = 21, AuthorId = a5.AuthorId, Author = a5, Text = "Reality can be whatever I want it to be.", TimeStamp = DateTime.Parse("2023-08-21 13:35:00") };
            var c22 = new Cheep() { CheepId = 22, AuthorId = a5.AuthorId, Author = a5, Text = "Chaos magic is not for everyone. Trust me.", TimeStamp = DateTime.Parse("2023-08-22 14:40:00") };
            var c23 = new Cheep() { CheepId = 23, AuthorId = a5.AuthorId, Author = a5, Text = "Every now and then, I miss Sokovia. But not Ultron.", TimeStamp = DateTime.Parse("2023-08-23 15:45:00") };
            var c24 = new Cheep() { CheepId = 24, AuthorId = a5.AuthorId, Author = a5, Text = "Hexes are fun until someone gets hurt. Or is it?", TimeStamp = DateTime.Parse("2023-08-24 16:50:00") };
            var c25 = new Cheep() { CheepId = 25, AuthorId = a5.AuthorId, Author = a5, Text = "Vision had better not touch my leftovers. Again.", TimeStamp = DateTime.Parse("2023-08-25 17:55:00") };

            var c26 = new Cheep() { CheepId = 26, AuthorId = a1.AuthorId, Author = a1, Text = "Is it possible to patent a superhero name? Asking for a friend.", TimeStamp = DateTime.Parse("2023-08-26 18:00:00") };
            var c27 = new Cheep() { CheepId = 27, AuthorId = a2.AuthorId, Author = a2, Text = "Espionage isn't as glamorous as the movies make it seem.", TimeStamp = DateTime.Parse("2023-08-27 19:00:00") };
            var c28 = new Cheep() { CheepId = 28, AuthorId = a3.AuthorId, Author = a3, Text = "Anyone else think Doc Ock looks like a squid?", TimeStamp = DateTime.Parse("2023-08-28 20:00:00") };
            var c29 = new Cheep() { CheepId = 29, AuthorId = a4.AuthorId, Author = a4, Text = "Freedom isn’t free. But the coffee at Avengers Tower is.", TimeStamp = DateTime.Parse("2023-08-29 21:00:00") };
            var c30 = new Cheep() { CheepId = 30, AuthorId = a5.AuthorId, Author = a5, Text = "Magic doesn't solve everything. But it sure helps with dishes.", TimeStamp = DateTime.Parse("2023-08-30 22:00:00") };

            var c31 = new Cheep() { CheepId = 31, AuthorId = a1.AuthorId, Author = a1, Text = "Why does Thor always drink out of a giant mug? Showoff.", TimeStamp = DateTime.Parse("2023-08-31 23:00:00") };
            var c32 = new Cheep() { CheepId = 32, AuthorId = a2.AuthorId, Author = a2, Text = "I've lost count of how many times I've saved the world. It's starting to get old.", TimeStamp = DateTime.Parse("2023-09-01 08:30:00") };
            var c33 = new Cheep() { CheepId = 33, AuthorId = a3.AuthorId, Author = a3, Text = "I’ve fought aliens, robots, and my own clone. But nothing’s tougher than high school.", TimeStamp = DateTime.Parse("2023-09-02 09:00:00") };
            var c34 = new Cheep() { CheepId = 34, AuthorId = a4.AuthorId, Author = a4, Text = "Every time someone calls me 'Captain', I stand a little taller.", TimeStamp = DateTime.Parse("2023-09-03 10:00:00") };
            var c35 = new Cheep() { CheepId = 35, AuthorId = a5.AuthorId, Author = a5, Text = "Magic doesn't solve everything. But it sure helps with dishes.", TimeStamp = DateTime.Parse("2023-09-04 11:00:00") };
            var c36 = new Cheep() { CheepId = 36, AuthorId = a1.AuthorId, Author = a1, Text = "Jarvis says my jokes are bad. Jarvis is wrong.", TimeStamp = DateTime.Parse("2023-09-05 12:00:00") };


            var cheeps = new List<Cheep>()
            {
                c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16, c17, c18, c19, c20, c21, c22,
                c23, c24, c25, c26, c27, c28, c29, c30, c31, c32, c33, c34, c35, c36
            };
            a1.Cheeps = new List<Cheep>() { c1, c2, c3, c4, c5, c26, c31, c36 };
            a2.Cheeps = new List<Cheep>() { c6, c7, c8, c9, c10 , c27, c32 };
            a3.Cheeps = new List<Cheep>() { c11, c12, c13, c14, c15, c28, c33 };
            a4.Cheeps = new List<Cheep>() { c16, c17, c18, c19, c20, c29, c34 };
            a5.Cheeps = new List<Cheep>() { c21, c22, c23, c24, c25, c30, c35 };
            
            chirpContext.Authors.AddRange(authors);
            chirpContext.Cheeps.AddRange(cheeps);
            chirpContext.SaveChanges();
        }
    }
}