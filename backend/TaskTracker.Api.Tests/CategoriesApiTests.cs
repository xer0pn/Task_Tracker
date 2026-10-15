using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace TaskTracker.Api.Tests;

public sealed class CategoriesApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;

    public CategoriesApiTests(ApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task GetCategories_ReturnsTaskCounts()
    {
        var categories = await client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");

        Assert.NotNull(categories);
        Assert.Contains(categories, category => category.Name == "Work" && category.TaskCount == 3);
        Assert.Contains(categories, category => category.Name == "Personal" && category.TaskCount == 3);
        Assert.Contains(categories, category => category.Name == "Learning" && category.TaskCount == 4);
    }

    [Fact]
    public async Task CreateCategory_Returns201AndLocationHeader()
    {
        var name = $"API Test {Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/api/categories", new { name });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/categories/4", response.Headers.Location?.ToString());

        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(category);
        Assert.Equal(name, category.Name);
        Assert.Equal(0, category.TaskCount);
    }

    [Fact]
    public async Task CreateCategory_WithBlankName_Returns400()
    {
        var response = await client.PostAsJsonAsync("/api/categories", new { name = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record CategoryResponse(int Id, string Name, int TaskCount);
}
