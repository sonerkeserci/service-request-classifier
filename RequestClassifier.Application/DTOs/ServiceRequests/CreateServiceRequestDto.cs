using System;
using System.Collections.Generic;
using System.Text;

namespace RequestClassifier.Application.DTOs.ServiceRequests;

public class CreateServiceRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string RequesterFirstName { get; set; } = string.Empty;

    public string RequesterLastName { get; set; } = string.Empty;

    public string RequesterEmail { get; set; } = string.Empty;

    public string? RequesterPhoneNumber { get; set; }
}
