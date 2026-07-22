using Microsoft.EntityFrameworkCore;
using RequestClassifier.Infrastructure.Data;

namespace RequestClassifier.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Departments.AnyAsync()) // If any data has added, return
            return;

        /*
        Departments and RequestCategories will be added here.
        This method will be called once in Program.cs, then disabled.
        */
    }
}