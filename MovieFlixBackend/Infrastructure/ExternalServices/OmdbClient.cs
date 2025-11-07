using MovieFlixBackend.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Linq;
using System.Text.Json;

namespace MovieFlixBackend.Infrastructure.ExternalServices
{
    public class OmdbClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OmdbClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["OmdbApiKey"] ?? throw new Exception("OMDb API key missing!");
        }

        public async Task<Movie?> GetMovieDetailsAsync(string imdbId)
        {
            var url = $"https://www.omdbapi.com/?apikey={_apiKey}&i={imdbId}";
            var responseString = await _httpClient.GetStringAsync(url);

            using var jsonDoc = JsonDocument.Parse(responseString);
            var root = jsonDoc.RootElement;

            // 🛡️ Handle invalid or failed responses safely
            if (!root.TryGetProperty("Response", out var resp) || resp.GetString()?.ToLower() != "true")
                return null;

            string GetStringSafe(string propName)
                => root.TryGetProperty(propName, out var el) ? el.GetString() ?? "" : "";

            string[] SplitList(string input)
                => string.IsNullOrWhiteSpace(input) ? Array.Empty<string>() :
                   input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var movie = new Movie
            {
                ImdbID = GetStringSafe("imdbID"),
                Title = GetStringSafe("Title"),
                Year = GetStringSafe("Year"),
                Genre = SplitList(GetStringSafe("Genre")).ToList(),
                Director = GetStringSafe("Director"),
                Actors = SplitList(GetStringSafe("Actors")).ToList(),
                Rating = double.TryParse(GetStringSafe("imdbRating"), out var rating) ? rating : 0,
                Runtime = int.TryParse(GetStringSafe("Runtime").Replace("min", "").Trim(), out var runtime) ? runtime : 0,
                Plot = GetStringSafe("Plot"),
                CachedAt = DateTime.UtcNow
            };

            return movie;
        }



        public async Task<List<string>> SearchMovieIdsAsync(string search)
        {
            Console.WriteLine($"🔍 OMDb request URL: https://www.omdbapi.com/?apikey={_apiKey}&s={search}");
            Console.WriteLine($"🔍 API Key loaded: {_apiKey}");

            var url = $"https://www.omdbapi.com/?apikey={_apiKey}&s={Uri.EscapeDataString(search)}";
            var responseString = await _httpClient.GetStringAsync(url);

            using var jsonDoc = JsonDocument.Parse(responseString);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("Response", out var responseProp) ||
                responseProp.GetString()?.ToLower() != "true")
            {
                return new List<string>();
            }

            if (!root.TryGetProperty("Search", out var searchResults))
                return new List<string>();

            var ids = new List<string>();
            foreach (var item in searchResults.EnumerateArray())
            {
                if (item.TryGetProperty("imdbID", out var idProp))
                {
                    ids.Add(idProp.GetString() ?? string.Empty);
                }
            }

            return ids;
        }
    }
}
