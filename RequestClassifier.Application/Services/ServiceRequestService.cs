using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RequestClassifier.Application.DTOs.ServiceRequests;
using RequestClassifier.Application.Interfaces;
using RequestClassifier.Application.Settings;
using RequestClassifier.Domain.Entities;
using RequestClassifier.Domain.Enums;
using RequestClassifier.ML.Services;

namespace RequestClassifier.Application.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IApplicationDbContext _context;
    private readonly IServiceRequestPredictor _predictor;
    private readonly MachineLearningSettings _machineLearningSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ServiceRequestService(
        IApplicationDbContext context,
        IServiceRequestPredictor predictor,
        IOptions<MachineLearningSettings> machineLearningOptions,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _predictor = predictor;
        _machineLearningSettings = machineLearningOptions.Value;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<CreateServiceRequestResultDto> CreateAsync(CreateServiceRequestDto dto)
    {
        // Send the title and description to the trained model and receive the predicted category name and highest score.
        var predictionResult = _predictor.PredictCategory(
            dto.Title,
            dto.Description);

        // Find the active database category whose name matches the category name returned by the trained model.
        var predictedCategory = await _context.RequestCategories
            .Include(category => category.Department)
            .FirstOrDefaultAsync(category =>
                category.IsActive &&
                category.Name == predictionResult.PredictedCategory);

        var shouldAutoAssign =
            predictedCategory != null &&
            (predictionResult.MaxScore >= _machineLearningSettings.AutoAssignmentScoreThreshold) &&
            (predictionResult.ScoreMargin >= _machineLearningSettings.AutoAssignmentMarginThreshold);

        var serviceRequest = new ServiceRequest
        {
            RequestNumber = $"TMP-{Guid.NewGuid().ToString("N")[..8]}", // Temporary request number until the entity is saved and gets an real Id
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            RequesterFirstName = dto.RequesterFirstName.Trim(),
            RequesterLastName = dto.RequesterLastName.Trim(),
            RequesterEmail = dto.RequesterEmail.Trim().ToLowerInvariant(),
            RequesterPhoneNumber = dto.RequesterPhoneNumber?.Trim(),

            // Every new request first enters the Received status.
            Status = RequestStatus.Received,

            // Store the category predicted by the ML model.
            PredictedCategoryId = predictedCategory?.Id,

            // Store the highest score produced by the model.
            PredictionScore = predictionResult.MaxScore,

            // Store the difference between the first and second model scores.
            PredictionScoreMargin = predictionResult.ScoreMargin,

            // Assignment is applied after the first database save.
            AssignedCategoryId = null,
            IsAutoAssigned = false
        };

        _context.ServiceRequests.Add(serviceRequest);

        // Save the request first so the database generates its ID.
        await _context.SaveChangesAsync();

        serviceRequest.RequestNumber = $"REQ-{DateTime.UtcNow.Year}-{serviceRequest.Id:D6}"; // Update the request number with the generated Id

        // Record that the request was first received by the system.
        serviceRequest.StatusHistories.Add(
            new RequestStatusHistory
            {
                OldStatus = null,
                NewStatus = RequestStatus.Received,
                Description = "The service request was received.",
                ChangedAt = DateTime.UtcNow
            });

        // Apply automatic assignment when both thresholds are met.
        if (shouldAutoAssign)
        {
            // Assign the predicted category automatically.
            serviceRequest.AssignedCategoryId = predictedCategory!.Id;
            serviceRequest.AssignedCategory = predictedCategory;

            // Mark that the assignment was made by the system.
            serviceRequest.IsAutoAssigned = true;

            // Move the request directly to the Assigned status in DB.
            serviceRequest.Status = RequestStatus.Assigned;

            // Record the automatic assignment.
            serviceRequest.StatusHistories.Add(
                new RequestStatusHistory
                {
                    OldStatus = RequestStatus.Received,
                    NewStatus = RequestStatus.Assigned,
                    Description =
                        $"The request was automatically assigned to " +
                        $"'{predictedCategory!.Department.Name}/{predictedCategory.Name}'.",
                    ChangedAt = DateTime.UtcNow
                });
        }
        else
        {
            // Leave the request for employee review.
            serviceRequest.AssignedCategoryId = null;
            serviceRequest.IsAutoAssigned = false;
            serviceRequest.Status = RequestStatus.Classified;

            // Record that the prediction requires employee review.
            serviceRequest.StatusHistories.Add(
                new RequestStatusHistory
                {
                    OldStatus = RequestStatus.Received,
                    NewStatus = RequestStatus.Classified,
                    Description =
                        $"The request was classified as '{predictedCategory?.Name ?? "Unknown"}' and left for employee review.",
                    ChangedAt = DateTime.UtcNow
                });
        }

        serviceRequest.UpdatedAt = DateTime.UtcNow;

        // Save the permanent request number, final status,
        // assignment result and both history records.
        await _context.SaveChangesAsync();

        return new CreateServiceRequestResultDto
        {
            RequestNumber = serviceRequest.RequestNumber,
            Status = serviceRequest.Status,
            AssignedDepartmentName = serviceRequest.AssignedCategory?.Department?.Name,
            CreatedAt = serviceRequest.CreatedAt
        };
    }

    public async Task<TrackServiceRequestResultDto?> TrackAsync(TrackServiceRequestDto dto)
    {
        var request = await _context.ServiceRequests
            .AsNoTracking()
            .Include(r => r.PredictedCategory)
                .ThenInclude(category => category!.Department)
            .Include(r => r.AssignedCategory)
                .ThenInclude(category => category!.Department)
            .FirstOrDefaultAsync(r => r.RequestNumber == dto.RequestNumber && r.RequesterEmail == dto.RequesterEmail.Trim().ToLowerInvariant());
        // FirstOrDefaultAsync will return the first matching request or null if no match is found

        return request is null
            ? null
            : new TrackServiceRequestResultDto
            {
                RequestNumber = request.RequestNumber,
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                AssignedDepartmentName = request.AssignedCategory?.Department?.Name,
                CreatedAt = request.CreatedAt,
            };
    }

    public async Task<ServiceRequestDetailDto?> GetByIdAsync(int id)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var isEmployee = user?.IsInRole("Employee") == true;

        var departmentIdClaim = user?.FindFirst("departmentId")?.Value;

        int? employeeDepartmentId = int.TryParse(
            departmentIdClaim,
            out var parsedDepartmentId)
                ? parsedDepartmentId
                : null;

        var query = _context.ServiceRequests
            .AsNoTracking()
            .Include(request => request.PredictedCategory)
                .ThenInclude(category => category!.Department)
            .Include(request => request.AssignedCategory)
                .ThenInclude(category => category!.Department)
            .AsQueryable();

        if (isEmployee)
        {
            if (!employeeDepartmentId.HasValue)
            {
                return null;
            }

            query = query.Where(request =>
                request.AssignedCategory != null &&
                request.AssignedCategory.DepartmentId == employeeDepartmentId.Value);
        }

        var request = await query
            .FirstOrDefaultAsync(request => request.Id == id);

        return request is null
            ? null
            : MapToDetailDto(request);
    }

    public async Task<List<ServiceRequestDetailDto>> GetAllAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var isEmployee = user?.IsInRole("Employee") == true;

        var departmentIdClaim = user?.FindFirst("departmentId")?.Value;

        int? employeeDepartmentId = int.TryParse(
            departmentIdClaim,
            out var parsedDepartmentId)
                ? parsedDepartmentId
                : null;

        var query = _context.ServiceRequests
            .AsNoTracking()
            .Include(request => request.PredictedCategory)
                .ThenInclude(category => category!.Department)
            .Include(request => request.AssignedCategory)
                .ThenInclude(category => category!.Department)
            .AsQueryable();

        if (isEmployee)
        {
            if (!employeeDepartmentId.HasValue)
            {
                return new List<ServiceRequestDetailDto>();
            }

            query = query.Where(request =>
                request.AssignedCategory != null &&
                request.AssignedCategory.DepartmentId == employeeDepartmentId.Value);
        }

        return await query
            .OrderByDescending(request => request.CreatedAt)
            .Select(request => new ServiceRequestDetailDto
            {
                Id = request.Id,
                RequestNumber = request.RequestNumber,
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,

                PredictedCategoryId = request.PredictedCategoryId,
                PredictedCategoryName =
                    request.PredictedCategory != null
                        ? request.PredictedCategory.Name
                        : null,

                AssignedCategoryName =
                    request.AssignedCategory != null
                        ? request.AssignedCategory.Name
                        : null,

                PredictionScore = request.PredictionScore,
                PredictionScoreMargin = request.PredictionScoreMargin,
                IsAutoAssigned = request.IsAutoAssigned,
                CreatedAt = request.CreatedAt,

                PredictedDepartmentId =
                    request.PredictedCategory != null
                        ? request.PredictedCategory.DepartmentId
                        : null,

                PredictedDepartmentName =
                    request.PredictedCategory != null
                        ? request.PredictedCategory.Department.Name
                        : null,

                AssignedDepartmentId =
                    request.AssignedCategory != null
                        ? request.AssignedCategory.DepartmentId
                        : null,

                AssignedDepartmentName =
                    request.AssignedCategory != null
                        ? request.AssignedCategory.Department.Name
                        : null,

                RequesterFirstName = request.RequesterFirstName,
                RequesterLastName = request.RequesterLastName,
                RequesterEmail = request.RequesterEmail,
                RequesterPhoneNumber = request.RequesterPhoneNumber,


            })
            .ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(int id, UpdateRequestStatusDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var isEmployee = user?.IsInRole("Employee") == true;

        var departmentIdClaim = user?
            .FindFirst("departmentId")?
            .Value;

        int? employeeDepartmentId = int.TryParse(
            departmentIdClaim,
            out var parsedDepartmentId)
                ? parsedDepartmentId
                : null;

        var query = _context.ServiceRequests
            .Include(request => request.AssignedCategory)
            .AsQueryable();

        if (isEmployee)
        {
            if (!employeeDepartmentId.HasValue)
            {
                return false;
            }

            query = query.Where(request =>
                request.AssignedCategory != null &&
                request.AssignedCategory.DepartmentId ==
                    employeeDepartmentId.Value);
        }

        var request = await query
            .FirstOrDefaultAsync(request => request.Id == id);

        if (request is null)
        {
            return false;
        }

        var oldStatus = request.Status;

        request.Status = dto.NewStatus;
        request.UpdatedAt = DateTime.UtcNow;

        var history = new RequestStatusHistory
        {
            ServiceRequestId = request.Id,
            OldStatus = oldStatus,
            NewStatus = dto.NewStatus,
            Description = dto.Description?.Trim(),
            ChangedAt = DateTime.UtcNow
        };

        _context.RequestStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<List<RequestStatusHistoryDto>?> GetStatusHistoryAsync(int requestId)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var isEmployee = user?.IsInRole("Employee") == true;

        var departmentIdClaim = user?
            .FindFirst("departmentId")?
            .Value;

        int? employeeDepartmentId = int.TryParse(
            departmentIdClaim,
            out var parsedDepartmentId)
                ? parsedDepartmentId
                : null;

        var requestQuery = _context.ServiceRequests
            .AsNoTracking()
            .Include(request => request.AssignedCategory)
            .AsQueryable();

        if (isEmployee)
        {
            if (!employeeDepartmentId.HasValue)
            {
                return null;
            }

            requestQuery = requestQuery.Where(request =>
                request.AssignedCategory != null &&
                request.AssignedCategory.DepartmentId ==
                    employeeDepartmentId.Value);
        }

        var requestExists = await requestQuery
            .AnyAsync(request => request.Id == requestId);

        if (!requestExists)
        {
            return null;
        }

        return await _context.RequestStatusHistories
            .AsNoTracking()
            .Where(history =>
                history.ServiceRequestId == requestId)
            .OrderBy(history => history.ChangedAt)
            .Select(history => new RequestStatusHistoryDto
            {
                Id = history.Id,
                OldStatus = history.OldStatus,
                NewStatus = history.NewStatus,
                Description = history.Description,
                ChangedAt = history.ChangedAt
            })
            .ToListAsync();
    }
    public async Task<List<CategoryPredictionCandidateDto>> GetPredictionCandidatesAsync(int id)
    {
        // Find the request by its database ID.
        var serviceRequest = await _context.ServiceRequests
            .AsNoTracking() // Read-only query. EF Core does not track this entity.
            .Where(request => request.Id == id) // Keep only the request with the given ID.
            .Select(request => new ServiceRequest
            {
                // Load only the fields needed by the ML model.
                Title = request.Title,
                Description = request.Description
            })
            .FirstOrDefaultAsync(); // Return the first match, or null if no request exists.

        // Stop and return an empty list when the request cannot be found.
        if (serviceRequest == null)
        {
            return [];
        }

        // Send the saved title and description to the model again.
        // The result contains the five categories with the highest scores.
        var predictionResult = _predictor.PredictCategory(
            serviceRequest.Title,
            serviceRequest.Description);

        // Take only the category names from the model candidates.
        // Example: ["Kırsal Yol ve Altyapı", "Yol, Asfalt ve Kaldırım"]
        var candidateNames = predictionResult.TopCandidates
            .Select(candidate => candidate.CategoryName) // Convert each candidate into its name.
            .ToList(); // Convert the result into a normal List<string>.

        // Find the database category records matching the model's category names.
        // This provides the real CategoryId values needed by the frontend.
        var categories = await _context.RequestCategories
            .AsNoTracking()
            .Where(category =>
                category.IsActive && // Ignore inactive categories.
                candidateNames.Contains(category.Name)) // Keep names found in the Top 5 list.
            .ToListAsync();

        // Convert each ML candidate into the DTO returned by the API.
        var candidateDtos = predictionResult.TopCandidates
            .Select(candidate =>
            {
                // Find the database category with the same name as this ML candidate.
                var category = categories.FirstOrDefault(
                    item => item.Name == candidate.CategoryName);

                // Combine the database ID with the model name and score.
                return new CategoryPredictionCandidateDto
                {
                    // Use 0 when no matching database category is found.
                    CategoryId = category?.Id ?? 0,

                    // Take the category name from the ML result.
                    CategoryName = candidate.CategoryName,

                    // Take the score from the ML result.
                    Score = candidate.Score
                };
            })
            // Remove candidates that could not be matched with a database category.
            .Where(candidate => candidate.CategoryId != 0)
            .ToList();

        // Return the final Top 5 candidate list to the controller.
        return candidateDtos;
    }
    public async Task<bool> AssignCategoryAsync(int id, AssignCategoryDto dto)
    {
        // Load the service request that will be updated.
        var serviceRequest = await _context.ServiceRequests.
            FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest is null)
            return false;

        // Load the selected active category.
        var category = await _context.RequestCategories.
            FirstOrDefaultAsync(rc => rc.Id == dto.CategoryId && rc.IsActive);

        if (category is null)
            return false;


        // Store the current status before changing it.
        var oldStatus = serviceRequest.Status;

        // Assign the employee-selected category to the service request.
        serviceRequest.AssignedCategoryId = category.Id;

        // Mark the assignment as manual because an employee selected it.
        serviceRequest.IsAutoAssigned = false;

        // Update the request status after category assignment.
        serviceRequest.Status = RequestStatus.Assigned;

        // Update the modification date.
        serviceRequest.UpdatedAt = DateTime.UtcNow;

        // Record the assignment and status change in the history table.
        _context.RequestStatusHistories.
            Add(new RequestStatusHistory
            {
                ServiceRequestId = serviceRequest.Id,
                OldStatus = oldStatus,
                NewStatus = RequestStatus.Assigned,
                Description =
                $"Category manually assigned as '{category.Name}'.",
                ChangedAt = DateTime.UtcNow

            });

        // Save all changes in one database operation.
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
            // Map the predicted category identifier to the response DTO.
            PredictedCategoryId = request.PredictedCategoryId,
            AssignedCategoryName =
                request.AssignedCategory?.Name,
            PredictionScore = request.PredictionScore,
            PredictionScoreMargin = request.PredictionScoreMargin,
            IsAutoAssigned = request.IsAutoAssigned,
            CreatedAt = request.CreatedAt,
            PredictedDepartmentId = request.PredictedCategory?.DepartmentId,
            PredictedDepartmentName = request.PredictedCategory?.Department?.Name,
            AssignedDepartmentId = request.AssignedCategory?.DepartmentId,
            AssignedDepartmentName = request.AssignedCategory?.Department?.Name,
            RequesterFirstName = request.RequesterFirstName,
            RequesterLastName = request.RequesterLastName,
            RequesterEmail = request.RequesterEmail,
            RequesterPhoneNumber= request.RequesterPhoneNumber,
        };
    }
}