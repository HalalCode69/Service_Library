using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayPalHttp;
using System.Net;
using SystemTextJson = System.Text.Json.JsonSerializer;

namespace Service_Library.Services
{
    public class PayPalService
    {
        private readonly PayPalSettings _payPalSettings;
        private readonly ILogger<PayPalService> _logger;

        public PayPalService(IOptions<PayPalSettings> payPalSettings, ILogger<PayPalService> logger)
        {
            _payPalSettings = payPalSettings.Value;
            _logger = logger;
        }

        public PayPalEnvironment GetPayPalEnvironment()
        {
            return _payPalSettings.Environment.ToLower() == "live"
                ? new LiveEnvironment(_payPalSettings.ClientId, _payPalSettings.ClientSecret)
                : new SandboxEnvironment(_payPalSettings.ClientId, _payPalSettings.ClientSecret);
        }

        public async Task<string> CreateOrder(decimal amount, string currency, string returnUrl)
        {
            var environment = GetPayPalEnvironment();
            var client = new PayPalHttpClient(environment);

            var order = new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = currency,
                            Value = amount.ToString("F2")
                        }
                    }
                },
                ApplicationContext = new ApplicationContext
                {
                    ReturnUrl = returnUrl
                }
            };

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(order);

            var response = await client.Execute(request);
            var result = response.Result<Order>();

            return result.Links.FirstOrDefault(link => link.Rel == "approve")?.Href;
        }

        public async Task<bool> CompleteOrder(string orderId)
        {
            var environment = GetPayPalEnvironment();
            var client = new PayPalHttpClient(environment);

            var request = new OrdersCaptureRequest(orderId);
            request.RequestBody(new OrderActionRequest());

            try
            {
                var response = await client.Execute(request);
                return response.StatusCode == HttpStatusCode.Created;
            }
            catch (HttpException ex)
            {
                var error = SystemTextJson.Deserialize<PayPalError>(ex.Message);
                if (error?.Name == "UNPROCESSABLE_ENTITY" && error.Details.Any(d => d.Issue == "ORDER_ALREADY_CAPTURED"))
                {
                    // Log the error and return true as the order is already captured
                    _logger.LogWarning("Order already captured: {OrderId}", orderId);
                    return true;
                }
                throw;
            }
        }

        public class PayPalError
        {
            public string Name { get; set; }
            public List<PayPalErrorDetail> Details { get; set; }
        }

        public class PayPalErrorDetail
        {
            public string Issue { get; set; }
            public string Description { get; set; }
        }
    }
}
