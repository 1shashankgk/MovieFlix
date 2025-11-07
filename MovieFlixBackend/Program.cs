using Microsoft.OpenApi.Models;
using MovieFlixBackend.Domain.Interfaces;
using MovieFlixBackend.Infrastructure.Repositories;
using MovieFlixBackend.Infrastructure.ExternalServices;
using MovieFlixBackend.Application.Interfaces;
using MovieFlixBackend.Application.Services;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using MovieFlixBackend.Presentation.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Movies API",
        Version = "v1",
        Description = "🎬 MovieFlix Movies Endpoints"
    });
});




// Dependency Injection setup
builder.Services.AddSingleton<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<OmdbClient>();
builder.Services.AddHttpClient<OmdbClient>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieFlixBackend v1");
    c.RoutePrefix = string.Empty;
});
app.UseHttpsRedirection();

/*var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");*/

app.UseCors(policy =>
    policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapGet("/", () => Results.Ok("🎬 MovieFlix API is running! Visit /swagger for documentation."));
app.MapControllers();

var partManager = app.Services.GetRequiredService<ApplicationPartManager>();
foreach (var part in partManager.ApplicationParts)
{
    Console.WriteLine($"📦 Loaded assembly part: {part.Name}");
}

Console.WriteLine("✅ Controllers discovered:");
foreach (var feature in partManager.FeatureProviders)
{
    Console.WriteLine($"   👉 {feature.GetType().Name}");
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
