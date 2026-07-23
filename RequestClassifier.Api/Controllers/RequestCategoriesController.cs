using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequestClassifier.Application.DTOs.RequestCategories;
using RequestClassifier.Application.Interfaces;

namespace RequestClassifier.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestCategoriesController : ControllerBase
    {
        private readonly IRequestCategoryService _service;
        public RequestCategoriesController(IRequestCategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result is null)
                return NotFound();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateRequestCategoryDto dto)
        {
            var result = await _service.CreateAsync(dto);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update (UpdateRequestCategoryDto dto, int id)
        {
            var result = await _service.UpdateAsync(id,dto);

            if (!result)
                return NotFound();
            return NoContent();               
        }

    }
}
