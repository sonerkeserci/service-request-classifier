namespace RequestClassifier.Application.DTOs.Departments;

// This DTO is used for updating an existing department by administrator.
public class UpdateDepartmentDto
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}