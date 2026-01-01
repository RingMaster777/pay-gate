using PayGate.Data;
using Microsoft.EntityFrameworkCore;

namespace PayGate.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        // Skip auth for webhooks
        if (context.Request.Path.StartsWithSegments("/api/webhooks"))
        {
            await _next(context);
            return;
        }

        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        if (string.IsNullOrEmpty(apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API key required" });
            return;
        }

        var merchant = await db.Merchants.FirstOrDefaultAsync(m => m.ApiKey == apiKey);
        if (merchant == null || !merchant.IsActive)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        context.Items["MerchantId"] = merchant.Id;
        await _next(context);
    }
}
