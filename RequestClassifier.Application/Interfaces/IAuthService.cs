using RequestClassifier.Application.DTOs.Auth;

namespace RequestClassifier.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto); // Returns null if login fails; returns AuthResponseDto if login is successful

    Task<bool> CreateEmployeeAsync(CreateEmployeeDto dto);  // Returns true if employee creation is successful
}