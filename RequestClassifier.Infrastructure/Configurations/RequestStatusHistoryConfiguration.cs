using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RequestClassifier.Domain.Entities;

namespace RequestClassifier.Infrastructure.Configurations
{
    public class RequestStatusHistoryConfiguration : IEntityTypeConfiguration<RequestStatusHistory>
    {
        public void Configure(EntityTypeBuilder<RequestStatusHistory> builder)
        {
            builder.ToTable("RequestStatusHistories");

            builder.HasKey(rsh => rsh.Id);

            builder.HasOne(rsh => rsh.ServiceRequest)
                   .WithMany(sr => sr.StatusHistories)
                   .HasForeignKey(rsh => rsh.ServiceRequestId)
                   .OnDelete(DeleteBehavior.Cascade);   // Cascade delete: When a ServiceRequest is deleted, its related RequestStatusHistory records will also be deleted.

            // We dont configure enums like OldStatus or NewStatus here; because EF Core will automatically map them to their underlying integer values in the database.

            builder.Property(rsh => rsh.Description)
                .HasMaxLength(500);
        }
    }
}
