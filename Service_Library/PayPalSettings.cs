namespace Service_Library.Services
{
    public class PayPalSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Environment { get; set; } // "sandbox" or "live"
    }
}
