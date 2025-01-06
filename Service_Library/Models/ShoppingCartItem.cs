using System.ComponentModel.DataAnnotations;

namespace Service_Library.Models
{
    public class ShoppingCartItem
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
