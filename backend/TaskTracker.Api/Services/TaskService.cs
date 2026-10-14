using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskState = TaskTracker.Api.Entities.TaskStatus;

namespace TaskTracker.Api.Services;

public class TaskService(TaskTrackerDbContext db) : ITaskService
{
    public async Task<TaskListResponse> GetAllAsync(string? status, int? categoryId, string? search, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = db.Tasks.AsNoTracking().Include(task => task.Category).AsQueryable();
        if (Enum.TryParse<TaskState>(status, true, out var parsedStatus)) query = query.Where(task => task.Status == parsedStatus);
        if (categoryId.HasValue) query = query.Where(task => task.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(task => task.Title.Contains(search.Trim()));

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(task => task.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(task => ToResponse(task))
            .ToListAsync();

        return new TaskListResponse(items, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<TaskResponse?> GetByIdAsync(int id)
    {
        var task = await db.Tasks.AsNoTracking().Include(item => item.Category).SingleOrDefaultAsync(item => item.Id == id);
        return task is null ? null : ToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(TaskRequest request)
    {
        var now = DateTime.UtcNow;
        var task = new TaskItem { Title = request.Title.Trim(), Description = request.Description?.Trim(), Status = request.Status, Priority = request.Priority, DueDate = request.DueDate, CategoryId = request.CategoryId, CreatedAt = now, UpdatedAt = now };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        await db.Entry(task).Reference(item => item.Category).LoadAsync();
        return ToResponse(task);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, TaskRequest request)
    {
        var task = await db.Tasks.Include(item => item.Category).SingleOrDefaultAsync(item => item.Id == id);
        if (task is null) return null;

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.CategoryId = request.CategoryId;
        task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await db.Entry(task).Reference(item => item.Category).LoadAsync();
        return ToResponse(task);
    }

    public async Task<TaskResponse?> UpdateStatusAsync(int id, TaskState status)
    {
        var task = await db.Tasks.Include(item => item.Category).SingleOrDefaultAsync(item => item.Id == id);
        if (task is null) return null;
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ToResponse(task);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null) return false;
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return true;
    }

    private static TaskResponse ToResponse(TaskItem task) => new(task.Id, task.Title, task.Description, task.Status, task.Priority, task.DueDate, task.CategoryId, task.Category.Name, task.CreatedAt, task.UpdatedAt);
}
