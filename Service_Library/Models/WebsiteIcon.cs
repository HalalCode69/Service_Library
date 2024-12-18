using System.ComponentModel.DataAnnotations;

namespace Service_Library.Models
{
    public class WebsiteIcon
    {
        [Key]
        public int Id { get; set; } // Primary Key

        public string Name { get; set; } // Name of the image
        public byte[] IconData { get; set; } // Image data in bytes
        public string ContentType { get; set; } // e.g., "image/jpeg"
    }
}
