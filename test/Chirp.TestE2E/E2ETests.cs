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
    /// <summary>
    /// One time Setup for setting up the process of running the test enviroment
    /// </summary>
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
            _appProcess.WaitForExit(5000); //closes after specified time
            _appProcess.Dispose();
        }
        
        // Dispose of the browser context
        _context?.DisposeAsync().GetAwaiter().GetResult();

        // Dispose of the browser
        _browser?.DisposeAsync().GetAwaiter().GetResult();
        
        // Delete the test database file
        var solutionDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\.."));
        var testDbFilePath = Path.Combine(solutionDirectory, "src", "Chirp.Infrastructure", "Data", "CheepTest.db");
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
    private async Task RegisterUser(String? userCount="")
    {
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Register");

        // Arrived at register page, and put in email and password
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync(TestUsername+userCount);
        await _page.GetByPlaceholder("name@example.com").ClickAsync();
        await _page.GetByPlaceholder("name@example.com").FillAsync(TestUserEmail+userCount);
        await _page.GetByPlaceholder("Password", new() { Exact = true }).ClickAsync();
        await _page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync(TestUserPassword+userCount);
        await _page.GetByPlaceholder("Confirm password").ClickAsync();
        await _page.GetByPlaceholder("Confirm password").FillAsync(TestUserPassword+userCount);

        
        
        // Clicks on the register button to register the account
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
    }

    // Login
    private async Task LoginUser(String? userCount="")
    {
        // Goes to login page
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Login");

        // Fills in information
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync(TestUsername+userCount);
        await _page.GetByPlaceholder("password").ClickAsync();
        await _page.GetByPlaceholder("password").FillAsync(TestUserPassword+userCount);

        // Clicks on log in button
        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
    }

    // Logout
    private async Task LogoutUser()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Login Symbol Logout" }).ClickAsync();
    }

    // Delete 
    private async Task DeleteUser()
    {
        // Removing the test user
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Manage");
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Forget me!" }).ClickAsync();
    }

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
        // Go to Adrians's page
        await _page!.GotoAsync($"{AppUrl}/Adrian");
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Adrian's Timeline" })).ToBeVisibleAsync();
    }

    // Verify that clicking on a user goes to their timeline
    [Test]
    [Category("End2End")]
    public async Task GoToUserTimelineFromUsername()
    {
        await _page!.GotoAsync($"{AppUrl}");

        var firstMessageLink = _page.Locator("#messagelist > li:first-child a").Nth(0);

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
        await _page!.GotoAsync($"{AppUrl}/Adrian");
        await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
    }

    // Check for no cheeps on user timeline with no cheeps
    [Test]
    [Category("End2End")]
    public async Task NoCheepsOnUserTimeline()
    {
        await RegisterUser();
        await LoginUser();
        
        // Go to a user page with no cheeps
        await _page!.GotoAsync($"{AppUrl}/{TestUsername}");
        await Expect(_page.GetByText("There are no cheeps so far.")).ToBeVisibleAsync();
        
        await DeleteUser();
    }
    
    // Check for no cheeps on user timeline with no cheeps
    [Test]
    [Category("End2End")]
    public async Task NoUserRedirectToPublicTimeline()
    {
        // Go to a user page with no cheeps
        await _page!.GotoAsync($"{AppUrl}/AUserThatDoesNotExist");
        await Expect(_page.GetByText("Public Timeline")).ToBeVisibleAsync();
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
        await _page!.GotoAsync($"{AppUrl}/Darth%20Vader");

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
        await _page!.GotoAsync($"{AppUrl}/Darth%20Vader");
        
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
        await LogoutUser();

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

    // The logout button logs user out (check for no authentication and redirect)
    [Test]
    [Category("End2End")]
    public async Task LogoutButtonWorks()
    {
        await RegisterUser();
        
        await _page!.GotoAsync($"{AppUrl}");
        await _page.GetByRole(AriaRole.Link, new() { Name = "Login Symbol Logout" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Log in", Exact = true })).ToBeVisibleAsync();
        
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
        
        await _page!.GotoAsync($"{AppUrl}/Identity/Account/Manage");
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Personal Data" })).ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
    }
    
    // Check that users don't follow authors that are deleting.
    [Test]
    [Category("End2End")]
    public async Task StopFollowingDeletedUser()
    {
        await _page!.GotoAsync($"{AppUrl}");
        await _page.GetByRole(AriaRole.Link, new() { Name = "Register Symbol Register" }).ClickAsync();
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync("bob1");
        await _page.GetByPlaceholder("Name@example.com").ClickAsync();
        await _page.GetByPlaceholder("Name@example.com").FillAsync("bob1@bob.dk");
        await _page.GetByPlaceholder("Name@example.com").PressAsync("Tab");
        await _page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync("Bob1234!");
        await _page.GetByPlaceholder("Password", new() { Exact = true }).PressAsync("Tab");
        await _page.GetByPlaceholder("Confirm password").FillAsync("Bob1234!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("hej");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Login Symbol Logout" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Register Symbol Register" }).ClickAsync();
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync("bob2");
        await _page.GetByPlaceholder("Username").PressAsync("Tab");
        await _page.GetByPlaceholder("Name@example.com").FillAsync("bob2@bob.dk");
        await _page.GetByPlaceholder("Name@example.com").PressAsync("Tab");
        await _page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync("Bob1234!");
        await _page.GetByPlaceholder("Password", new() { Exact = true }).PressAsync("Tab");
        await _page.GetByPlaceholder("Confirm password").FillAsync("Bob1234!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await _page.Locator("li").Filter(new() { HasText = "bob1 Follow" }).Locator("#followButton").First.ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Login Symbol Logout" }).ClickAsync();
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync("bob1");
        await _page.GetByPlaceholder("Password").ClickAsync();
        await _page.GetByPlaceholder("Password").FillAsync("Bob1234!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Login Symbol Manage account" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Forget me!" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Register Symbol Register" }).ClickAsync();
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync("bob1");
        await _page.GetByPlaceholder("Name@example.com").ClickAsync();
        await _page.GetByPlaceholder("Name@example.com").FillAsync("bob1@bob.dk");
        await _page.GetByPlaceholder("Name@example.com").PressAsync("Tab");
        await _page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync("Bob1234!");
        await _page.GetByPlaceholder("Confirm password").ClickAsync();
        await _page.GetByPlaceholder("Confirm password").FillAsync("Bob1234!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("hejsa");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Login Symbol Logout" }).ClickAsync();
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync("bob2");
        await _page.GetByPlaceholder("Password").ClickAsync();
        await _page.GetByPlaceholder("Password").FillAsync("Bob1234!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        
        // Assert
        await Expect(_page.GetByText("bob1 Follow")).ToBeVisibleAsync();
        
        // Clean up
        await _page.GetByRole(AriaRole.Link, new() { Name = "Login Symbol Manage account" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Forget me!" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Login Symbol Login" }).ClickAsync();
        await _page.GetByPlaceholder("Username").ClickAsync();
        await _page.GetByPlaceholder("Username").FillAsync("bob1");
        await _page.GetByPlaceholder("Username").PressAsync("Tab");
        await _page.GetByPlaceholder("Password").FillAsync("Bob1234!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Login Symbol Manage account" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Forget me!" }).ClickAsync();
    }
    
    // Personal data page loads successfully
    [Test]
    [Category("End2End")]
    public async Task DeleteUserCheeps()
    {
        await RegisterUser();
        
        await _page!.GotoAsync($"{AppUrl}");
        
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("My message");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("So nice");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Home Symbol My timeline" }).ClickAsync();
        await Expect(_page.GetByText("There are no cheeps so far.")).Not.ToBeVisibleAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Login Symbol Manage account" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "Delete my Cheeps" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Home Symbol My timeline" }).ClickAsync();
        await Expect(_page.GetByText("There are no cheeps so far.")).ToBeVisibleAsync();
        await Expect(_page.GetByText("There are no cheeps so far.")).ToBeVisibleAsync();
        
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
    
            await _page.GetByRole(AriaRole.Link, new() { Name = "Home Symbol My timeline" }).ClickAsync();
            await _page.Locator("#CheepText").ClickAsync();
            await _page.Locator("#CheepText").FillAsync("Hello");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Home Symbol My timeline" }).ClickAsync();
            await Expect(_page.GetByText("Hello")).ToBeVisibleAsync();
    
        
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

    //---------------------------------- FOLLOWING TESTS ----------------------------------
    
    [Test]
    [Category("End2End")]
    public async Task DoesFollowButtonLoad()
    {
        await RegisterUser();

        await Expect(_page.Locator("li").Locator("#followButton").First).ToBeVisibleAsync();
  
        
        // Clean up
        await DeleteUser();
    }
    
    [Test]
    [Category("End2End")]
    public async Task DoesUnfollowButtonLoad()
    {
        await RegisterUser();

        await _page.Locator("li").Locator("#followButton").First.ClickAsync();
        
        await Expect(_page.Locator("li").Locator("#unfollowButton").First).ToBeVisibleAsync();
        
        await _page.Locator("li").Locator("#unfollowButton").First.ClickAsync();
        
        // Clean up
        await DeleteUser();
    }


    [Test]
    [Category("End2End")]
    public async Task DoesFollowedAuthorLoadCheeps()
    {
        await RegisterUser();

        await _page.Locator("li").Locator("#followButton").First.ClickAsync();
        
        await _page.GetByRole(AriaRole.Link, new() { Name = "Home Symbol My timeline" }).ClickAsync();
        
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Unfollow" })).ToBeVisibleAsync();
        
        await _page.GetByRole(AriaRole.Button, new() { Name = "Unfollow" }).ClickAsync();
        
        // Clean up
        await DeleteUser();
    }

    /*---------------------------------- FOLLOWING LISTS TESTS ----------------------------------*/
    [Test]
    [Category("End2End")]
    public async Task DoesFollowListDisplayCorrectAmountFollowers()
    {
        // Follows another user and checks if the count for following is 1 and then unfollows the user and checks if the count is 0
        await RegisterUser("1");
        
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("HelloWorld!RasmusMathiasNikolajMarcusErTelos!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await LogoutUser();
        
        await RegisterUser("2");

        await _page.Locator("li").First.Locator("#followButton").ClickAsync();
        
        await _page.GetByRole(AriaRole.Link, new() { Name = "Home Symbol My timeline" }).ClickAsync();
        
        await Expect(_page.Locator("body")).ToContainTextAsync("Following: 1");
        
        await _page.GetByRole(AriaRole.Button, new() { Name = "Unfollow" }).ClickAsync();
        
        await _page.GetByRole(AriaRole.Link, new() { Name = "Home Symbol My timeline" }).ClickAsync();
        
        await Expect(_page.Locator("body")).ToContainTextAsync("Followers: 0");
        
        // Clean up
        await DeleteUser();
        await LoginUser("1");
        await DeleteUser();    
    }
    
    [Test]
    [Category("End2End")]
    public async Task DoesFollowListDisplayCorrectAmountFollowing()
    {
        // Follows another user and checks if the count for followers is 1 and then unfollows the user and checks if the count is 0

        await RegisterUser("1");
        
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync($"HelloWorld!RasmusMathiasNikolajMarcusErTelos!+{DateTime.Now.ToString("HH:mm:ss")}");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await LogoutUser();
        
        await RegisterUser("2");
        
        await _page.Locator("li").First.Locator("#followButton").ClickAsync();

        await _page.GetByRole(AriaRole.Link, new() { Name = "Tester1" }).ClickAsync();
        
        await Expect(_page.Locator("body")).ToContainTextAsync("Followers: 1");
        
        await _page.GetByRole(AriaRole.Button, new() { Name = "Unfollow" }).ClickAsync();
        
        await _page.GetByRole(AriaRole.Link, new() { Name = "Tester1" }).ClickAsync();
        
        await Expect(_page.Locator("body")).ToContainTextAsync("Followers: 0");
        
        // Clean up
        await DeleteUser();
        await LoginUser("1");
        await DeleteUser();
    }
    [Test]
    [Category("End2End")]
    public async Task DoesFollowPopupDisplayCorrectFollowers()
    {
        // Follows another user and checks if a user shows up in the follower list popup
        await RegisterUser("1");
        
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("HelloWorld!RasmusMathiasNikolajMarcusErTelos!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await LogoutUser();
        
        await RegisterUser("2");
    
        await _page.Locator("li").First.Locator("#followButton").ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Home Symbol My timeline" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Following:" }).ClickAsync();
        await Expect(_page.Locator("#popup2").GetByRole(AriaRole.Button, new() { Name = "Unfollow" })).ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
        await LoginUser("1");
        await DeleteUser();
    }
    
    [Test]
    [Category("End2End")]
    public async Task DoesFollowPopupDisplayCorrectFollowing()
    {
        // Follows another user and checks if a user shows up in the following list popup

        await RegisterUser();
    
        await _page.Locator("li").First.Locator("#followButton").ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Home Symbol My timeline" }).ClickAsync();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Following:" }).ClickAsync();
        await Expect(_page.Locator("#popup2").GetByRole(AriaRole.Button, new() { Name = "Unfollow" })).ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
    }
    
    // ---------------------------------- Delete TESTS ----------------------------------
    [Test]
    [Category("End2End")]
    // Test that the cheep is being deleted
    public async Task DoesDeleteButtonLoad()
    {
        await RegisterUser();
        
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("Testing that this is a deleteable cheep");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(_page.Locator("li").Filter(new() { HasText = "Testing that this is a deleteable cheep" }).First).ToBeVisibleAsync();
        await _page.Locator("#deleteButton").ClickAsync();
        await Expect(_page.Locator("li").Filter(new() { HasText = "Testing that this is a deleteable cheep" }).First).Not.ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
    }
    
    /*---------------------------------- REACTIONS TESTS ----------------------------------*/
    [Test]
    [Category("End2End")]
    public async Task LikeReactionTest()
    {
        // Like reaction another users post and check if the corresponding reaction shows up
        await RegisterUser("1");
        
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("HelloWorld!RasmusMathiasNikolajMarcusErTelos!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await LogoutUser();
        await RegisterUser("2");
        
        // Hover over the like button to reveal the reactions
        await _page.Locator("li").First.Locator("#likeMethod").HoverAsync();

        // Click the reaction button
        await _page.GetByRole(AriaRole.Button, new() { Name = "👍" }).First.ClickAsync();
        
        await Expect(_page.Locator("span").Filter(new() { HasText = "👍" }).First).ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
        await LoginUser("1");
        await DeleteUser();    
    }
    
    [Test]
    [Category("End2End")]
    public async Task DislikeReactionTest()
    {
        // Dislike reaction another users post and check if the corresponding reaction shows up
        await RegisterUser("1");
        
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("HelloWorld!RasmusMathiasNikolajMarcusErTelos!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await LogoutUser();
        await RegisterUser("2");
        
        // Hover over the dislike button to reveal the reactions
        await _page.Locator("li").First.Locator("#dislikeMethod").HoverAsync();

        // Click the reaction button
        await _page.GetByRole(AriaRole.Button, new() { Name = "👎" }).First.ClickAsync();
        
        await Expect(_page.Locator("span").Filter(new() { HasText = "👎" })).ToBeVisibleAsync();

        // Clean up
        await DeleteUser();
        await LoginUser("1");
        await DeleteUser();    
    }
    
    [Test]
    [Category("End2End")]
    public async Task SwitchFromLikeToDislikeReactionTest()
    {
        // Dislike reaction another users post and switch to Like reaction and check if the corresponding reaction shows up
        await RegisterUser("1");
        
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("HelloWorld!RasmusMathiasNikolajMarcusErTelos!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await LogoutUser();
        await RegisterUser("2");
        
        await _page.Locator("li").First.Locator("#likeMethod").HoverAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "👍" }).First.ClickAsync();
        await _page.Locator("li").First.Locator("#dislikeMethod").HoverAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "👎" }).First.ClickAsync();
        
        await Expect(_page.Locator("span").Filter(new() { HasText = "👎" })).ToBeVisibleAsync();


        // Clean up
        await DeleteUser();
        await LoginUser("1");
        await DeleteUser();    
    }

    [Test]
    [Category("End2End")]
    public async Task SwitchFromDislikeToLikeReactionTest()
    {
        // Like reaction another users post and switch to Dislike reaction and check if the corresponding reaction shows up
        await RegisterUser("1");

        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("HelloWorld!RasmusMathiasNikolajMarcusErTelos!");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

        await LogoutUser();
        await RegisterUser("2");

        await _page.Locator("li").First.Locator("#dislikeMethod").HoverAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "👎" }).First.ClickAsync();
        await _page.Locator("li").First.Locator("#likeMethod").HoverAsync();
        await _page.GetByRole(AriaRole.Button, new() { Name = "👍" }).First.ClickAsync();

        await Expect(_page.Locator("span").Filter(new() { HasText = "👍" }).First).ToBeVisibleAsync();


        // Clean up
        await DeleteUser();
        await LoginUser("1");
        await DeleteUser();
    }

//---------------------------------- Image TESTS  ----------------------------------
    [Test]
    [Category("SkipOnGitHubActions")]
    public async Task CanUserUploadImage()
    {
        await RegisterUser();
        
        var solutionDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\.."));
        var imagePath = Path.Combine(solutionDirectory, "src", "Chirp.Web", "wwwroot", "images", "icon1.png");
        
        await _page.Locator("#CheepImage").ClickAsync();
        await _page.Locator("#CheepImage").SetInputFilesAsync(new[] { imagePath });
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("Hej");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Img, new() { Name = "Cheep Image" })).ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
    }
    
    [Test]
    [Category("SkipOnGitHubActions")]
    public async Task CanUserUploadGif()
    {
        await RegisterUser();
        
        var solutionDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\.."));
        var imagePath = Path.Combine(solutionDirectory, "src", "Chirp.Web", "wwwroot", "images", "TESTGIF.gif");
        
        await _page.Locator("#CheepImage").ClickAsync();
        await _page.Locator("#CheepImage").SetInputFilesAsync(new[] { imagePath});
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("Hej");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Img, new() { Name = "Cheep Image" })).ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
    }
    
    // ---------------------------------- Comment TESTS ----------------------------------
    [Test]
    [Category("End2End")]
    public async Task DoesCommentDeleteButtonLoad()
    {
        await RegisterUser();
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("CreateCheepTest");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "View Comments" }).First).ToBeVisibleAsync();
        await  _page.GetByRole(AriaRole.Button, new() { Name = "View Comments" }).First.ClickAsync();
        await _page.GetByPlaceholder("Answer Tester").ClickAsync();
        await _page.GetByPlaceholder("Answer Tester").FillAsync("CreateCommentTest");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Add Comment" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "\uf1f8" })).ToBeVisibleAsync();
        
        // Clean up
        await DeleteUser();
    }
    [Test]
    [Category("End2End")]
    public async Task CommentPublicAvalable()
    {
        await RegisterUser();
        await _page.Locator("#CheepText").ClickAsync();
        await _page.Locator("#CheepText").FillAsync("TestCreateCheep");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await LogoutUser();
        await _page.GetByRole(AriaRole.Link, new() { Name = "Home Symbol Timeline" }).ClickAsync();
        
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "View Comments" }).First).ToBeVisibleAsync();
        await  _page.GetByRole(AriaRole.Button, new() { Name = "View Comments" }).First.ClickAsync();
        
        await LoginUser();
        
        // Clean up
        await DeleteUser();
    }
    [Test]
    [Category("End2End")]
    public async Task CommentTestLocator()
    {
        await RegisterUser();

        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "View Comments" }).First).ToBeVisibleAsync();
        await  _page.GetByRole(AriaRole.Button, new() { Name = "View Comments" }).First.ClickAsync();
        await Expect(_page.GetByText("Back Comment section")).ToBeVisibleAsync();

        // Clean up
        await DeleteUser();
    }
    
    
    
}
