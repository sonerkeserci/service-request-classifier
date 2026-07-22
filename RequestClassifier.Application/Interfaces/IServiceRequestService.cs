using RequestClassifier.Application.DTOs.ServiceRequests;

namespace RequestClassifier.Application.Interfaces;

public interface IServiceRequestService
{
    Task<ServiceRequestDetailDto> CreateAsync(CreateServiceRequestDto dto); // Input: CreateServiceRequestDto, Output: ServiceRequestDetailDto

    Task<ServiceRequestDetailDto?> TrackAsync(TrackServiceRequestDto dto);

    Task<ServiceRequestDetailDto?> GetByIdAsync(int id);    // Input: id, Output: ServiceRequestDetailDto

    Task<List<ServiceRequestDetailDto>> GetAllAsync();

    Task<bool> UpdateStatusAsync(int id, UpdateRequestStatusDto dto);   // Input: id, UpdateRequestStatusDto, Output: bool (true if successful, false if not)

    Task<List<RequestStatusHistoryDto>?> GetStatusHistoryAsync(int requestId);
}