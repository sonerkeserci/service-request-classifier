using RequestClassifier.Application.DTOs.ServiceRequests;

namespace RequestClassifier.Application.Interfaces;

public interface IServiceRequestService
{
    Task<CreateServiceRequestResultDto> CreateAsync(CreateServiceRequestDto dto); // Input: CreateServiceRequestDto, Output: ServiceRequestDetailDto

    Task<TrackServiceRequestResultDto?> TrackAsync(TrackServiceRequestDto dto);

    Task<ServiceRequestDetailDto?> GetByIdAsync(int id);    // Input: id, Output: ServiceRequestDetailDto

    Task<List<ServiceRequestDetailDto>> GetAllAsync();

    Task<bool> UpdateStatusAsync(int id, UpdateRequestStatusDto dto);   // Input: id, UpdateRequestStatusDto, Output: bool (true if successful, false if not)

    Task<List<RequestStatusHistoryDto>?> GetStatusHistoryAsync(int requestId);

    // Returns the five category candidates with the highest model scores for the specified service request.
    Task<List<CategoryPredictionCandidateDto>> GetPredictionCandidatesAsync(int id);

    // Assigns the category selected by an employee to the service request.
    Task<bool> AssignCategoryAsync(int id, AssignCategoryDto dto);
}