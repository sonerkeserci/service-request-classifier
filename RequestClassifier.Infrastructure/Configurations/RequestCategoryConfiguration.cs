using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RequestClassifier.Domain.Entities;

namespace RequestClassifier.Infrastructure.Configurations
{
    public class RequestCategoryConfiguration : IEntityTypeConfiguration<RequestCategory>
    {
        public void Configure(EntityTypeBuilder<RequestCategory> builder)
        {
            builder.ToTable("RequestCategories");

            builder.HasKey(rc => rc.Id);

            builder.Property(rc => rc.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(rc => rc.Code)
                .HasMaxLength(10);

            builder.Property(rc => rc.Description)
                .HasMaxLength(500);

            builder.HasIndex(rc => rc.Code)
                .IsUnique();

            builder.HasOne(rc => rc.Department)         // RequestCategory has one Department
                .WithMany(d => d.RequestCategories)     // Department has many RequestCategories
                .HasForeignKey(rc => rc.DepartmentId)   // Foreign key in RequestCategory pointing to Department
                .OnDelete(DeleteBehavior.Restrict);     // Prevent deletion of a department if it has associated request categories


        }
    }
}
