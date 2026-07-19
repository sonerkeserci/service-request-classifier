namespace RequestClassifier.Application.DTOs.Departments;

// This DTO is used for transferring department data between layers of the application.
// Users and administrators can view department details using this DTO.
public class DepartmentDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}