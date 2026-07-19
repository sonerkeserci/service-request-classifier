namespace RequestClassifier.Application.DTOs.Auth;

// When administrator creates a new employee, this DTO will be used to send data to the API

// CreaeteEmployeeDto goes to the API
// AuthService will create a new ApplicationUser
// Identity will hash the password and store it in the database
// New role will be assigned to the new user
// New user will be assigned to a department with the given DepartmentId
public class CreateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }
}