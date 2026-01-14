using PayGate.Data;
using PayGate.Middleware;
using PayGate.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using DotNetEnv;

// Load environment variables from .env file (industry standard)
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Override configuration with environment variables
builder.Configuration.AddEnvironmentVariables();

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Our services
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<BkashService>();
builder.Services.AddScoped<StripeService>();
builder.Services.AddScoped<WebhookService>();

// HttpClient for gateway calls
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Simple middleware
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
