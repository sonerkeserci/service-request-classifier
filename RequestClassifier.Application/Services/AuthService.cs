using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RequestClassifier.Application.DTOs.Auth;
using RequestClassifier.Application.Interfaces;
using RequestClassifier.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RequestClassifier.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IApplicationDbContext context, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        // Find the employee account by email address.
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null || !user.IsActive)
            return null;

        // Verify the submitted password against the stored password hash.
        var isPasswordValid =
            await _userManager.CheckPasswordAsync(user, dto.Password);

        if (!isPasswordValid)
            return null;

        // Get the role assigned to the authenticated employee.
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        if (role is null)
            return null;

        // Generate a signed JWT for the authenticated employee.
        var tokenResult = GenerateToken(user, role);

        return new AuthResponseDto
        {
            Token = tokenResult.Token,
            Expiration = tokenResult.Expiration,
            Email = user.Email!,
            Role = role
        };
    }

    public async Task<bool> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        // Prevent duplicate employee accounts with the same email address.
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);

        if (existingUser is not null)
            return false;

        // Validate the department when one is assigned to the employee.
        if (dto.DepartmentId.HasValue)
        {
            // Check whether at least one active department exists with the given ID.
            // AnyAsync returns true or false and does not load the department record.
            // Use FirstOrDefaultAsync when you need the actual department object.
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == dto.DepartmentId.Value && d.IsActive);

            if (!departmentExists)
                return false;
        }

        // Only supported employee roles may be assigned.
        var allowedRoles = new[] { "Admin", "Employee" };

        var normalizedRole = allowedRoles.FirstOrDefault(role => role.Equals(dto.Role, StringComparison.OrdinalIgnoreCase));

        if (normalizedRole is null)
            return false;

        var user = new ApplicationUser
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            DepartmentId = dto.DepartmentId,
            IsActive = true,
            EmailConfirmed = true
        };

        // Create the Identity user and securely hash the submitted password.
        var createResult = await _userManager.CreateAsync(user, dto.Password);

        if (!createResult.Succeeded)
            return false;

        // Create the role when it has not been added to the database yet.
        if (!await _roleManager.RoleExistsAsync(normalizedRole))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(normalizedRole));

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return false;
            }
        }

        // Assign the selected role to the newly created employee.
        var addRoleResult = await _userManager.AddToRoleAsync(user, normalizedRole);

        if (!addRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return false;
        }

        return true;
    }

    private (string Token, DateTime Expiration) GenerateToken(ApplicationUser user, string role)
    {
        var jwtSection = _configuration.GetSection("Jwt");

        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException(             // ?? = if key is null
                "JWT key is missing from configuration.");

        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is missing from configuration.");

        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is missing from configuration.");

        if (!int.TryParse(
                jwtSection["ExpirationMinutes"],
                out var expirationMinutes))
        {
            expirationMinutes = 60;
        }

        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

        // Store employee identity and authorization information in token claims.
        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email!),

            new(
                ClaimTypes.NameIdentifier,
                user.Id),

            new(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}"),

            new(
                ClaimTypes.Role,
                role),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }
}