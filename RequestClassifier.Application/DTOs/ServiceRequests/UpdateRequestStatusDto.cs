using RequestClassifier.Domain.Enums;

namespace RequestClassifier.Application.DTOs.ServiceRequests;

// DTO for updating the status of a service request by a personel or an admin, users cannot update the status of a service request

public class UpdateRequestStatusDto
{
    public RequestStatus NewStatus { get; set; }

    public string? Description { get; set; }
}