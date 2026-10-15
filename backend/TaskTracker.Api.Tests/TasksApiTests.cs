using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace TaskTracker.Api.Tests;

public sealed class TasksApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;

    public TasksApiTests(ApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task GetTasks_ReturnsSeededTasksWithPagingMetadata()
    {
        var response = await client.GetFromJsonAsync<TaskListResponse>("/api/tasks?page=1&pageSize=2");

        Assert.NotNull(response);
        Assert.Equal(2, response.Items.Count);
        Assert.Equal(10, response.TotalCount);
        Assert.Equal(5, response.TotalPages);
    }

    [Fact]
    public async Task CreateTask_Returns201AndLocationHeader()
    {
        var title = $"API test task {Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            title,
            description = "Created by the API integration test.",
            status = "Todo",
            priority = "High",
            categoryId = 1
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/tasks/", response.Headers.Location!.ToString());

        var created = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);
        Assert.Equal(title, created.Title);
        Assert.Equal("Work", created.CategoryName);

        await client.DeleteAsync($"/api/tasks/{created.Id}");
    }

    [Fact]
    public async Task CreateTask_WithMissingCategory_Returns400()
    {
        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            title = "Invalid category task",
            status = "Todo",
            priority = "Medium",
            categoryId = 9999
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsUpdatedTask()
    {
        var response = await client.PatchAsJsonAsync("/api/tasks/3/status", new { status = "InProgress" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(updated);
        Assert.Equal("InProgress", updated.Status);
    }

    [Fact]
    public async Task CreateDuplicateCategory_Returns409()
    {
        var response = await client.PostAsJsonAsync("/api/categories", new { name = "Work" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private sealed record TaskListResponse(List<TaskResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
    private sealed record TaskResponse(int Id, string Title, string? Description, string Status, string Priority, DateTime? DueDate, int CategoryId, string CategoryName, DateTime CreatedAt, DateTime UpdatedAt);
}
