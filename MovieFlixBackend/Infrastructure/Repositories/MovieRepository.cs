using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MovieFlixBackend.Domain.Entities;
using MovieFlixBackend.Domain.Interfaces;
using MongoDB.Bson;

namespace MovieFlixBackend.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IMongoCollection<Movie> _movies;

        public MovieRepository(IConfiguration config)
        {
            var client = new MongoClient(config["MovieDatabaseSettings:ConnectionString"]);
            var database = client.GetDatabase(config["MovieDatabaseSettings:DatabaseName"]);
            _movies = database.GetCollection<Movie>("MovieDatabaseSettings:Movies");
        }

        public async Task<Movie?> GetByImdbIdAsync(string imdbId)
        {
            var filter = Builders<Movie>.Filter.Eq(m => m.ImdbID, imdbId);
            return await _movies.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Movie?> FindByImdbIdAsync(string imdbId)
        {
            var filter = Builders<Movie>.Filter.Eq(m => m.ImdbID, imdbId);
            return await _movies.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Movie>> FindByTitleAsync(string title)
        {
            var filter = Builders<Movie>.Filter.Regex("Title", new BsonRegularExpression(title, "i"));
            return await _movies.Find(filter).ToListAsync();
        }

        public async Task UpsertAsync(Movie movie)
        {
            var filter = Builders<Movie>.Filter.Eq(m => m.ImdbID, movie.ImdbID);
            await _movies.ReplaceOneAsync(filter, movie, new ReplaceOptions { IsUpsert = true });
        }

        public async Task DeleteOlderThanAsync(DateTime cutoff)
        {
            var filter = Builders<Movie>.Filter.Lt(m => m.CreatedAt, cutoff);
            await _movies.DeleteManyAsync(filter);
        }
    }
}
