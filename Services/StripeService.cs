using System.Text;
using System.Text.Json;

namespace PayGate.Services;

public class StripeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<StripeService> _logger;

    private string SecretKey => _config["Gateways:Stripe:SecretKey"]!;

    public StripeService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<StripeService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<(string PaymentUrl, string PaymentIntentId)> CreatePaymentAsync(
        string txnId,
        decimal amount,
        string currency,
        string callbackUrl)
    {
        var client = _httpClientFactory.CreateClient();

        var formData = new Dictionary<string, string>
        {
            { "amount", ((int)(amount * 100)).ToString() }, // Stripe uses cents
            { "currency", currency.ToLower() },
            { "metadata[transaction_id]", txnId },
            { "success_url", callbackUrl },
            { "cancel_url", callbackUrl }
        };

        var content = new FormUrlEncodedContent(formData);

        var request = new HttpRequestMessage(HttpMethod.Post, 
            "https://api.stripe.com/v1/payment_intents")
        {
            Headers = {
                { "Authorization", $"Bearer {SecretKey}" }
            },
            Content = content
        };

        var response = await client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Stripe payment failed: {Response}", responseText);
            throw new Exception("Stripe payment failed");
        }

        var result = JsonSerializer.Deserialize<JsonElement>(responseText);
        var paymentIntentId = result.GetProperty("id").GetString();
        var clientSecret = result.GetProperty("client_secret").GetString();

        // For simplicity, return Stripe Checkout URL (in real app, use Stripe.js on frontend)
        var paymentUrl = $"https://checkout.stripe.com/pay/{clientSecret}";

        return (paymentUrl, paymentIntentId!);
    }

    public async Task<bool> VerifyPaymentAsync(string paymentIntentId)
    {
        var client = _httpClientFactory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.stripe.com/v1/payment_intents/{paymentIntentId}")
        {
            Headers = {
                { "Authorization", $"Bearer {SecretKey}" }
            }
        };

        var response = await client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return false;

        var result = JsonSerializer.Deserialize<JsonElement>(responseText);
        var status = result.GetProperty("status").GetString();

        return status == "succeeded";
    }
}
