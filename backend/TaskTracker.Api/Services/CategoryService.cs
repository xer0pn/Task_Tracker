using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;

namespace TaskTracker.Api.Services;

public class CategoryService(TaskTrackerDbContext db) : ICategoryService
{
    public async Task<IReadOnlyCollection<CategoryResponse>> GetAllAsync() => await db.Categories.AsNoTracking().OrderBy(category => category.Name).Select(category => new CategoryResponse(category.Id, category.Name, category.Tasks.Count)).ToListAsync();

    public async Task<CategoryResponse?> CreateAsync(CategoryRequest request)
    {
        var category = new Category { Name = request.Name.Trim() };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return new CategoryResponse(category.Id, category.Name, 0);
    }

    public Task<bool> ExistsByNameAsync(string name) => db.Categories.AnyAsync(category => category.Name.ToLower() == name.Trim().ToLower());

    public Task<bool> ExistsAsync(int id) => db.Categories.AnyAsync(category => category.Id == id);
}
