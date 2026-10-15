using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace TaskTracker.Api.Tests;

public sealed class AssistantApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;

    public AssistantApiTests(ApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task CreateTaskDraft_WithBlankPrompt_Returns400()
    {
        var response = await client.PostAsJsonAsync("/api/assistant/tasks", new { prompt = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
