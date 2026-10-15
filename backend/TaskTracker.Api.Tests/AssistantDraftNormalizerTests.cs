using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskTracker.Api.Services;
using Xunit;

namespace TaskTracker.Api.Tests;

public sealed class AssistantDraftNormalizerTests
{
    private static readonly DateTime Thursday = new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly CategoryResponse[] Categories = [new(1, "Work", 0), new(2, "Personal", 0)];

    [Theory]
    [InlineData("HIGH", TaskPriority.High)]
    [InlineData("urgent", TaskPriority.High)]
    [InlineData("normal", TaskPriority.Medium)]
    [InlineData("minor", TaskPriority.Low)]
    public void NormalizePriority_HandlesCasingAndSynonyms(string value, TaskPriority expected) => Assert.Equal(expected, AssistantDraftNormalizer.NormalizePriority(value));

    [Fact]
    public void ResolveDueDate_ResolvesNextWeekdayFromKnownDate()
    {
        var dueDate = AssistantDraftNormalizer.ResolveDueDate("2026-10-20", "Prepare notes next Friday", Thursday);

        Assert.Equal(new DateTime(2026, 10, 23), dueDate!.Value.Date);
    }

    [Fact]
    public void Normalize_LeavesUnknownCategoryUnassigned()
    {
        var draft = AssistantDraftNormalizer.Normalize(new AssistantTaskDraftPayload
        {
            Title = "Interview notes",
            Priority = "high",
            CategoryName = "Applications"
        }, Categories, "Prepare interview notes", Thursday);

        Assert.Null(draft.CategoryId);
        Assert.Equal("Applications", draft.CategoryName);
        Assert.Equal(TaskPriority.High, draft.Priority);
    }

    [Fact]
    public void OutputLanguage_MatchesInputScript()
    {
        Assert.Equal("English", AssistantDraftNormalizer.OutputLanguage("Prepare interview notes"));
        Assert.Equal("Arabic", AssistantDraftNormalizer.OutputLanguage("أنشئ مهمة للمقابلة"));
    }
}
