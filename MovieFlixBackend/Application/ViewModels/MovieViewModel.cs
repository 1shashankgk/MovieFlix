namespace MovieFlixBackend.Application.ViewModels
{
    public class MovieViewModel
    {
        public string ImdbID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public List<string> Genre { get; set; } = new();
        public string Director { get; set; } = string.Empty;
        public List<string> Actors { get; set; } = new();
        public double Rating { get; set; }
        public int Runtime { get; set; }
        public string Plot { get; set; } = string.Empty;
    }
}
