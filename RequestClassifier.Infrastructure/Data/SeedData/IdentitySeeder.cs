using Microsoft.AspNetCore.Identity;
using RequestClassifier.Domain.Entities;

namespace RequestClassifier.Infrastructure.Data.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            "Admin",
            "Employee"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult =
                    await roleManager.CreateAsync(
                        new IdentityRole(role));

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(
                            error => error.Description));

                    throw new InvalidOperationException(
                        $"Rol oluşturulamadı: {errors}");
                }
            }
        }

        /*
         * Sistemde Admin rolüne sahip en az bir kullanıcı varsa
         * yeni başlangıç yöneticisi oluşturma.
         *
         * Böylece yönetici e-postası panelden değiştirilse bile
         * Seeder tekrar ikinci bir admin oluşturmaz.
         */
        var existingAdmins =
            await userManager.GetUsersInRoleAsync(
                "Admin");

        if (existingAdmins.Any())
        {
            return;
        }

        /*
         * Bunlar yalnızca veritabanında hiç yönetici
         * bulunmadığında kullanılacak başlangıç bilgileridir.
         */
        const string adminEmail =
            "admin@belediye.gov.tr";

        const string adminPassword =
            "Admin.65";

        var adminUser =
            new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "Belediye",
                IsActive = true,
                EmailConfirmed = true
            };

        var createResult =
            await userManager.CreateAsync(
                adminUser,
                adminPassword);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                createResult.Errors.Select(
                    error => error.Description));

            throw new InvalidOperationException(
                $"Başlangıç yöneticisi oluşturulamadı: {errors}");
        }

        var roleAssignmentResult =
            await userManager.AddToRoleAsync(
                adminUser,
                "Admin");

        if (!roleAssignmentResult.Succeeded)
        {
            await userManager.DeleteAsync(
                adminUser);

            var errors = string.Join(
                ", ",
                roleAssignmentResult.Errors.Select(
                    error => error.Description));

            throw new InvalidOperationException(
                $"Admin rolü atanamadı: {errors}");
        }
    }
}