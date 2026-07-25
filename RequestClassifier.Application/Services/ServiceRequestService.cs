using Microsoft.EntityFrameworkCore;
using RequestClassifier.Application.DTOs.ServiceRequests;
using RequestClassifier.Application.Interfaces;
using RequestClassifier.Domain.Entities;
using RequestClassifier.Domain.Enums;
using RequestClassifier.ML.Services;

namespace RequestClassifier.Application.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IApplicationDbContext _context;
    private readonly IServiceRequestPredictor _predictor;
    public ServiceRequestService(IApplicationDbContext context, IServiceRequestPredictor predictor)
    {
        _context = context;
        _predictor = predictor;
    }

    public async Task<ServiceRequestDetailDto> CreateAsync(CreateServiceRequestDto dto)
    {
        // Send the title and description to the trained model and receive the predicted category name and highest score.
        var predictionResult = _predictor.PredictCategory(
            dto.Title,
            dto.Description);

        // Find the active database category whose name matches the category name returned by the trained model.
        var predictedCategory = await _context.RequestCategories
            .FirstOrDefaultAsync(category =>
                category.IsActive &&
                category.Name == predictionResult.PredictedCategory);

        var serviceRequest = new ServiceRequest
        {
            RequestNumber = $"TMP-{Guid.NewGuid().ToString("N")[..8]}", // Temporary request number until the entity is saved and gets an real Id
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            RequesterFirstName = dto.RequesterFirstName.Trim(),
            RequesterLastName = dto.RequesterLastName.Trim(),
            RequesterEmail = dto.RequesterEmail.Trim().ToLowerInvariant(),
            RequesterPhoneNumber = dto.RequesterPhoneNumber?.Trim(),
            Status = RequestStatus.Received,

            // Store the database Id of the category predicted by the model.
            // The value remains null if no matching active category is found.
            PredictedCategoryId = predictedCategory?.Id,

            // Store the highest score returned by the model.
            PredictionScore = predictionResult.MaxScore,

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


        // Save the permanent request number and initial status history.
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

    public async Task<List<RequestStatusHistoryDto>?> GetStatusHistoryAsync(
    int requestId)
    {
        // Check whether the service request exists before retrieving its history.
        var requestExists = await _context.ServiceRequests
            .AnyAsync(r => r.Id == requestId);

        if (!requestExists)
            return null;

        // Retrieve all status changes for the selected service request.
        return await _context.RequestStatusHistories
            .Where(h => h.ServiceRequestId == requestId)
            .OrderBy(h => h.ChangedAt)
            .Select(h => new RequestStatusHistoryDto
            {
                Id = h.Id,
                OldStatus = h.OldStatus,
                NewStatus = h.NewStatus,
                Description = h.Description,
                ChangedAt = h.ChangedAt
            })
            .ToListAsync();
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