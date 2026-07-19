namespace RequestClassifier.Application.DTOs.Auth;

// This DTO will use AppllicationUser for login, so we only need email and password
// Password will be hashed and compared with the hashed password in the database
public class LoginDto
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}