using TaskTracker.Api.Entities;

namespace TaskTracker.Api.DTOs;

public record AssistantRequest(string Prompt);

public record AssistantTaskDraft(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate,
    string CategoryName,
    int? CategoryId);

public sealed class AssistantTaskDraftPayload
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string? DueDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
