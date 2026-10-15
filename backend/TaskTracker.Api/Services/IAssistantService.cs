using TaskTracker.Api.DTOs;

namespace TaskTracker.Api.Services;

public interface IAssistantService
{
    Task<AssistantTaskDraft> CreateTaskDraftAsync(string prompt, CancellationToken cancellationToken = default);
}

public sealed class AssistantUnavailableException(string message) : Exception(message);
