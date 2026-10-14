using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Api.DTOs;

public class CategoryRequest
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}

public record CategoryResponse(int Id, string Name, int TaskCount);
