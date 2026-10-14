using TaskTracker.Api.DTOs;

namespace TaskTracker.Api.Services;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> CreateAsync(CategoryRequest request);
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsAsync(int id);
}
