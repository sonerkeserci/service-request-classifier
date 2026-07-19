namespace RequestClassifier.Application.DTOs.RequestCategories;

// This DTO is used for updating an existing request category by administrator.
public class UpdateRequestCategoryDto
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DepartmentId { get; set; }

    public bool IsActive { get; set; }
}