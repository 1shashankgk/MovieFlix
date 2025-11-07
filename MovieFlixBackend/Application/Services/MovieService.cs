using MovieFlixBackend.Application.Interfaces;
using MovieFlixBackend.Application.ViewModels;
using MovieFlixBackend.Domain.Entities;
using MovieFlixBackend.Domain.Interfaces;
using MovieFlixBackend.Infrastructure.ExternalServices;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace MovieFlixBackend.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly OmdbClient _omdbClient;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public MovieService(
            IMovieRepository movieRepository,
            OmdbClient omdbClient,
            IMapper mapper,
            IConfiguration config)
        {
            _movieRepository = movieRepository;
            _omdbClient = omdbClient;
            _mapper = mapper;
            _config = config;
        }

        public async Task<List<MovieViewModel>> SearchMoviesAsync(
    string search,
    string? genre = null,
    int? year = null,
    double? minRating = null)
        {
    
    if (string.IsNullOrWhiteSpace(search))
        return new();

    search = search.ToLowerInvariant();

    var cached = await _movieRepository.FindByTitleAsync(search);
    var results = cached.Any()
        ? cached
        : await FetchFromOmdbAndCacheAsync(search);

    if (!string.IsNullOrWhiteSpace(genre))
        results = results.Where(m =>
            m.Genre != null &&
            m.Genre.Any(g => !string.IsNullOrWhiteSpace(g) && g.IndexOf(genre, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();

    if (year.HasValue)
        results = results.Where(m => m.Year == year.Value.ToString()).ToList();

    if (minRating.HasValue)
        results = results.Where(m =>
        {
            if (double.TryParse(m.ImdbRating, out double rating))
                return rating >= minRating.Value;
            return false;
        }).ToList();

    return _mapper.Map<List<MovieViewModel>>(results);
}

private async Task<List<Movie>> FetchFromOmdbAndCacheAsync(string search)
{
            try
            {
                var apiKey = _config["Omdb:ApiKey"];
                Console.WriteLine($"🔍 Azure DEBUG: Omdb:ApiKey = {apiKey}");

                var ids = await _omdbClient.SearchMovieIdsAsync(search);
                var result = new List<Movie>();

                foreach (var id in ids)
                {
                    var existing = await _movieRepository.FindByImdbIdAsync(id);
                    if (existing != null)
                    {
                        result.Add(existing);
                        continue;
                    }

                    var movie = await _omdbClient.GetMovieDetailsAsync(id);
                    if (movie != null)
                    {
                        movie.ImdbID = id;
                        await _movieRepository.UpsertAsync(movie);
                        result.Add(movie);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Azure ERROR: Failed to fetch from OMDb - {ex.Message}");
                return new List<Movie>();
            }
}



        public async Task<MovieViewModel?> GetMovieByIdAsync(string imdbId)
        {
            // 1️⃣ Try cache first
            var cached = await _movieRepository.FindByImdbIdAsync(imdbId);
            if (cached != null)
                return _mapper.Map<MovieViewModel>(cached);

            // 2️⃣ Fetch fresh data from OMDb
            var movie = await _omdbClient.GetMovieDetailsAsync(imdbId);
            if (movie == null) return null;

            await _movieRepository.UpsertAsync(movie);
            return _mapper.Map<MovieViewModel>(movie);
        }

        public async Task RefreshCacheAsync()
        {
            int hours = int.Parse(_config["Cache:ExpiryHours"] ?? "24");
            var cutoff = DateTime.UtcNow.AddHours(-hours);
            await _movieRepository.DeleteOlderThanAsync(cutoff);
        }
    }
}
