using Microsoft.EntityFrameworkCore;
using RequestClassifier.Domain.Entities;

namespace RequestClassifier.Application.Interfaces;

// Application layer services use IApplicationDbContext: Application layer use the database but it doesnt know where and how the data is stored
// and Infrastructure provides the real ApplicationDbContext

public interface IApplicationDbContext
{
    DbSet<Department> Departments { get; }      // only "get", because we don't want to set the DbSet, just use it to get the data from the database
    DbSet<RequestCategory> RequestCategories { get; }
    DbSet <ServiceRequest> ServiceRequests { get; }
    DbSet <RequestStatusHistory> RequestStatusHistories { get; }

    Task <int> SaveChangesAsync(CancellationToken cancellationToken = default);
}