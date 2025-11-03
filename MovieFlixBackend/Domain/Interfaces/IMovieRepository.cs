using MovieFlixBackend.Domain.Entities;

namespace MovieFlixBackend.Domain.Interfaces
{
    public interface IMovieRepository
    {
        Task<Movie?> FindByImdbIdAsync(string imdbId);

        Task<List<Movie>> FindByTitleAsync(string title);

        Task UpsertAsync(Movie movie);

        Task DeleteOlderThanAsync(DateTime cutoff);

        Task<Movie?> GetByImdbIdAsync(string imdbId);
    }
}
