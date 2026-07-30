using RequestClassifier.Domain.Enums;

namespace RequestClassifier.Application.DTOs.ServiceRequests;

public class CreateServiceRequestResultDto
{
    public string RequestNumber { get; set; } = string.Empty;

    public RequestStatus Status { get; set; }

    public string? AssignedDepartmentName { get; set; }

    public DateTime CreatedAt { get; set; }
}