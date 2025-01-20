using System.ComponentModel.DataAnnotations;

namespace Service_Library.Models
{
    public class WebsiteIcon
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] IconData { get; set; }
        public string ContentType { get; set; }
    }
}
