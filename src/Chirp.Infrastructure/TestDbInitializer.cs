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

            var c37 = new Cheep() { CheepId = 37, AuthorId = a1.AuthorId, Author = a1, Text = "I'm pretty sure I could have solved all of science by now, but I’m still figuring out autocorrect.", TimeStamp = DateTime.Parse("2023-08-06 15:00:00") };
            var c38 = new Cheep() { CheepId = 38, AuthorId = a1.AuthorId, Author = a1, Text = "I designed a robot that can cook. It's already better than most of my attempts.", TimeStamp = DateTime.Parse("2023-08-07 16:10:00") };
            var c39 = new Cheep() { CheepId = 39, AuthorId = a1.AuthorId, Author = a1, Text = "Having a nap in my suit. The tech's comfortable, but the joke’s on me when I wake up.", TimeStamp = DateTime.Parse("2023-08-08 17:20:00") };
            var c40 = new Cheep() { CheepId = 40, AuthorId = a1.AuthorId, Author = a1, Text = "Pepper says I need hobbies. I told her 'Building a new suit every week counts, right?'", TimeStamp = DateTime.Parse("2023-08-09 18:30:00") };
            var c41 = new Cheep() { CheepId = 41, AuthorId = a1.AuthorId, Author = a1, Text = "Note to self: Don't mix coffee and tech. The last update nearly crashed the coffee machine.", TimeStamp = DateTime.Parse("2023-08-10 19:40:00") };
            var c42 = new Cheep() { CheepId = 42, AuthorId = a1.AuthorId, Author = a1, Text = "Building an AI to keep track of my to-do list... It keeps adding more things to itself.", TimeStamp = DateTime.Parse("2023-08-11 20:50:00") };
            var c43 = new Cheep() { CheepId = 43, AuthorId = a1.AuthorId, Author = a1, Text = "I created a supercharged vacuum cleaner. It’s now cleaning up my old projects, too.", TimeStamp = DateTime.Parse("2023-08-12 21:00:00") };
            var c44 = new Cheep() { CheepId = 44, AuthorId = a1.AuthorId, Author = a1, Text = "Had a meeting with the Avengers. I’m still the smartest one. #humblebrag", TimeStamp = DateTime.Parse("2023-08-13 22:10:00") };
            var c45 = new Cheep() { CheepId = 45, AuthorId = a1.AuthorId, Author = a1, Text = "I built a better Iron Man suit, but the real upgrade was the Wi-Fi. You can’t beat 5G.", TimeStamp = DateTime.Parse("2023-08-14 23:20:00") };
            var c46 = new Cheep() { CheepId = 46, AuthorId = a1.AuthorId, Author = a1, Text = "I should really teach a course on genius. No one’s figured it out like I have.", TimeStamp = DateTime.Parse("2023-08-15 10:30:00") };
            var c47 = new Cheep() { CheepId = 47, AuthorId = a1.AuthorId, Author = a1, Text = "Creating new tech is fun, but trying to explain it to Pepper? That's the real challenge.", TimeStamp = DateTime.Parse("2023-08-16 11:40:00") };
            var c48 = new Cheep() { CheepId = 48, AuthorId = a1.AuthorId, Author = a1, Text = "Just realized my suit’s AI has more personality than I do. Might be time to fix that.", TimeStamp = DateTime.Parse("2023-08-17 12:50:00") };
            var c49 = new Cheep() { CheepId = 49, AuthorId = a1.AuthorId, Author = a1, Text = "I might be a genius, but I still can’t figure out why Wi-Fi goes down every time I need it.", TimeStamp = DateTime.Parse("2023-08-18 13:00:00") };
            var c50 = new Cheep() { CheepId = 50, AuthorId = a1.AuthorId, Author = a1, Text = "Just sent out a ‘vacation alert’ to the team. I’ll be ‘working remotely’ from a beach in Malibu.", TimeStamp = DateTime.Parse("2023-08-19 14:10:00") };
            var c51 = new Cheep() { CheepId = 51, AuthorId = a1.AuthorId, Author = a1, Text = "I’m not saying I’m a genius, but the world’s best superhero suit just dropped. You’re welcome.", TimeStamp = DateTime.Parse("2023-08-20 15:20:00") };
            var c52 = new Cheep() { CheepId = 52, AuthorId = a1.AuthorId, Author = a1, Text = "Can we talk about the Avengers' meeting? It’s basically me explaining tech to a bunch of kids.", TimeStamp = DateTime.Parse("2023-08-21 16:30:00") };
            var c53 = new Cheep() { CheepId = 53, AuthorId = a1.AuthorId, Author = a1, Text = "I’ve decided that my next suit will come with an in-built snack dispenser. It's the future.", TimeStamp = DateTime.Parse("2023-08-22 17:40:00") };
            var c54 = new Cheep() { CheepId = 54, AuthorId = a1.AuthorId, Author = a1, Text = "I once made a device that could locate lost socks. Haven't quite figured out world peace yet, though.", TimeStamp = DateTime.Parse("2023-08-23 18:50:00") };
            var c55 = new Cheep() { CheepId = 55, AuthorId = a1.AuthorId, Author = a1, Text = "I could conquer the world. But first, I need to finish upgrading my suit.", TimeStamp = DateTime.Parse("2023-08-24 19:00:00") };
            var c56 = new Cheep() { CheepId = 56, AuthorId = a1.AuthorId, Author = a1, Text = "Another day, another genius-level invention. Someone stop me.", TimeStamp = DateTime.Parse("2023-08-25 20:10:00") };
            var c57 = new Cheep() { CheepId = 57, AuthorId = a1.AuthorId, Author = a1, Text = "My suit can fly. But can it avoid awkward family dinners? Asking for a friend.", TimeStamp = DateTime.Parse("2023-08-26 21:20:00") };
            var c58 = new Cheep() { CheepId = 58, AuthorId = a1.AuthorId, Author = a1, Text = "I built a machine that does the dishes. Now I need one that can take out the trash.", TimeStamp = DateTime.Parse("2023-08-27 22:30:00") };
            var c59 = new Cheep() { CheepId = 59, AuthorId = a1.AuthorId, Author = a1, Text = "I invented a robot to make my morning coffee. It's still better than me at it.", TimeStamp = DateTime.Parse("2023-08-28 23:40:00") };
            var c60 = new Cheep() { CheepId = 60, AuthorId = a1.AuthorId, Author = a1, Text = "Trying to figure out how to invent teleportation. Getting closer, but the Wi-Fi’s still the bigger problem.", TimeStamp = DateTime.Parse("2023-08-29 10:50:00") };
            var c61 = new Cheep() { CheepId = 61, AuthorId = a1.AuthorId, Author = a1, Text = "I’ve upgraded my suit. Now it can even handle Pepper’s shopping lists. Progress.", TimeStamp = DateTime.Parse("2023-08-30 12:00:00") };
            var c62 = new Cheep() { CheepId = 62, AuthorId = a1.AuthorId, Author = a1, Text = "Invented a new gadget. It’s too cool for words, but I’ll just call it ‘Stark-finity’ for now.", TimeStamp = DateTime.Parse("2023-08-31 13:10:00") };
            var c63 = new Cheep() { CheepId = 63, AuthorId = a1.AuthorId, Author = a1, Text = "Just spent an hour fixing my suit. Could have been saving the world, but priorities, right?", TimeStamp = DateTime.Parse("2023-09-01 14:20:00") };
            var c64 = new Cheep() { CheepId = 64, AuthorId = a1.AuthorId, Author = a1, Text = "I think I’ve just solved the mystery of why socks always disappear in the laundry. #genius", TimeStamp = DateTime.Parse("2023-09-02 15:30:00") };
            var c65 = new Cheep() { CheepId = 65, AuthorId = a1.AuthorId, Author = a1, Text = "Sometimes, I wonder if my suit is smarter than me. It probably is.", TimeStamp = DateTime.Parse("2023-09-03 16:40:00") };
            var c66 = new Cheep() { CheepId = 66, AuthorId = a1.AuthorId, Author = a1, Text = "Created a new AI assistant. It’s way better at multitasking than I am.", TimeStamp = DateTime.Parse("2023-09-04 17:50:00") };
            var c67 = new Cheep() { CheepId = 67, AuthorId = a1.AuthorId, Author = a1, Text = "I’m starting to think my suit has better social skills than I do.", TimeStamp = DateTime.Parse("2023-09-05 19:00:00") };
            var c68 = new Cheep() { CheepId = 68, AuthorId = a1.AuthorId, Author = a1, Text = "Invented a robot that does my laundry. Unfortunately, it still won’t fold the shirts the way I like it.", TimeStamp = DateTime.Parse("2023-09-06 20:10:00") };
            var c69 = new Cheep() { CheepId = 69, AuthorId = a1.AuthorId, Author = a1, Text = "Apparently, I’m the only one who thinks my jokes are funny. Still, I’m laughing.", TimeStamp = DateTime.Parse("2023-09-07 21:20:00") };
            var c70 = new Cheep() { CheepId = 70, AuthorId = a1.AuthorId, Author = a1, Text = "I built a new suit. It doesn’t do much, but at least it makes a great party trick.", TimeStamp = DateTime.Parse("2023-09-08 22:30:00") };


            var cheeps = new List<Cheep>()
            {
                c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16, c17, c18, c19, c20, c21, c22,
                c23, c24, c25, c26, c27, c28, c29, c30, c31, c32, c33, c34, c35, c36,
                c37, c38, c39, c40, c41, c42, c43, c44, c45, c46, c47, c48, c49, c50, c51, c52, c53, c54, c55, c56, c57, c58, c59, c60, c61, c62, c63, c64, c65, c66, c67, c68, c69, c70
            };
            a1.Cheeps = new List<Cheep>() { c1, c2, c3, c4, c5, c26, c31, c36, c37, c38, c39, c40, c41, c42, c43, c44, c45, c46, c47, c48, c49, c50, c51, c52, c53, c54, c55, c56, c57, c58, c59, c60, c61, c62, c63, c64, c65, c66, c67, c68, c69, c70 };
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