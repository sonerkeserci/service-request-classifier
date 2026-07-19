using RequestClassifier.Domain.Enums;

namespace RequestClassifier.Application.DTOs.ServiceRequests;

// Reverse of the CreateServiceRequestDto, this DTO is used to transfer data from the server to the client when retrieving a service request's details.
// It contains all the information about a service request, including its status, predicted and assigned categories, prediction score, and timestamps.
// This is the API response DTO when a service request is saved.
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