using RequestClassifier.Application.DTOs.RequestCategories;

namespace RequestClassifier.Application.Interfaces;

public interface IRequestCategoryService
{
    Task<List<RequestCategoryDto>> GetAllAsync();

    Task<RequestCategoryDto?> GetByIdAsync(int id);

    Task<List<RequestCategoryDto>> GetByDepartmentIdAsync(int departmentId); // Get all request categories for a specific department

    Task<RequestCategoryDto> CreateAsync(CreateRequestCategoryDto dto);

    Task<bool> UpdateAsync(int id, UpdateRequestCategoryDto dto);
}