namespace Chirp.TestE2E;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class E2ETests : PageTest
{
    private const string AppUrl = "http://localhost:5273";
    private string _startupProjectPath;
    private Process? _appProcess;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private const string TestUsername = "Tester";
    private const string TestUserEmail = "testuser@gmail.com";
    private const string TestUserPassword = "Test@12345";

    readonly BrowserTypeLaunchOptions _browserTypeLaunchOptions = new BrowserTypeLaunchOptions
    {
        Headless = true
    };

    [SetUp]
    public async Task Setup()
    {
        Console.WriteLine(_startupProjectPath);
        _browser = await Playwright.Chromium.LaunchAsync(_browserTypeLaunchOptions);

        _context = await _browser.NewContextAsync();

        _page = await _context.NewPageAsync();

        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var solutionDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\.."));

        // Construct the path to your project
        _startupProjectPath = Path.Combine(solutionDirectory, "src", "Chirp.Web", "Chirp.Web.csproj");

        // Start the ASP.NET application
        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{_startupProjectPath}\" test",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            }
        };

        _appProcess.Start();

        // Wait for the application to start
        await Task.Delay(5000); // Adjust delay if needed
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Stop the ASP.NET application
        if (_appProcess != null && !_appProcess.HasExited)
        {
            _appProcess.Kill();
            _appProcess.Dispose();
        }
        
        // Dispose of the browser context
        _context?.DisposeAsync().GetAwaiter().GetResult();

        // Dispose of the browser
        _browser?.DisposeAsync().GetAwaiter().GetResult();
        
        // Delete the test database file
        string testDbFilePath = "F:\\Udvikling\\CSharp\\Chirp\\src\\Chirp.Infrastructure\\Data\\CheepTest.db";
        string walFilePath = testDbFilePath + "-wal";
        string shmFilePath = testDbFilePath + "-shm";
        
        // Check if the database file exists and delete it
        if (File.Exists(testDbFilePath))
        {
            File.Delete(testDbFilePath);
        }
        // Check if the WAL file exists and delete it
        if (File.Exists(walFilePath))
        {
            File.Delete(walFilePath);
        }
        // Check if the SHM file exists and delete it
        if (File.Exists(shmFilePath))
        {
            File.Delete(shmFilePath);
        }
    }
    
    //---------------------------------- HELPER METHODS ----------------------------------
    // Register
    private async Task RegisterUser()
    {
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Register");

        // Arrived at register page, and put in email and password
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync(TestUsername);
        await _page.GetByPlaceholder("name@example.com").ClickAsync();
        await _page.GetByPlaceholder("name@example.com").FillAsync(TestUserEmail);
        await _page.GetByPlaceholder("Password", new() { Exact = true }).ClickAsync();
        await _page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync(TestUserPassword);
        await _page.GetByPlaceholder("Confirm password").ClickAsync();
        await _page.GetByPlaceholder("Confirm password").FillAsync(TestUserPassword);

        
        
        // Clicks on the register button to register the account
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
    }

    // Login
    private async Task LoginUser()
    {
        // Goes to login page
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Login");

        // Fills in information
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync(TestUsername);
        await _page.GetByPlaceholder("password").ClickAsync();
        await _page.GetByPlaceholder("password").FillAsync(TestUserPassword);

        // Clicks on log in button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
    }

    // Logout
    /*private async Task LogoutUser()
    {
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Logout");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Click here to Logout" }).ClickAsync();
    }*/

    // Delete 
    private async Task DeleteUser()
    {
        // Removing the test user
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Manage");
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await _page.GetByPlaceholder("Please enter your username.").ClickAsync();
        await _page.GetByPlaceholder("Please enter your username.").FillAsync(TestUsername);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Delete all my data and close my account" }).ClickAsync();
    }
    
    /*
    private async Task GithubRegisterUser()
    {
        await _page.GotoAsync("http://localhost:5273/Identity/Account/Register");
        await _page.GetByRole(AriaRole.Button, new() { Name = "GitHub" }).ClickAsync();
        await _page.GetByLabel("Username or email address").FillAsync(GitHubCredentials.GetGitHubTestEmail());
        await _page.GetByLabel("Password").ClickAsync();
        await _page.GetByLabel("Password").FillAsync(GitHubCredentials.GetGitHubTestPassword());
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign in", Exact = true }).ClickAsync();

        // If reauthorization is required
        if (await _page.GetByRole(AriaRole.Heading, new() { Name = "Reauthorization required", Exact = true })
                .CountAsync() > 0)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Authorize ITU-BDSA2024-GROUP7" }).ClickAsync();
        }

        await _page.GetByPlaceholder("Please enter your email.").FillAsync("mygithubaccount@gmail.com");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" }).ClickAsync();
    }
    
    
    // If github login is saved in cache
    // Use this when you have already authorized with github in the test
    public async Task GithubCacheLoginUser()
    {
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Login");
        await _page.GetByRole(AriaRole.Button, new() { Name = "GitHub" }).ClickAsync();
    }
    
    // If github login is not saved in cache
    // Use this if it's the first time authorizing with github in the test
    public async Task GithubNoCacheLoginUser()
    {
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Login");
        await _page.GetByRole(AriaRole.Button, new() { Name = "GitHub" }).ClickAsync();
        await _page.GetByLabel("Username or email address").FillAsync("CODE-TEMP-TESTER");
        await _page.GetByLabel("Password").FillAsync("Telos@54321!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Sign in", Exact = true }).ClickAsync();
    }
    
    public async Task GithubDeleteUser()
    {
        // Removing the test user
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Manage");
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Delete data and close my" }).ClickAsync();
    }
    */

    //---------------------------------- PUBLIC TIMELINE TESTS ----------------------------------
    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task LoadPublicTimeline()
    {
        await _page!.GotoAsync($"{AppUrl}");
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" })).ToBeVisibleAsync();
    }

    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task PublicTimelineLoadingCheeps()
    {
        await _page!.GotoAsync($"{AppUrl}");
        await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
    }

    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task PublicTimelineNextAndPreviousPage()
    {
        await _page!.GotoAsync($"{AppUrl}");

        // If there is a next page button
        if (await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).CountAsync() > 0)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "<", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
        }
    }

    [Test]
    [Category("End2End")]
    [Category("PublicTimeline")]
    public async Task PublicTimelineFirstAndLastPage()
    {
        await _page!.GotoAsync($"{AppUrl}");

        // If there is a next page button
        if (await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).CountAsync() > 0)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = ">>", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "<<", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
        }
    }

    //---------------------------------- USER TIMELINE TESTS ----------------------------------
    [Test]
    [Category("End2End")]
    public async Task DoesUserTimelinePageSuccessfullyLoad()
    {
        // Go to Adrian's page
        await _page!.GotoAsync($"{AppUrl}/Adrian");
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Adrian's Timeline" })).ToBeVisibleAsync();
    }

    // Verify that clicking on a user goes to their timeline
    [Test]
    [Category("End2End")]
    public async Task GoToUserTimelineFromUsername()
    {
        await _page!.GotoAsync($"{AppUrl}");

        var firstMessageLink = _page.Locator("#messagelist > li:first-child a");

        var name = await firstMessageLink.InnerTextAsync();

        await firstMessageLink.ClickAsync();

        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = $"{name}'s Timeline" })).ToBeVisibleAsync();
    }

    // Check for presence of cheeps for some author
    [Test]
    [Category("End2End")]
    public async Task PresenceOfCheeps()
    {
        // Go to Adrian's page
        await _page!.GotoAsync($"{AppUrl}/Tony Stark");
        await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
    }

    // Check for no cheeps on user timeline with no cheeps
    [Test]
    [Category("End2End")]
    public async Task NoCheepsOnUserTimeline()
    {
        // Go to a user page with no cheeps
        await _page!.GotoAsync($"{AppUrl}/UserWithNoCheeps");
        await Expect(_page.GetByText("There are no cheeps so far.")).ToBeVisibleAsync();
    }

    // Check back button goes to public timeline
    [Test]
    [Category("End2End")]
    public async Task BackButtonGoesToPublicTimeline()
    {
        // Go to Adrian's page
        await _page!.GotoAsync($"{AppUrl}/Adrian");

        // Click on the back button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Back" }).ClickAsync();

        // Check if the public timeline is visible
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" })).ToBeVisibleAsync();
    }

    // Check next and previous buttons on user timeline
    [Test]
    [Category("End2End")]
    [Category("UserTimeline")]
    public async Task UserTimelineNextAndPreviousPage()
    {
        await _page!.GotoAsync($"{AppUrl}/Tony%20Stark");

        // If there is a next page button
        if (await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).CountAsync() > 0)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "<", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
        }
    }

    // Check first and last page buttons on user timeline
    [Test]
    [Category("End2End")]
    [Category("UserTimeline")]
    public async Task UserTimelineFirstAndLastPage()
    {
        await _page!.GotoAsync($"{AppUrl}/Tony%20Stark");

        // If there is a next page button
        if (await _page.GetByRole(AriaRole.Button, new() { Name = ">", Exact = true }).CountAsync() > 0)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = ">>", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "<<", Exact = true }).ClickAsync();
            await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
        }
    }

    //---------------------------------- PERSONAL TIMELINE TESTS ----------------------------------
    [Test]
    [Category("End2End")]
    public async Task GoToPersonalTimeline()
    {
        await RegisterUser();
        await LoginUser();

        await _page!.GetByRole(AriaRole.Link, new() { Name = "My timeline" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = $"{TestUsername}'s Timeline" }))
            .ToBeVisibleAsync();

        await DeleteUser();
    }


    //---------------------------------- REGISTER PAGE TESTS ----------------------------------

    // Registration page loads successfully (Expect the registration form)
    [Test]
    [Category("End2End")]
    public async Task RegisterPageLoads()
    {
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Register");

        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Create a new account." })).ToBeVisibleAsync();
        await Expect(_page.GetByPlaceholder("Username")).ToBeVisibleAsync();
        await Expect(_page.GetByPlaceholder("Name@example.com")).ToBeVisibleAsync();
        await Expect(_page.GetByPlaceholder("Password", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(_page.GetByPlaceholder("Confirm password")).ToBeVisibleAsync();
    }

    // Successfully registration with valid inputs
    [Test]
    [Category("End2End")]
    public async Task SuccessfullRegister()
    {
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Register");

        // Arrived at register page, and put in email and password
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync(TestUsername);
        await _page.GetByPlaceholder("name@example.com").ClickAsync();
        await _page.GetByPlaceholder("name@example.com").FillAsync(TestUserEmail);
        await _page.GetByPlaceholder("Password", new() { Exact = true }).ClickAsync();
        await _page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync(TestUserPassword);
        await _page.GetByPlaceholder("Confirm Password").ClickAsync();
        await _page.GetByPlaceholder("Confirm Password").FillAsync(TestUserPassword);

        // Clicks on the register button to register the account
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        // Person has correctly registered if logout button is visible
        await Expect(_page.GetByRole(AriaRole.Link, new() { Name = $"Logout" })).ToBeVisibleAsync();


        // Clean up
        await LoginUser();
        await DeleteUser();
    }

    // Registration without @ in email
    [Test]
    [Category("End2End")]
    public async Task RegisterWithoutAtInEmail()
    {
        // Listen for the dialog event
        _page!.Dialog += async (_, dialog) =>
        {
            // Verify the text of the popup
            if (dialog.Message.Contains("Mailadressen skal indeholde et \"@\". \"emailwithoutat\" mangler et \"@\"."))
            {
                // Accept the popup
                await Expect(_page.GetByText("Mailadressen skal indeholde et \"@\". \"emailwithoutat\" mangler et \"@\".")).ToBeVisibleAsync();
            }
        };

        // Attempt to register with an invalid email
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Register");
        await _page.GetByPlaceholder("name@example.com").FillAsync("emailwithoutat");
        await _page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync("MyBadAccount");
        await _page.GetByPlaceholder("Confirm Password").FillAsync("MyBadAccount");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
    }

    // Registration with password not living up to constraint (at least one nonalphanumeric character)
    [Test]
    [Category("End2End")]
    public async Task RegisterWithNoAlphanumericCharacter()
    {
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Register");
        await _page.GetByPlaceholder("Username").FillAsync("myusername");
        await _page.GetByPlaceholder("name@example.com").FillAsync("my@mail.com");
        await _page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync("BadPassword1234");
        await _page.GetByPlaceholder("Confirm Password").FillAsync("BadPassword1234");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(_page.GetByText("Passwords must have at least one non alphanumeric character."))
            .ToBeVisibleAsync();
    }

    // Registration with password not living up to constraints (at least one digit ('0'-'9'))
    [Test]
    [Category("End2End")]
    public async Task RegisterWithNoDigit()
    {
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Register");
        await _page.GetByPlaceholder("Username").FillAsync("myusername");
        await _page.GetByPlaceholder("name@example.com").FillAsync("my@mail.com");
        await _page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync("BadPassword!");
        await _page.GetByPlaceholder("Confirm Password").FillAsync("BadPassword!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(_page.GetByText("Passwords must have at least one digit ('0'-'9').")).ToBeVisibleAsync();
    }

    // Registration with password not living up to constraints (at least one uppercase ('A'-'Z'))
    [Test]
    [Category("End2End")]
    public async Task RegisterWithNoUppercase()
    {
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Register");
        await _page.GetByPlaceholder("Username").FillAsync("myusername");
        await _page.GetByPlaceholder("name@example.com").FillAsync("my@mail.com");
        await _page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync("badpassword1234!");
        await _page.GetByPlaceholder("Confirm Password").FillAsync("badpassword1234!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(_page.GetByText("Passwords must have at least one uppercase ('A'-'Z').")).ToBeVisibleAsync();
    }

    //---------------------------------- LOGIN PAGE TESTS ----------------------------------

    // Login page loads successfully (check for login form)
    [Test]
    [Category("End2End")]
    public async Task LoginPageLoads()
    {
        await _page!.GotoAsync("http://localhost:5273/Identity/Account/Login");

        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Use a local account to log in." }))
            .ToBeVisibleAsync();
        await Expect(_page.GetByPlaceholder("Username")).ToBeVisibleAsync();
        await Expect(_page.GetByPlaceholder("password")).ToBeVisibleAsync();
    }

    // Test successfully login
    [Test]
    [Category("End2End")]
    public async Task LoginSuccessfully()
    {
        await RegisterUser();

        // Goes to login page
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Login");

        // Fills in information
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync(TestUsername);
        await _page.GetByPlaceholder("password").ClickAsync();
        await _page.GetByPlaceholder("password").FillAsync(TestUserPassword);

        // Clicks on log in button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        // User arrived at the homepage and should now see a logout button with their email attached
        await Expect(_page.GetByRole(AriaRole.Link, new() { Name = $"Logout" })).ToBeVisibleAsync();

        await DeleteUser();
    }

    // Login with invalid credentials
    [Test]
    [Category("End2End")]
    public async Task NoRegisterInvalidLogin()
    {
        // Goes to login page
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Login");

        // Fills in information
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync(TestUsername);
        await _page.GetByPlaceholder("password").ClickAsync();
        await _page.GetByPlaceholder("password").FillAsync(TestUserPassword);

        // Clicks on log in button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        await Expect(_page.GetByText("Invalid login attempt.")).ToBeVisibleAsync();
    }

    // Login with no email entered
    [Test]
    [Category("End2End")]
    public async Task LoginWithNoEmailEntered()
    {
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Login");
        // Fills in information
        await _page.GetByPlaceholder("password").ClickAsync();
        await _page.GetByPlaceholder("password").FillAsync(TestUserPassword);

        // Clicks on log in button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        await Expect(_page.GetByText("The Username field is required.")).ToBeVisibleAsync();
    }

    // Login with no password entered
    [Test]
    [Category("End2End")]
    public async Task LoginWithNoPasswordEntered()
    {
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Login");
        // Fills in information
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync(TestUsername);

        // Clicks on log in button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        await Expect(_page.GetByText("The Password field is required.")).ToBeVisibleAsync();
    }


    // Check 'register as a new user' redirects to registration page.
    [Test]
    [Category("End2End")]
    public async Task LoginPageLinkRedirectToRegistrationPage()
    {
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Login");
        await _page.GetByRole(AriaRole.Link, new() { Name = "Register as a new user" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Create a new account." })).ToBeVisibleAsync();
    }
    
   
    //---------------------------------- LOGOUT PAGE TESTS ----------------------------------

    // Logout page load successfully (check for logout button)
    [Test]
    [Category("End2End")]
    public async Task LogoutPageLoads()
    {
        await RegisterUser();
        await LoginUser();
        
        await _page!.GetByRole(AriaRole.Link, new() { Name = $"Logout" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Click here to Logout" })).ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
    }

    // The logout button logs user out (check for no authentication and redirect)
    [Test]
    [Category("End2End")]
    public async Task LogoutButtonWorks()
    {
        await RegisterUser();
        await LoginUser();
        
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Logout");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Click here to Logout" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Use a local account to log in." })).ToBeVisibleAsync();
        
        // Clean up
        await LoginUser();
        await DeleteUser();
    }
    
    //---------------------------------- MANAGE ACCOUNT TESTS ----------------------------------
    
    // Manage page loads successfully
    [Test]
    [Category("End2End")]
    public async Task LoadManageAccountPage()
    {
        await RegisterUser();
        await LoginUser();
        
        await _page!.GotoAsync($"{AppUrl}");
        
        await _page.GetByRole(AriaRole.Link, new() { Name = "Manage account" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Manage your account" })).ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
    }
    
    // Personal data page loads successfully
    [Test]
    [Category("End2End")]
    public async Task LoadManageUserPage()
    {
        await RegisterUser();
        await LoginUser();
        
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Manage");
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Personal Data" })).ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
    }
    
    //---------------------------------- CHEEPS TESTS ----------------------------------
    
    // Testing Successful cheep line after login
    [Test]
    [Category("End2End")]
    public async Task TestShareCheepsVisibilityPublicTimeline()
    {
        await RegisterUser();
        await LoginUser();
        await _page!.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
        await Expect(_page.GetByText($"What's on your mind {TestUsername}? Share")).ToBeVisibleAsync();
        
        
        // Clean up
        await DeleteUser();   
    }
    [Test]
    [Category("End2End")]
    public async Task TestShareCheepsVisibilityPrivateTimeline()
    {
        await RegisterUser();
        await LoginUser();
        
        await _page!.GetByRole(AriaRole.Link, new() { Name = "My timeline" }).ClickAsync();
        await Expect(_page.GetByText($"What's on your mind {TestUsername}? Share")).ToBeVisibleAsync();
        
        
        // Clean up
        await DeleteUser();   
    }
    [Test]
    [Category("End2End")]
    public async Task CheepingCheeps()
    {
        await RegisterUser();
        await LoginUser();
        
        await _page!.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("Hello World!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(_page.Locator("li").Filter(new() { HasText = "Hello World!" }).First).ToBeVisibleAsync();

        
        // Clean up and delete data
        await DeleteUser();   
    }

    [Test]
    [Category("End2End")]
    public async Task CheepingFromPrivateTimeline()
    {
        {
            await RegisterUser();
            await LoginUser();
            
            await _page!.GetByRole(AriaRole.Link, new() { Name = "My timeline" }).ClickAsync();
            await _page.Locator("#CheepText").ClickAsync();
            await _page.Locator("#CheepText").FillAsync("Hello World!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            await Expect(_page.Locator("li").Filter(new() { HasText = "Hello World!" }).First).ToBeVisibleAsync();

        
            // Clean up and delete data
            await DeleteUser();   
        }
    }
    [Test]
    [Category("End2End")]
    public async Task EmptyCheeps()
    {
        {
            await RegisterUser();
            await LoginUser();
            
            await _page!.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            await Expect(_page.GetByText("At least write something")).ToBeVisibleAsync();
        
            // Clean up and delete data
            await DeleteUser();   
        }
    }
    [Test]
    [Category("End2End")]
    public async Task LongCheeps()
    {
        {
            await RegisterUser();
            await LoginUser();
            
            await _page!.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
            await _page.Locator("#CheepText").ClickAsync();
            await _page.Locator("#CheepText").FillAsync("Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message Very Long Message ");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            await Expect(_page.GetByText("Maximum length is 160")).ToBeVisibleAsync();
        
            // Clean up and delete data
            await DeleteUser();   
        }
    }
    [Test]
    [Category("End2End")]
    public async Task DeletedCheeps()
    {
        {
            await RegisterUser();
            await LoginUser();
            
            
            await _page!.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
            await _page.Locator("#CheepText").ClickAsync();
            await _page.Locator("#CheepText").FillAsync("HelloWorld!RasmusMathiasNikolajMarcusErTelos!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            // Clean up and delete data
            await DeleteUser();   
            
            // check that the cheep is deleted
            await Expect(_page.Locator("li").Filter(new() { HasText = "HelloWorld!RasmusMathiasNikolajMarcusErTelos!" }).First).Not.ToBeVisibleAsync();
        }
    } 
    //---------------------------------- GitHub ACCOUNT TESTS ----------------------------------
    
    
    // // Login with Github (cached / login saved in cache)
    // [Test]
    // [Category("End2End")]
    // public async Task LoginWithCachedGithub()
    // {
    //     await GithubRegisterUser();
    //     await GithubCacheLoginUser();
    //     await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "Logout [mygithubaccount@gmail" })).ToBeVisibleAsync();
    //
    //     await GithubDeleteUser();
    // }
    
    // Login with Github (non cached / login not saved in cache)
    // [Test]
    // [Category("End2End")]
    // public async Task LoginWithNoCachedGithub()
    // {
    //     await GithubRegisterUser();
    //     await _context!.CloseAsync(); // Close the current context to clear cache
    //     _context = await _browser.NewContextAsync(); // Create a new context
    //     _page = await _context.NewPageAsync(); // Create a new page in the new context
    //     await GithubNoCacheLoginUser();
    //     await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "Logout [mygithubaccount@gmail" })).ToBeVisibleAsync();
    //
    //     await GithubDeleteUser();
    // }

    // // Register with Github
    // [Test]
    // [Category("End2End")]
    // public async Task RegisterWithGithub()
    // {
    //     await GithubRegisterUser();
    //     await Expect(_page.GetByText("Thank you for confirming your")).ToBeVisibleAsync();
    //
    //     await GithubCacheLoginUser();
    //     await GithubDeleteUser();
    // }
}


// This is used to get the GitHub credentials from the user secrets
// public class GitHubCredentials
// {
//     private static IConfiguration GetConfiguration()
//     {
//         return new ConfigurationBuilder()
//             .SetBasePath(Directory.GetCurrentDirectory())
//             .AddUserSecrets<GitHubCredentials>()
//             .Build();
//     }
//
//     public static string GetGitHubTestEmail()
//     {
//         var config = GetConfiguration();
//         return config["GITHUBTESTACCOUNTUSERNAME"];
//     }
//
//     public static string GetGitHubTestPassword()
//     {
//         var config = GetConfiguration();
//         return config["GITHUBTESTACCOUNTPASSWORD"];
//     }
// }