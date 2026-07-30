namespace RequestClassifier.Application.DTOs.Auth;

public class UpdateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; }
}