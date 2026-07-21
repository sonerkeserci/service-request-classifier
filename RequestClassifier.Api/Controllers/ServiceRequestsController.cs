using Microsoft.AspNetCore.Mvc;
using RequestClassifier.Application.DTOs.ServiceRequests;
using RequestClassifier.Application.Interfaces;

namespace RequestClassifier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _service;
    public ServiceRequestsController(IServiceRequestService service) // Receives the service implementation through dependency injection.
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() // Calls GetAllAsync from the service and returns all ServiceRequests.
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    // Calls GetByIdAsync to find a single request by its database Id.
    // Returns 404 if the request does not exist.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    // Sends the incoming DTO to CreateAsync.
    // The service creates and saves the request, then returns its details.
    [HttpPost]
    public async Task<IActionResult> Create (CreateServiceRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return Ok(result);
    }

    // Calls TrackAsync using the request number and requester email.
    // Returns the request details if both values match.
    [HttpPost("track")]
    public async Task<IActionResult> Track (TrackServiceRequestDto dto)
    {
        var result = await _service.TrackAsync(dto);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    // Calls UpdateStatusAsync to change the request status
    // and create a new status history record.
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateRequestStatusDto dto)
    {
        var result = await _service.UpdateStatusAsync(id, dto);
        if(!result)
            return NotFound();
        return NoContent();
    }

}