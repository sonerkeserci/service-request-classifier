using RequestClassifier.Domain.Enums;

namespace RequestClassifier.Application.DTOs.ServiceRequests;

public class UpdateRequestStatusDto
{
    public RequestStatus NewStatus { get; set; }

    public string? Description { get; set; }
}