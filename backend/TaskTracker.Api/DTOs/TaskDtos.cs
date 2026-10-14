using System.ComponentModel.DataAnnotations;
using TaskTracker.Api.Entities;
using TaskState = TaskTracker.Api.Entities.TaskStatus;

namespace TaskTracker.Api.DTOs;

public class TaskRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskState Status { get; set; } = TaskState.Todo;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

public record TaskResponse(
    int Id,
    string Title,
    string? Description,
    TaskState Status,
    TaskPriority Priority,
    DateTime? DueDate,
    int CategoryId,
    string CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TaskListResponse(
    IReadOnlyCollection<TaskResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public class StatusRequest
{
    [Required]
    public TaskState? Status { get; set; }
}
