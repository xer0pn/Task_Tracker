using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;

namespace TaskTracker.Api.Services;

public sealed class AssistantService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ICategoryService categories) : IAssistantService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AssistantTaskDraft> CreateTaskDraftAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) throw new AssistantUnavailableException("The assistant is temporarily unavailable.");

        var categoryList = await categories.GetAllAsync();
        var categoryNames = categoryList.Select(category => category.Name).ToArray();
        var request = new
        {
            model = configuration["OpenAI:Model"] ?? Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini",
            input = new object[]
            {
                new { role = "system", content = new[] { new { type = "input_text", text = BuildSystemPrompt(categoryNames, prompt) } } },
                new { role = "user", content = new[] { new { type = "input_text", text = prompt.Trim() } } }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "task_draft",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        properties = new
                        {
                            title = new { type = "string" },
                            description = new { type = new[] { "string", "null" } },
                            priority = new { type = "string", @enum = new[] { "Low", "Medium", "High" } },
                            dueDate = new { type = new[] { "string", "null" } },
                            categoryName = new { type = "string" }
                        },
                        required = new[] { "title", "description", "priority", "dueDate", "categoryName" }
                    }
                }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await httpClientFactory.CreateClient().SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode) throw new AssistantUnavailableException("The assistant is temporarily unavailable.");

        using var responseDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var jsonText = FindOutputText(responseDocument.RootElement);
        if (string.IsNullOrWhiteSpace(jsonText)) throw new AssistantUnavailableException("The assistant returned an invalid response.");

        AssistantTaskDraftPayload payload;
        try
        {
            payload = JsonSerializer.Deserialize<AssistantTaskDraftPayload>(jsonText, JsonOptions)
                ?? throw new JsonException("Empty assistant response.");
        }
        catch (JsonException)
        {
            throw new AssistantUnavailableException("The assistant returned an invalid response.");
        }

        return AssistantDraftNormalizer.Normalize(payload, categoryList, prompt, DateTime.UtcNow);
    }

    private static string BuildSystemPrompt(IReadOnlyCollection<string> categoryNames, string userPrompt)
    {
        var outputLanguage = AssistantDraftNormalizer.OutputLanguage(userPrompt);
        return $"""
        You create a draft for a task tracker. Return only the requested JSON schema.
        Extract one concise task from the user's message. Never invent a category silently.
        All human-readable fields must be written in {outputLanguage}. Do not switch languages. JSON property names and priority enum values must remain exactly as defined by the schema.
        The existing categories are: {string.Join(", ", categoryNames)}.
        categoryName must be one of those names when possible; if the user asks for a category not in that list, return the user's category name unchanged so the application can ask them to choose an existing category.
        priority must be exactly one of Low, Medium, High. Normalize capitalization and synonyms: urgent/critical become High, normal/regular become Medium, and minor becomes Low.
        dueDate must be an ISO-8601 date or null. Resolve relative dates from today's date, {DateTime.UtcNow:yyyy-MM-dd}.
        Preserve the user's language, including Arabic, in title and description.
        """;
    }

    private static string? FindOutputText(JsonElement root)
    {
        if (!root.TryGetProperty("output", out var output)) return null;
        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content)) continue;
            foreach (var part in content.EnumerateArray())
            {
                if (part.TryGetProperty("type", out var type) && type.GetString() == "output_text" && part.TryGetProperty("text", out var text))
                    return text.GetString();
            }
        }
        return null;
    }
}
