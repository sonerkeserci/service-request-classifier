namespace RequestClassifier.Application.DTOs.ServiceRequests;

// Represents one category suggestion displayed to an employee.
public class CategoryPredictionCandidateDto
{
    // Contains the database identifier of the suggested category.
    // This value will be selected by the employee and sent back through AssignCategoryDto.
    public int CategoryId { get; set; } 

    // Contains the display name of the suggested category.
    public string CategoryName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    // Contains the score produced by the ML model for this category.
    public float Score { get; set; }
}