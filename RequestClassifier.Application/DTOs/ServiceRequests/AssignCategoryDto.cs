namespace RequestClassifier.Application.DTOs.ServiceRequests;

// Represents the category selected by an authorized employee for a specific service request.
public class AssignCategoryDto
{
    // Contains the database identifier of the category selected by the employee.
    public int CategoryId { get; set; }
}