# Authy.AspNetCore
Authy.AspNetCore makes it easy for developers to replace the existing TOTP algorithm in asp.net core with Authy. 

### Authy.AspNetCore is in beta and you may encounter bugs. This project is subject to breaking changes.

# Quick Start
Using Authy.AspNetCore is easy. There are several "tricks" used that allow you to use Authy.AspNetCore without requiring any database migrations.

#### Add authy to your application (Note: If you are using a custom DBContext or IdentityUser class, replace them as needed)

```cs
services.AddDefaultIdentity<ApplicationUser>(options => {
    options.Tokens.AuthenticatorTokenProvider = AuthyBuilder.AUTHY_TOKEN_PROVIDER_NAME;
}).AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddAuthy2FA<ApplicationUser>();

services.AddAuthy(new AuthyCredentials("YOUR AUTHY API KEY HERE"));

services.AddHttpClient();
```

#### Note: This replaces the existing TOTP algorithm. We may readd the existing TOTP algorithm

Now that you have added Authy.AspNetCore you can create a user as follows (you should verify that the user was created)

```cs
//Note: IAuthyCall2FA is registered with DI
private readonly IAuthyCall2FA _authy;

public async Task<IActionResult> OnPostAsync()
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
    }

    if (!ModelState.IsValid)
    {
        return Page();
    }

    var authyUser = new AuthyUser
    {
        CountryCode = CountryCode,
        PhoneNumber = PhoneNumber,
        Email = user.Email
    };

    var code = await _authy.RegisterUserAsync(authyUser, _userManager, user);
    return RedirectToPage("./EnableAuthenticator");
    return Page();
}
```

You should not need to modify LoginWith2fa at all unless you want to use Authy OneTouch

# Using Authy OneTouch

Implementing OneTouch can be done in several ways. This is a method that allows you to add OneTouch with minimal effort.

#### Note: This uses HttpContext.Session and SignalR and Controllers

Modify the LoginWith2fa.cshtml.cs class to contain the following
```cs
//Note: IAuthyCall2FA is registered with DI
private readonly IAuthyCall2FA _authy;

//This should be part of the model
public string PushLoginId { get; set; }

public async Task<IActionResult> OnGetAsync(bool rememberMe = false, string returnUrl = null)
{
    // Ensure the user has gone through the username & password screen first
    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

    if (user == null)
    {
        throw new InvalidOperationException($"Unable to load two-factor authentication user.");
    }

    if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("AuthyUUID")))
    {
        PushLoginId = HttpContext.Session.GetString("AuthyPushId");
        HasPush = true;
        return Page();
    }

    var pref = await _authy.GetUserPreferredVerificationTypeAsync(_userManager, user);

    if (pref == VerificationType.UNKNOWN)
    {
        await _authy.SetUserPreferredVerificationTypeAsync(_userManager, user, VerificationType.PUSH);
        pref = VerificationType.PUSH;
    }

    if (pref == VerificationType.PUSH)
    {
        PushLoginId = Guid.NewGuid().ToString();
        var pushCode = await _authy.CreatePushVerificaiton(_userManager, user, new AuthyPushNotificationDetails
        {
            Message = $"{user.Username}, please verify that you are trying to login. If you did not initiate this request, click deny.",
            Details = new Dictionary<string, string>()
            {
                { "Device details", Request.Headers[HeaderNames.UserAgent] },
                { "Message", "univ.app login" },
                { "Action", "Login" },
                { "IP Address", HttpContext.Connection.RemoteIpAddress.ToString() }
            },
            HiddenDetails = new Dictionary<string, string>()
            {
                { "push_id", PushLoginId }
            },
            SecondsToExpire = 180
        });
        HasPush = true;

        HttpContext.Session.SetString("AuthyPushId", PushLoginId);
        HttpContext.Session.SetString("AuthyUUID", pushCode);
    }
    else
    {
        HasPush = false;
        await _authy.CreateVerification(_userManager, user, pref, true);
    }

    return Page();
}

public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
{
    if (!ModelState.IsValid)
    {
        return Page();
    }

    returnUrl = returnUrl ?? Url.Content("~/");

    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
    if (user == null)
    {
        throw new InvalidOperationException($"Unable to load two-factor authentication user.");
    }

    var authenticatorCode = Input.TwoFactorCode?.Replace(" ", string.Empty).Replace("-", string.Empty);

    if (string.IsNullOrWhiteSpace(authenticatorCode))
    {
        var code = HttpContext.Session.GetString("AuthyUUID");

        if (code == null)
        {
            _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return Page();
        }

        authenticatorCode = code;
    }

    var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, false);

    if (result.Succeeded)
    {
        _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
        HttpContext.Session.Remove("AuthyUUID");
        HttpContext.Session.Remove("AuthyPushId");
        return LocalRedirect(returnUrl);
    }
    else if (result.IsLockedOut)
    {
        _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
        return RedirectToPage("./Lockout");
    }
    else
    {
        _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return Page();
    }
}
```

Create a controller with the following
```cs
[Route("api/authyWebhooks/")]
[ApiController]
public class AuthyWebhookController : ControllerBase
{
    private readonly IHubContext<AuthyPushHub> _push;

    public AuthyWebhookController(IHubContext<AuthyPushHub> push)
    {
        _push = push;
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<string> Post([FromBody] AuthyOneTouch body)
    {
        await _push.Clients.Group(body.ApprovalRequest.Transaction.HiddenDetails.PushId.ToString()).SendAsync("authyLogin");

        return body.ApprovalRequest.Transaction.HiddenDetails.PushId.ToString();
    }
}
```

Create a hub with the following
```cs
[AllowAnonymous]
public class AuthyPushHub : Hub
{
    public Task ListenForAuthy(string groupId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    public Task StopListen(string groupId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }
}
```

Add the following javascript to the LoginWith2fa.cshtml

#### Note: Your Login button should have the id of "loginButton" for this to work

```js
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/3.1.3/signalr.min.js"></script>
<script>
    var signalR = require("@@microsoft/signalr");
    var connection;
    var aPushId = '@Model.PushLoginId';
    function onLogin() {
        connection.invoke("StopListen", aPushId);
        connection.stop().then(function () {
            document.getElementById('loginButton').click();
        }).catch(function (err) {
            return console.error(err.toString());
        });
    }
    function Login2FA() {
        connection = new signalR.HubConnectionBuilder().withUrl("/hubs/authy2fa")
            .configureLogging(signalR.LogLevel.Information)
            .build();
        connection.on("authyLogin", onLogin);
        connection.start().then(function () {
            connection.invoke("ListenForAuthy", aPushId).catch(function (err) { return console.error(err); });
        }).catch(function (err) {
            return console.error(err.toString());
        });
    }

    Login2FA();
</script>
```

Ensure that your startup class contains the following
```cs
services.AddSignalR();
services.AddControllers();

services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllers();
    endpoints.MapHub<AuthyPushHub>("/hubs/authy2fa");
});
```

Finally, you should add the endpoint url to OneTouch settings (for example "https://example.com/api/authyWebhooks/") with the POST method and Approved Transaction events.
