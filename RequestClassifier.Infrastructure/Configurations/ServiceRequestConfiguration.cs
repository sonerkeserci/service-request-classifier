using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RequestClassifier.Domain.Entities;

namespace RequestClassifier.Infrastructure.Configurations
{
    public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
    {
        public void Configure(EntityTypeBuilder<ServiceRequest> builder)
        {
            builder.ToTable("ServiceRequests");

            builder.HasKey(sr => sr.Id);

            builder.Property(sr => sr.RequestNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(sr => sr.RequestNumber)
                .IsUnique();

            builder.Property(sr => sr.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sr => sr.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(sr => sr.RequesterFirstName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(sr => sr.RequesterLastName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(sr => sr.RequesterEmail)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(sr => sr.RequesterPhoneNumber)
                .HasMaxLength(25);

            // Configure the relationship with RequestCategory for PredictedCategory
            builder.HasOne(sr => sr.PredictedCategory)
                .WithMany(rc => rc.PredictedRequests)
                .HasForeignKey(sr => sr.PredictedCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the relationship with RequestCategory for AssignedCategory
            builder.HasOne(sr => sr.AssignedCategory)
                .WithMany(rc => rc.AssignedRequests)
                .HasForeignKey(sr => sr.AssignedCategoryId)
                .OnDelete(DeleteBehavior.Restrict);



        }
    }
}
