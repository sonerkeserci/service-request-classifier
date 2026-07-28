using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet]
    public async Task<IActionResult> GetAll() // Calls GetAllAsync from the service and returns all ServiceRequests.
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    // Calls GetByIdAsync to find a single request by its database Id.
    // Returns 404 if the request does not exist.
    [Authorize(Roles = "Admin,Employee")]
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
    public async Task<IActionResult> Create(CreateServiceRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);

        // nameof(GetById): defines which endpoint can retrieve the newly created service request.
        // new { id = result.Id }: provides the route parameter required by the GetById endpoint.
        // result: becomes the response body containing the created service request.
        // CreatedAtAction also returns HTTP 201 Created and generates a Location header such as /api/ServiceRequests/11.
    }

    // Calls TrackAsync using the request number and requester email.
    // Returns the request details if both values match.
    [HttpPost("track")]
    public async Task<IActionResult> Track(TrackServiceRequestDto dto)
    {
        var result = await _service.TrackAsync(dto);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    // Calls UpdateStatusAsync to change the request status
    // and create a new status history record.
    [Authorize(Roles = "Admin,Employee")]
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, UpdateRequestStatusDto dto)
    {
        var result = await _service.UpdateStatusAsync(id, dto);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("{id:int}/histories")]
    public async Task<IActionResult> GetStatusHistory(int id)
    {
        var history = await _service.GetStatusHistoryAsync(id);

        if (history == null) return NotFound();
        return Ok(history);
    }

    // Returns the five strongest category suggestions for the specified request.
    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("{id:int}/prediction-candidates")]
    public async Task<IActionResult> GetPredictionCandidates(int id)
    {
        //Ask the application service for the model's category suggestion.
        var candidates = await _service.GetPredictionCandidatesAsync(id);

        // Return 404 when the request does not exist or no candidate could be produced.
        if (candidates.Count == 0)
            return NotFound();

        // Return the candidate list with HTTP 200.
        return Ok(candidates);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpPut("{id:int}/assign")]
    public async Task<IActionResult> AssignCategory(int id, AssignCategoryDto dto)
    {
        // Send the request ID and selected category to the application service.
        var isAssigned = await _service.AssignCategoryAsync(id, dto);

        // Return HTTP 400 when the request or selected category is invalid.
        if (!isAssigned)
        {
            return BadRequest("The service request or selected category is invalid.");
        }

        // Return HTTP 204 because the assignment succeeded and no response body is required.
        return NoContent();
    }
}