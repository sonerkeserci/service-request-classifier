namespace RequestClassifier.Application.DTOs.RequestCategories;

// This DTO is used for creating a new request category by administrator.
public class CreateRequestCategoryDto
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DepartmentId { get; set; }
}