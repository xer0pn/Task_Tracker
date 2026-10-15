using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Api.Data;

namespace TaskTracker.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            connection.Open();

            foreach (var descriptor in services.Where(item =>
                         item.ServiceType == typeof(DbContextOptions<TaskTrackerDbContext>) ||
                         item.ServiceType == typeof(TaskTrackerDbContext)).ToList())
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TaskTrackerDbContext>(options => options.UseSqlite(connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) connection.Dispose();
        base.Dispose(disposing);
    }
}
