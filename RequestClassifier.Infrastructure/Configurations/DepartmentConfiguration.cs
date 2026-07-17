using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RequestClassifier.Domain.Entities;

namespace RequestClassifier.Infrastructure.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department> //This interface asks for the implementation of the Configure method, which is used to configure the defined entity such as Department in this case.
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments");

            builder.HasKey(d => d.Id);  // Primary key configuration for the Department entity, specifying that the Id property is the primary key.

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(d => d.Code)
                .IsRequired()
                .HasMaxLength(5);

            builder.Property(d => d.Description)
                .HasMaxLength(500);

            builder.HasIndex(d => d.Code)   // This line creates a unique index on the Code property of the Department entity, ensuring that each department has a unique code.
                .IsUnique();
        }

    }
}
