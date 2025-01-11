namespace Service_Library.Models
{
    public class Genre
    {
        public int GenreId { get; set; }
        public string Name { get; set; }

        // Navigation property for the many-to-many relationship
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
