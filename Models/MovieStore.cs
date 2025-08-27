using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gen_ai_api_agents.Models
{
    // Simple static data store
    public static class MovieStore
    {
        public static List<Movie> Movies = new() {
        new Movie { Title = "Inception", Cinema = "Grand Cinema", Time = "7:00 PM", Price = 15 },
        new Movie { Title = "The Matrix", Cinema = "Cityplex", Time = "9:00 PM", Price = 13 }
    };
        public static ConcurrentBag<string> PurchasedTickets = new();
    }

    public class Movie
    {
        public string Title { get; set; } = "";
        public string Cinema { get; set; } = "";
        public string Time { get; set; } = "";
        public decimal Price { get; set; }
    }
}
