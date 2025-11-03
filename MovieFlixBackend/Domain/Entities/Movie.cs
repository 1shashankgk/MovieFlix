using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MovieFlixBackend.Domain.Entities
{
    public class Movie
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string ImdbID { get; set; } = ""; 

        [BsonElement("Title")]
        public string Title { get; set; } = "";

        [BsonElement("Year")]
        public string Year { get; set; } = "";

        [BsonElement("Genre")]
        public List<string> Genre { get; set; } = new();

        [BsonElement("Director")]
        public string Director { get; set; } = "";

        [BsonElement("Actors")]
        public List<string> Actors { get; set; } = new();

        [BsonElement("Rating")]
        public double Rating { get; set; }

        [BsonElement("Runtime")]
        public int Runtime { get; set; }

        [BsonElement("Plot")]
        public string Plot { get; set; } = "";

        [BsonElement("CachedAt")]
        public DateTime CachedAt { get; set; }

        [BsonElement("imdbRating")]
        public string? ImdbRating { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
