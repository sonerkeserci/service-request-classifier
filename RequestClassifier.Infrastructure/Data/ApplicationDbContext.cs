using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RequestClassifier.Domain.Entities;
using RequestClassifier.Application.Interfaces;

namespace RequestClassifier.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<RequestCategory> RequestCategories { get; set; } = null!;
        public DbSet<ServiceRequest> ServiceRequests { get; set; } = null!;
        public DbSet<RequestStatusHistory> RequestStatusHistories { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder) // This method allows further configuration of the models
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            // This line applies all configurations from the current assembly, allowing for a clean separation of configuration logic into separate classes.
            // You can also add additional configurations one by one here if needed. But it's better to keep the configurations in separate classes for better maintainability and readability.
            // It will automatically apply any Fluent API configurations defined in the assembly, such as entity configurations, relationships, constraints, etc.
        }

    }
}
