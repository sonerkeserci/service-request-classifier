namespace RequestClassifier.Application.DTOs.Departments;

// This DTO is used for creating a new department by administrator.
public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
}