using RequestClassifier.Domain.Enums;

namespace RequestClassifier.Application.DTOs.ServiceRequests;

public class TrackServiceRequestResultDto
{
    public string RequestNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public RequestStatus Status { get; set; }

    public string? AssignedDepartmentName { get; set; }

    public DateTime CreatedAt { get; set; }
}