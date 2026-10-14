using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Entities;
using TaskState = TaskTracker.Api.Entities.TaskStatus;

namespace TaskTracker.Api.Data;

public class TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(category => category.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(category => category.Name).IsUnique();
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(task => task.Title).HasMaxLength(200).IsRequired();
            entity.Property(task => task.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(task => task.Priority).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(task => task.Category)
                .WithMany(category => category.Tasks)
                .HasForeignKey(task => task.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        var categories = new[]
        {
            new Category { Id = 1, Name = "Work" },
            new Category { Id = 2, Name = "Personal" },
            new Category { Id = 3, Name = "Learning" }
        };

        modelBuilder.Entity<Category>().HasData(categories);

        var createdAt = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem { Id = 1, Title = "Prepare project brief", Description = "Write the first version of the product brief.", Status = TaskState.Done, Priority = TaskPriority.High, CategoryId = 1, CreatedAt = createdAt, UpdatedAt = createdAt },
            new TaskItem { Id = 2, Title = "Review pull request", Status = TaskState.InProgress, Priority = TaskPriority.High, CategoryId = 1, CreatedAt = createdAt, UpdatedAt = createdAt },
            new TaskItem { Id = 3, Title = "Plan next sprint", Status = TaskState.Todo, Priority = TaskPriority.Medium, CategoryId = 1, CreatedAt = createdAt, UpdatedAt = createdAt },
            new TaskItem { Id = 4, Title = "Book dentist appointment", Status = TaskState.Todo, Priority = TaskPriority.Medium, CategoryId = 2, CreatedAt = createdAt, UpdatedAt = createdAt },
            new TaskItem { Id = 5, Title = "Buy groceries", Status = TaskState.Done, Priority = TaskPriority.Low, CategoryId = 2, CreatedAt = createdAt, UpdatedAt = createdAt },
            new TaskItem { Id = 6, Title = "Organize home office", Status = TaskState.Todo, Priority = TaskPriority.Low, CategoryId = 2, CreatedAt = createdAt, UpdatedAt = createdAt },
            new TaskItem { Id = 7, Title = "Read EF Core documentation", Status = TaskState.InProgress, Priority = TaskPriority.Medium, CategoryId = 3, CreatedAt = createdAt, UpdatedAt = createdAt },
            new TaskItem { Id = 8, Title = "Practice Vue forms", Status = TaskState.Done, Priority = TaskPriority.Medium, CategoryId = 3, CreatedAt = createdAt, UpdatedAt = createdAt },
            new TaskItem { Id = 9, Title = "Study REST conventions", Status = TaskState.Todo, Priority = TaskPriority.High, CategoryId = 3, CreatedAt = createdAt, UpdatedAt = createdAt },
            new TaskItem { Id = 10, Title = "Write technical notes", Status = TaskState.Todo, Priority = TaskPriority.Low, CategoryId = 3, CreatedAt = createdAt, UpdatedAt = createdAt });
    }
}
