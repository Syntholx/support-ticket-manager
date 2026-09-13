using Microsoft.AspNetCore.Identity;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(WebApplication app)
    {
        app.MapPost("/api/auth/register", async (RegisterRequest request, UserManager<ApplicationUser> userManager) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new
                {
                    message = "E-mail i hasło są wymagane"
                });
            }
            ApplicationUser user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            IdentityResult result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new
                {
                    message = "Nie udało się utworzyć konta"
                });
            }
            return Results.Json(new
            {
                message = "Konto zostało utworzone."
            },


            statusCode: 201);
        });
        app.MapPost("/api/auth/login",
        async (LoginRequest request,
               SignInManager<ApplicationUser> signInManager) =>
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new
            {
                message = "E-mail i hasło są wymagane."
            });
        }

        var result = await signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            message = "Zalogowano."
        });
    });

        app.MapGet("/api/auth/me", (System.Security.Claims.ClaimsPrincipal user) =>
        {
            return Results.Ok(new
            {
                id = user.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                name = user.Identity?.Name
            });

        }).RequireAuthorization();
        app.MapPost("/api/auth/logout", async (SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();

            return Results.Ok(new
            {
                message = "Wylogowano"
            });
        }).RequireAuthorization();

    }
}
