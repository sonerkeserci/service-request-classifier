using RequestClassifier.Domain.Enums;

namespace RequestClassifier.Application.DTOs.ServiceRequests;

public class ServiceRequestDetailDto
{
    public int Id { get; set; }

    public string RequestNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public RequestStatus Status { get; set; }

    public string? PredictedCategoryName { get; set; }

    public string? AssignedCategoryName { get; set; }

    public float? PredictionScore { get; set; }

    public bool IsAutoAssigned { get; set; }

    public DateTime CreatedAt { get; set; }
}