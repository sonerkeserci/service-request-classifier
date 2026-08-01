using System.ComponentModel.DataAnnotations;

namespace RequestClassifier.Application.DTOs.Departments;

// This DTO is used for updating an existing department by administrator.
public class UpdateDepartmentDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(5)]
    public string Code { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}