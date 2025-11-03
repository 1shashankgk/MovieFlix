using Microsoft.AspNetCore.Mvc;
using MovieFlixBackend.Application.Interfaces;

namespace MovieFlixBackend.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet("search")]
public async Task<IActionResult> Search(
    string search,
    string? genre = null,
    int? year = null,
    double? minRating = null)
{
    var movies = await _movieService.SearchMoviesAsync(search, genre, year, minRating);

    if (!movies.Any())
        return NotFound(new { message = "No results found" });

    return Ok(movies);
}


        [HttpGet("{imdbId}")]
        public async Task<IActionResult> Get(string imdbId)
        {
            var movie = await _movieService.GetMovieByIdAsync(imdbId);
            if (movie == null) return NotFound();
            return Ok(movie);
        }

        [HttpDelete("refresh-cache")]
        public async Task<IActionResult> RefreshCache()
        {
            await _movieService.RefreshCacheAsync();
            return Ok(new { message = "Cache refreshed" });
        }

        [HttpGet("getById")]
public async Task<IActionResult> GetById(string imdbId)
{
    if (string.IsNullOrWhiteSpace(imdbId))
        return BadRequest("IMDb ID is required");

    var movie = await _movieService.GetMovieByIdAsync(imdbId);

    if (movie == null)
        return NotFound($"No movie found for IMDb ID: {imdbId}");

    return Ok(movie);
}


    }
}
