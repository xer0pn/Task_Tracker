using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskState = TaskTracker.Api.Entities.TaskStatus;

namespace TaskTracker.Api.Services;

public interface ITaskService
{
    Task<TaskListResponse> GetAllAsync(string? status, int? categoryId, string? search, int page, int pageSize);
    Task<TaskResponse?> GetByIdAsync(int id);
    Task<TaskResponse> CreateAsync(TaskRequest request);
    Task<TaskResponse?> UpdateAsync(int id, TaskRequest request);
    Task<TaskResponse?> UpdateStatusAsync(int id, TaskState status);
    Task<bool> DeleteAsync(int id);
}
