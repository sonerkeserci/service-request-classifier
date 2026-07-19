namespace RequestClassifier.Application.DTOs.Auth;

// This DTO will be used to return the token and expiration date to the client after successful login
// JWT token will be generated using the email and role of the user
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }    // Expiration date of the JWT token

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;    // Admin or Employee
}