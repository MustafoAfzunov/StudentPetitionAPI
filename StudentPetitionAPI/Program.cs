using Microsoft.EntityFrameworkCore;
using Serilog;
using StudentPetitionAPI.Infrastructure.Data;
using StudentPetitionAPI.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting StudentPetitionAPI");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // DbContext (SQLite)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Repositories
    builder.Services.AddRepositories();

    // Services
    builder.Services.AddApplicationServices();

    // AutoMapper
    builder.Services.AddMapping();

    // FluentValidation
    builder.Services.AddValidation();

    // Authentication + Authorization (JWT)
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // Localization (en-US / MM/DD/YYYY dates)
    builder.Services.AddAppLocalization();

    // Controllers + JSON options
    builder.Services.AddApiControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger
    builder.Services.AddSwaggerDocumentation();

    var app = builder.Build();

    // SQLite migrations
    app.ApplySqliteMigrations();

    // Global exception middleware
    app.UseGlobalExceptionHandling();

    // Serilog request logging
    app.UseRequestLogging();

    // Localization
    app.UseAppLocalization();

    // Swagger
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Petition API v1");
            options.DocumentTitle = "Student Petition API";
            options.DisplayRequestDuration();
        });
    }

    app.UseHttpsRedirection();

    // Authentication / Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "StudentPetitionAPI terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
