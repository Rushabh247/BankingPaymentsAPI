using Stripe;

namespace BankingPaymentsAPI.Services.PaymentProcessing
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _config;

        public StripePaymentService(IConfiguration config)
        {
            _config = config;

            // Initialize Stripe API with secret key
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        // Create a PaymentIntent
        public PaymentIntent CreatePaymentIntent(decimal amount, string currency, string receipt)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // smallest currency unit
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" }, // only card enabled
                Metadata = new Dictionary<string, string>
                {
                    { "receipt", receipt }
                }
            };

            var service = new PaymentIntentService();
            return service.Create(options);
        }

        // Retrieve a PaymentIntent
        public PaymentIntent GetPaymentIntent(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return service.Get(paymentIntentId);
        }

        // ✅ Confirm a PaymentIntent for testing
        public PaymentIntent ConfirmPaymentIntent(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = "pm_card_visa" // Stripe test card
            };
            return service.Confirm(paymentIntentId, options);
        }
    }
}
