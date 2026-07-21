using Microsoft.EntityFrameworkCore;
using RequestClassifier.Application.DTOs.ServiceRequests;
using RequestClassifier.Application.Interfaces;
using RequestClassifier.Domain.Entities;
using RequestClassifier.Domain.Enums;

namespace RequestClassifier.Application.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IApplicationDbContext _context;

    public ServiceRequestService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceRequestDetailDto> CreateAsync(CreateServiceRequestDto dto)
    {
        var serviceRequest = new ServiceRequest
        {
            RequestNumber = $"TMP-{Guid.NewGuid().ToString("N")[..8]}", // Temporary request number until the entity is saved and gets an Id
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            RequesterFirstName = dto.RequesterFirstName.Trim(),
            RequesterLastName = dto.RequesterLastName.Trim(),
            RequesterEmail = dto.RequesterEmail.Trim().ToLowerInvariant(),
            RequesterPhoneNumber = dto.RequesterPhoneNumber?.Trim(),
            Status = RequestStatus.Received,
            IsAutoAssigned = false
        };

        _context.ServiceRequests.Add(serviceRequest);

        await _context.SaveChangesAsync(); // Save to generate the Id for the service request

        serviceRequest.RequestNumber = $"REQ-{DateTime.UtcNow.Year}-{serviceRequest.Id:D6}"; // Update the request number with the generated Id

        // Add initial status history entry
        serviceRequest.StatusHistories.Add(
            new RequestStatusHistory
            {
                OldStatus = null,
                NewStatus = RequestStatus.Received,
                Description = "The service request was received."
            });

        await _context.SaveChangesAsync();

        return MapToDetailDto(serviceRequest);
    }

    public async Task<ServiceRequestDetailDto?> TrackAsync(TrackServiceRequestDto dto)
    {
        var request = await _context.ServiceRequests
            .AsNoTracking()
            .Include(r => r.PredictedCategory)
            .Include(r => r.AssignedCategory)
            .FirstOrDefaultAsync(r => r.RequestNumber == dto.RequestNumber && r.RequesterEmail == dto.RequesterEmail.Trim().ToLowerInvariant());
        // FirstOrDefaultAsync will return the first matching request or null if no match is found

        return request is null
            ? null
            : MapToDetailDto(request);
    }

    public async Task<ServiceRequestDetailDto?> GetByIdAsync(int id)
    {
        var request = await _context.ServiceRequests
            .AsNoTracking()
            .Include(r => r.PredictedCategory)
            .Include(r => r.AssignedCategory)
            .FirstOrDefaultAsync(r => r.Id == id);

        return request is null
            ? null
            : MapToDetailDto(request);
    }

    public async Task<List<ServiceRequestDetailDto>> GetAllAsync()
    {
        return await _context.ServiceRequests
            .AsNoTracking()
            .Include(r => r.PredictedCategory)
            .Include(r => r.AssignedCategory)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ServiceRequestDetailDto // Projecting to DTO to avoid loading unnecessary data
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status,
                PredictedCategoryName =
                    r.PredictedCategory != null
                        ? r.PredictedCategory.Name
                        : null,
                AssignedCategoryName =
                    r.AssignedCategory != null
                        ? r.AssignedCategory.Name
                        : null,
                PredictionScore = r.PredictionScore,
                IsAutoAssigned = r.IsAutoAssigned,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(); // Execute the query and return the list of DTOs
    }

    public async Task<bool> UpdateStatusAsync(int id, UpdateRequestStatusDto dto)
    {
        var request = await _context.ServiceRequests
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request is null)
            return false;

        var oldStatus = request.Status;

        request.Status = dto.NewStatus;
        request.UpdatedAt = DateTime.UtcNow;

        var history = new RequestStatusHistory
        {
            ServiceRequestId = request.Id,
            OldStatus = oldStatus,
            NewStatus = dto.NewStatus,
            Description = dto.Description?.Trim()
        };

        _context.RequestStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        return true;
    }

    // Private helper method to map ServiceRequest entity to ServiceRequestDetailDto to avoid code duplication
    private static ServiceRequestDetailDto MapToDetailDto(ServiceRequest request)
    {
        return new ServiceRequestDetailDto
        {
            Id = request.Id,
            RequestNumber = request.RequestNumber,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            PredictedCategoryName =
                request.PredictedCategory?.Name,
            AssignedCategoryName =
                request.AssignedCategory?.Name,
            PredictionScore = request.PredictionScore,
            IsAutoAssigned = request.IsAutoAssigned,
            CreatedAt = request.CreatedAt
        };
    }
}