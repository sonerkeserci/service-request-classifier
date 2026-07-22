using Microsoft.AspNetCore.Identity;
using RequestClassifier.Domain.Entities;

namespace RequestClassifier.Infrastructure.Data.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Create the required application roles if they do not already exist.
        var roles = new[] { "Admin", "Employee" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@vbb.com";
        const string adminPassword = "Admin.65";

        // Stop if the initial administrator account already exists.
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin is not null)
            return;

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Admin",
            IsActive = true,
            EmailConfirmed = true
        };

        // Create the initial administrator account.
        var createResult = await userManager.CreateAsync(
            adminUser,
            adminPassword);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                createResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"The initial administrator could not be created: {errors}");
        }

        // Assign the Admin role to the initial administrator account.
        var roleResult = await userManager.AddToRoleAsync(
            adminUser,
            "Admin");

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                "The Admin role could not be assigned.");
        }
    }
}