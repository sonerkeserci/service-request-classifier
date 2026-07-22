using RequestClassifier.Domain.Enums;

namespace RequestClassifier.Application.DTOs.ServiceRequests;

public class RequestStatusHistoryDto
{
    public int Id { get; set; }

    public RequestStatus? OldStatus { get; set; }

    public RequestStatus NewStatus { get; set; }

    public string? Description { get; set; }

    public DateTime ChangedAt { get; set; }
}