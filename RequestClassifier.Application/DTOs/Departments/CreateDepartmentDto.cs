using System.ComponentModel.DataAnnotations;

namespace RequestClassifier.Application.DTOs.Departments;

// This DTO is used for creating a new department by administrator.
public class CreateDepartmentDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(5)]
    public string Code { get; set; } = string.Empty;

    [StringLength(250)] 
    public string? Description { get; set; }
}