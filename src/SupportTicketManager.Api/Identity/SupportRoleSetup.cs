using Microsoft.AspNetCore.Identity;

public static class SupportRoleSetup
{
    public static async Task AssignAsync(
        string email,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            throw new InvalidOperationException("Nie znaleziono konta.");
        }

        bool roleExists = await roleManager.RoleExistsAsync("Support");

        if (!roleExists)
        {
            IdentityResult creation = await roleManager.CreateAsync(
                new IdentityRole("Support"));

            if (!creation.Succeeded)
            {
                throw new InvalidOperationException(
                    "Nie udało się utworzyć roli Support.");
            }
        }

        if (await userManager.IsInRoleAsync(user, "Support"))
        {
            return;
        }

        IdentityResult assignment =
            await userManager.AddToRoleAsync(user, "Support");

        if (!assignment.Succeeded)
        {
            throw new InvalidOperationException(
                "Nie udało się przypisać roli Support.");
        }
    }
}