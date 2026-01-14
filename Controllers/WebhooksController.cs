using PayGate.Services;
using Microsoft.AspNetCore.Mvc;

namespace PayGate.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly WebhookService _webhookService;

    public WebhooksController(WebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    [HttpPost("bkash")]
    public async Task<IActionResult> BkashWebhook()
    {
        var form = await Request.ReadFormAsync();
        var payload = form.ToDictionary(x => x.Key, x => x.Value.ToString());

        await _webhookService.ProcessBkashWebhookAsync(payload);
        return Ok();
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var payload = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(body) ?? new();

        await _webhookService.ProcessStripeWebhookAsync(payload);
        return Ok();
    }
}
