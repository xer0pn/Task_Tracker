using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskTracker.Api.Services;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController(ITaskService service, ICategoryService categories) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TaskListResponse>> GetAll([FromQuery] string? status, [FromQuery] int? categoryId, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) => Ok(await service.GetAllAsync(status, categoryId, search, page, pageSize));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id) => (await service.GetByIdAsync(id)) is { } task ? Ok(task) : NotFound(new { message = "Task not found." });

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(TaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest(new { message = "Title is required." });
        if (!await CategoryExists(request.CategoryId)) return BadRequest(new { message = "The selected category does not exist." });
        var task = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, TaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest(new { message = "Title is required." });
        if (!await CategoryExists(request.CategoryId)) return BadRequest(new { message = "The selected category does not exist." });
        return (await service.UpdateAsync(id, request)) is { } task ? Ok(task) : NotFound(new { message = "Task not found." });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(int id, StatusRequest request) => (await service.UpdateStatusAsync(id, request.Status!.Value)) is { } task ? Ok(task) : NotFound(new { message = "Task not found." });

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => await service.DeleteAsync(id) ? NoContent() : NotFound(new { message = "Task not found." });

    private Task<bool> CategoryExists(int categoryId) => categories.ExistsAsync(categoryId);
}
