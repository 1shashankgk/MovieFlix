using Microsoft.OpenApi.Models;
using MovieFlixBackend.Domain.Interfaces;
using MovieFlixBackend.Infrastructure.Repositories;
using MovieFlixBackend.Infrastructure.ExternalServices;
using MovieFlixBackend.Application.Interfaces;
using MovieFlixBackend.Application.Services;
using MovieFlixBackend.Presentation.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Dependency Injection setup
builder.Services.AddSingleton<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<OmdbClient>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .WithOrigins("https://movie-flix-again.vercel.app") // your Vercel frontend URL
            .AllowAnyHeader()
            .AllowAnyMethod());
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
        .WithOrigins("https://movie-flix-again.vercel.app/")
        .AllowAnyHeader()
        .AllowAnyMethod());
app.UseCors("AllowAll");
app.MapGet("/", () => Results.Ok("🎬 MovieFlix API is running! Visit /swagger for documentation."));
app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
