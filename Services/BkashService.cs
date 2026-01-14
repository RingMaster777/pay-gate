using System.Text;
using System.Text.Json;

namespace PayGate.Services;

public class BkashService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<BkashService> _logger;

    private string BaseUrl => _config["Gateways:Bkash:BaseUrl"]!;
    private string AppKey => _config["Gateways:Bkash:AppKey"]!;
    private string AppSecret => _config["Gateways:Bkash:AppSecret"]!;
    private string Username => _config["Gateways:Bkash:Username"]!;
    private string Password => _config["Gateways:Bkash:Password"]!;

    public BkashService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<BkashService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<(string PaymentUrl, string PaymentId)> CreatePaymentAsync(
        string txnId,
        decimal amount,
        string orderId,
        string callbackUrl)
    {
        var token = await GetTokenAsync();
        var client = _httpClientFactory.CreateClient();

        var request = new
        {
            mode = "0011",
            payerReference = " ",
            callbackURL = callbackUrl,
            amount = amount.ToString("F2"),
            currency = "BDT",
            intent = "sale",
            merchantInvoiceNumber = orderId
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/create")
        {
            Headers = {
                { "Authorization", $"Bearer {token}" },
                { "X-APP-Key", AppKey }
            },
            Content = content
        };

        var response = await client.SendAsync(httpRequest);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("bKash create payment failed: {Response}", responseText);
            throw new Exception("bKash payment failed");
        }

        var result = JsonSerializer.Deserialize<JsonElement>(responseText);
        var paymentUrl = result.GetProperty("bkashURL").GetString();
        var paymentId = result.GetProperty("paymentID").GetString();

        return (paymentUrl!, paymentId!);
    }

    public async Task<bool> VerifyPaymentAsync(string paymentId)
    {
        var token = await GetTokenAsync();
        var client = _httpClientFactory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{BaseUrl}/payment/status/{paymentId}")
        {
            Headers = {
                { "Authorization", $"Bearer {token}" },
                { "X-APP-Key", AppKey }
            }
        };

        var response = await client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return false;

        var result = JsonSerializer.Deserialize<JsonElement>(responseText);
        var status = result.GetProperty("transactionStatus").GetString();

        return status?.ToLower() == "completed";
    }

    private async Task<string> GetTokenAsync()
    {
        var client = _httpClientFactory.CreateClient();

        var request = new
        {
            app_key = AppKey,
            app_secret = AppSecret
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/token/grant")
        {
            Headers = {
                { "username", Username },
                { "password", Password }
            },
            Content = content
        };

        var response = await client.SendAsync(httpRequest);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to get bKash token");

        var result = JsonSerializer.Deserialize<JsonElement>(responseText);
        return result.GetProperty("id_token").GetString()!;
    }
}
