using Microsoft.EntityFrameworkCore;
using RequestClassifier.Application.DTOs.Departments;
using RequestClassifier.Application.Interfaces;
using RequestClassifier.Domain.Entities;

namespace RequestClassifier.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IApplicationDbContext _context;

    public DepartmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        return await _context.Departments
            .AsNoTracking()                 // Use AsNoTracking for read-only queries to improve performance
            .OrderBy(d => d.Name)           // Order the results by the Name property of the Department entity
            .Select(d => new DepartmentDto  // Project the Department entity to a DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();     // Execute the query asynchronously and return the results as a list of DepartmentDto
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.Id == id) // Filter the results to only include the department with the specified Id
            .Select(d => new DepartmentDto  // Project the Department entity to a DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt
            })
            .FirstOrDefaultAsync(); // Execute the query asynchronously and return the first matching DepartmentDto or null if not found
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto) // Create asynchronously a new DepartmentDto based on the provided CreateDepartmentDto
    {
        var department = new Department
        {
            Name = dto.Name.Trim(), // Trim whitespace from the Name property of the CreateDepartmentDto. " Fen İşleri " becomes "Fen İşleri"
            Code = dto.Code.Trim().ToUpperInvariant(), //ToUpperInvariant() converts the string to uppercase
            Description = dto.Description?.Trim()
        };

        _context.Departments.Add(department); // Add the new Department entity to the Departments DbSet in the IApplicationDbContext

        await _context.SaveChangesAsync();  // Save the changes to the database asynchronously, which will generate a new Id for the Department entity

        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Description = department.Description,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
            return false;

        department.Name = dto.Name.Trim();
        department.Code = dto.Code.Trim().ToUpperInvariant();
        department.Description = dto.Description?.Trim();
        department.IsActive = dto.IsActive;
        department.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}