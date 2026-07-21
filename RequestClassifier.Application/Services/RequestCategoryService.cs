using Microsoft.EntityFrameworkCore;
using RequestClassifier.Application.DTOs.RequestCategories;
using RequestClassifier.Application.Interfaces;
using RequestClassifier.Domain.Entities;

namespace RequestClassifier.Application.Services;

public class RequestCategoryService : IRequestCategoryService
{
    private readonly IApplicationDbContext _context;

    public RequestCategoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RequestCategoryDto>> GetAllAsync()
    {
        return await _context.RequestCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new RequestCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                IsActive = c.IsActive,
                DepartmentId = c.DepartmentId,
                DepartmentName = c.Department.Name,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<RequestCategoryDto?> GetByIdAsync(int id)
    {
        return await _context.RequestCategories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new RequestCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                IsActive = c.IsActive,
                DepartmentId = c.DepartmentId,
                DepartmentName = c.Department.Name,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<RequestCategoryDto>> GetByDepartmentIdAsync(int departmentId)
    {
        return await _context.RequestCategories
            .AsNoTracking()
            .Where(c => c.DepartmentId == departmentId)
            .OrderBy(c => c.Name)
            .Select(c => new RequestCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                IsActive = c.IsActive,
                DepartmentId = c.DepartmentId,
                DepartmentName = c.Department.Name,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<RequestCategoryDto> CreateAsync(CreateRequestCategoryDto dto)
    {
        // Validate that the department exists before creating a new category
        var departmentExists = await _context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId); // Check by DepartmentId if the department exists in the Departments table

        if (!departmentExists)
            throw new InvalidOperationException(
                "The selected department does not exist.");

        var category = new RequestCategory
        {
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim().ToUpperInvariant(),
            Description = dto.Description?.Trim(),
            DepartmentId = dto.DepartmentId
        };

        _context.RequestCategories.Add(category);

        await _context.SaveChangesAsync(); // Save changes to the database to generate the Id for the new RequestCategory

        // Retrieve the department name for the newly created category
        var departmentName = await _context.Departments
            .Where(d => d.Id == category.DepartmentId)
            .Select(d => d.Name)
            .FirstAsync();

        return new RequestCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            IsActive = category.IsActive,
            DepartmentId = category.DepartmentId,
            DepartmentName = departmentName,
            CreatedAt = category.CreatedAt
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateRequestCategoryDto dto)
    {
        var category = await _context.RequestCategories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return false;

        var departmentExists = await _context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId);

        if (!departmentExists)
            throw new InvalidOperationException("The selected department does not exist.");

        category.Name = dto.Name.Trim();
        category.Code = dto.Code.Trim().ToUpperInvariant();
        category.Description = dto.Description?.Trim();
        category.DepartmentId = dto.DepartmentId;
        category.IsActive = dto.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}