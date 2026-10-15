using System.Net;
using System.Net.Http.Json;
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
    public async Task GetTasks_CanFilterByStatusCategoryAndSearch()
    {
        var statusResponse = await client.GetFromJsonAsync<TaskListResponse>("/api/tasks?status=Done&pageSize=50");
        var categoryResponse = await client.GetFromJsonAsync<TaskListResponse>("/api/tasks?categoryId=1&pageSize=50");
        var searchResponse = await client.GetFromJsonAsync<TaskListResponse>("/api/tasks?search=pull&pageSize=50");

        Assert.NotNull(statusResponse);
        Assert.All(statusResponse.Items, task => Assert.Equal("Done", task.Status));
        Assert.NotNull(categoryResponse);
        Assert.All(categoryResponse.Items, task => Assert.Equal(1, task.CategoryId));
        Assert.NotNull(searchResponse);
        Assert.Single(searchResponse.Items);
        Assert.Equal("Review pull request", searchResponse.Items[0].Title);
    }

    [Fact]
    public async Task GetTask_ReturnsTaskById()
    {
        var task = await client.GetFromJsonAsync<TaskResponse>("/api/tasks/1");

        Assert.NotNull(task);
        Assert.Equal("Prepare project brief", task.Title);
        Assert.Equal("Work", task.CategoryName);
    }

    [Fact]
    public async Task GetMissingTask_Returns404()
    {
        var response = await client.GetAsync("/api/tasks/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
    public async Task CreateTask_WithBlankTitle_Returns400()
    {
        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            title = "   ",
            status = "Todo",
            priority = "Medium",
            categoryId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_ReturnsUpdatedTask()
    {
        var created = await CreateTask();

        var response = await client.PutAsJsonAsync($"/api/tasks/{created.Id}", new
        {
            title = "Updated API test task",
            description = "Updated description",
            status = "InProgress",
            priority = "Low",
            dueDate = "2026-11-01T09:00:00Z",
            categoryId = 2
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Updated API test task", updated.Title);
        Assert.Equal("Personal", updated.CategoryName);
        Assert.Equal("Low", updated.Priority);

        await client.DeleteAsync($"/api/tasks/{created.Id}");
    }

    [Fact]
    public async Task UpdateMissingTask_Returns404()
    {
        var response = await client.PutAsJsonAsync("/api/tasks/99999", new
        {
            title = "Missing task",
            status = "Todo",
            priority = "Medium",
            categoryId = 1
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
    public async Task UpdateMissingStatus_Returns404()
    {
        var response = await client.PatchAsJsonAsync("/api/tasks/99999/status", new { status = "Done" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_Returns204AndThen404()
    {
        var created = await CreateTask();

        var deleteResponse = await client.DeleteAsync($"/api/tasks/{created.Id}");
        var secondDeleteResponse = await client.DeleteAsync($"/api/tasks/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteMissingTask_Returns404()
    {
        var response = await client.DeleteAsync("/api/tasks/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateDuplicateCategory_Returns409()
    {
        var response = await client.PostAsJsonAsync("/api/categories", new { name = "Work" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task<TaskResponse> CreateTask()
    {
        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            title = $"API test task {Guid.NewGuid():N}",
            description = "Temporary task for an integration test.",
            status = "Todo",
            priority = "Medium",
            categoryId = 1
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskResponse>())!;
    }

    private sealed record TaskListResponse(List<TaskResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
    private sealed record TaskResponse(int Id, string Title, string? Description, string Status, string Priority, DateTime? DueDate, int CategoryId, string CategoryName, DateTime CreatedAt, DateTime UpdatedAt);
}
