using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequestClassifier.Application.DTOs.Auth;
using RequestClassifier.Application.Interfaces;

namespace RequestClassifier.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _service.LoginAsync(dto);

            if (result is null)
                return Unauthorized();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeDto dto)
        {
            var result = await _service.CreateEmployeeAsync(dto);
            if (!result)
                return BadRequest();

            return StatusCode(StatusCodes.Status201Created);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var result = await _service.GetEmployeesAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("employees/{id}")]
        public async Task<IActionResult> UpdateEmployee(string id, UpdateEmployeeDto dto)
        {
            var result = await _service.UpdateEmployeeAsync(id, dto);

            if (!result)
                return BadRequest();

            return NoContent();
        }
    }
}
