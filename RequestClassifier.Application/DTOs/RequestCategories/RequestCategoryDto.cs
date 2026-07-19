namespace RequestClassifier.Application.DTOs.RequestCategories;

// This DTO is used for transferring request category data between the application layers.
public class RequestCategoryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}