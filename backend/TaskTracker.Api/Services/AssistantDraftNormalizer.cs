using System.Globalization;
using System.Text.RegularExpressions;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;

namespace TaskTracker.Api.Services;

public static partial class AssistantDraftNormalizer
{
    public static AssistantTaskDraft Normalize(AssistantTaskDraftPayload payload, IReadOnlyCollection<CategoryResponse> categories, string prompt, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(payload.Title) || payload.Title.Length > 200 || string.IsNullOrWhiteSpace(payload.CategoryName))
            throw new AssistantUnavailableException("The assistant returned an invalid task draft.");

        var categoryName = payload.CategoryName.Trim();
        var category = categories.FirstOrDefault(item => string.Equals(item.Name.Trim(), categoryName, StringComparison.OrdinalIgnoreCase));
        return new AssistantTaskDraft(
            payload.Title.Trim(),
            payload.Description?.Trim(),
            NormalizePriority(payload.Priority),
            ResolveDueDate(payload.DueDate, prompt, utcNow),
            categoryName,
            category?.Id);
    }

    public static string OutputLanguage(string prompt) => ContainsArabic(prompt) ? "Arabic" : "English";

    public static TaskPriority NormalizePriority(string value) => value.Trim().ToLowerInvariant() switch
    {
        "high" or "urgent" or "critical" => TaskPriority.High,
        "low" or "minor" => TaskPriority.Low,
        _ => TaskPriority.Medium
    };

    public static DateTime? ResolveDueDate(string? modelDate, string prompt, DateTime utcNow)
    {
        var relativeMatch = RelativeWeekdayRegex().Match(prompt);
        if (relativeMatch.Success && Enum.TryParse<DayOfWeek>(relativeMatch.Groups[1].Value, true, out var weekday))
        {
            var daysUntil = ((int)weekday - (int)utcNow.DayOfWeek + 7) % 7;
            return utcNow.Date.AddDays(daysUntil == 0 ? 7 : daysUntil + 7);
        }

        return DateTime.TryParse(modelDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date) ? date : null;
    }

    public static bool ContainsArabic(string value) => value.Any(character => character is >= '\u0600' and <= '\u06ff' or >= '\u0750' and <= '\u077f');

    [GeneratedRegex(@"\bnext\s+(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)\b", RegexOptions.IgnoreCase)]
    private static partial Regex RelativeWeekdayRegex();
}
