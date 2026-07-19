namespace RequestClassifier.Application.DTOs.ServiceRequests;

// DTO for tracking a service request
// Users use this DTO to track the status of a service request by providing the request number and their email address
// Service ll search for requests where request.RequestNumber == dto.RequestNumber && request.RequesterEmail == dto.RequesterEmail
// Then return a ServiceRequestDetailDto if found, otherwise return a not found response
public class TrackServiceRequestDto
{
    public string RequestNumber { get; set; } = string.Empty;

    public string RequesterEmail { get; set; } = string.Empty;
}