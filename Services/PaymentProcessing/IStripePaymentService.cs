using Stripe;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace BankingPaymentsAPI.Services.PaymentProcessing
{
    public interface IStripePaymentService
    {
        /// <summary>
        /// Creates a Stripe PaymentIntent for the given amount and currency.
        /// </summary>
        /// <param name="amount">Amount in main currency unit (e.g., INR)</param>
        /// <param name="currency">Currency code, e.g., "INR"</param>
        /// <param name="receipt">Receipt or reference ID for tracking</param>
        /// <returns>The created PaymentIntent object</returns>
        PaymentIntent CreatePaymentIntent(decimal amount, string currency, string receipt);

        /// <summary>
        /// Fetches a Stripe PaymentIntent by its ID
        /// </summary>
        PaymentIntent GetPaymentIntent(string paymentIntentId);
    }
}
