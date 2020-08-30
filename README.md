# Authy.AspNetCore
![.NET Core](https://github.com/Intecreate/Authy.AspNetCore/workflows/.NET%20Core/badge.svg)
[![codecov](https://codecov.io/gh/Intecreate/Authy.AspNetCore/branch/master/graph/badge.svg)](https://codecov.io/gh/Intecreate/Authy.AspNetCore)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/Intecreate/Marqeta.Net/blob/master/LICENSE)

A library designed to replace the existing TOTP algorithm in asp.net core 3.1 with Authy (https://authy.com/)

### Authy.AspNetCore is in development and you may encounter bugs. This project is subject to breaking changes.

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
//Note: IAuthyClient is registered with DI
private readonly IAuthyClient _authy;

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

You should not need to modify LoginWith2fa at all unless you want to use Authy OneTouch. For more information about using OneTouch view 
OneTouch.md 
