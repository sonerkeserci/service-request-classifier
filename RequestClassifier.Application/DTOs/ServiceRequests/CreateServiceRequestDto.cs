using System;
using System.Collections.Generic;
using System.Text;

namespace RequestClassifier.Application.DTOs.ServiceRequests;

// DTO for creating a new service request
// This DTO is used to transfer data from the client to the server when creating a new service request
// It contains only the necessary information for creating a service request, such as title, description, and requester details to avoid exposing sensitive information
public class CreateServiceRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string RequesterFirstName { get; set; } = string.Empty;

    public string RequesterLastName { get; set; } = string.Empty;

    public string RequesterEmail { get; set; } = string.Empty;

    public string? RequesterPhoneNumber { get; set; }
}
