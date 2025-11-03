using MovieFlixBackend.Application.ViewModels;

namespace MovieFlixBackend.Application.Interfaces
{
    public interface IMovieService
    {
        Task<List<MovieViewModel>> SearchMoviesAsync(
            string search,
            string? genre = null,
            int? year = null,
            double? minRating = null);
        Task<MovieViewModel?> GetMovieByIdAsync(string imdbId);
        Task RefreshCacheAsync();
    }
}
