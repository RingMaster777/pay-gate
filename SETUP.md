# PayGate Setup Guide

## Quick Start Guide

Follow these steps to get PayGate up and running locally.

## Step 1: Prerequisites

Install the following:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 16](https://www.postgresql.org/download/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional)
- Code editor (VS Code, Visual Studio, or Rider)

## Step 2: Database Setup

### Option A: Local PostgreSQL

1. Install and start PostgreSQL
2. Create database:
```sql
CREATE DATABASE paygate;
```

### Option B: Docker PostgreSQL

```bash
docker run --name paygate-postgres \
  -e POSTGRES_DB=paygate \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres123 \
  -p 5432:5432 \
  -d postgres:16-alpine
```

## Step 3: Get Payment Gateway Credentials

### bKash Sandbox

1. Visit [developer.bka.sh](https://developer.bka.sh)
2. Register for sandbox account
3. Create an app to get credentials:
   - App Key
   - App Secret
   - Username
   - Password

### Stripe Test Mode

1. Visit [stripe.com](https://stripe.com)
2. Sign up for an account
3. Get test API key from Dashboard → Developers → API keys
4. Copy the "Secret key" (starts with `sk_test_`)

## Step 4: Configure Application

1. Open `appsettings.json`
2. Update the database connection:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=paygate;Username=postgres;Password=postgres123"
}
```

3. Add your gateway credentials:
```json
"Gateways": {
  "Bkash": {
    "AppKey": "your_bkash_app_key",
    "AppSecret": "your_bkash_app_secret",
    "Username": "your_bkash_username",
    "Password": "your_bkash_password"
  },
  "Stripe": {
    "SecretKey": "sk_test_your_stripe_secret_key"
  }
}
```

## Step 5: Run Database Migrations

```bash
# Navigate to project directory
cd PayGate

# Install EF Core CLI tools (one-time)
dotnet tool install --global dotnet-ef

# Create migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

## Step 6: Seed Test Merchant

Connect to your PostgreSQL database and run:

```sql
INSERT INTO merchants (id, name, email, api_key, is_active, created_at)
VALUES (
    gen_random_uuid(),
    'Test Merchant',
    'test@merchant.com',
    'test_key_12345',
    true,
    NOW()
);
```

## Step 7: Run the Application

```bash
# Run the API
dotnet run
```

You should see:
```
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
```

## Step 8: Test the API

### Using Swagger UI

1. Open browser: `https://localhost:5001/swagger`
2. Click "Authorize" and enter: `test_key_12345`
3. Try the `/api/payments/initiate` endpoint

### Using cURL

```bash
curl -X POST https://localhost:5001/api/payments/initiate \
  -H "Content-Type: application/json" \
  -H "X-API-Key: test_key_12345" \
  -d '{
    "amount": 100,
    "currency": "BDT",
    "gateway": "bkash",
    "orderId": "TEST-001",
    "customerEmail": "test@test.com",
    "customerPhone": "+8801712345678",
    "callbackUrl": "https://example.com/callback",
    "webhookUrl": "https://webhook.site/your-webhook-id"
  }' \
  --insecure
```

## Docker Deployment (Alternative)

If you prefer to run everything in Docker:

```bash
# Copy environment template
cp .env.example .env

# Edit .env with your credentials
nano .env

# Start all services
docker-compose up -d

# Run migrations
docker-compose exec api dotnet ef database update

# Seed merchant (connect to postgres container)
docker-compose exec postgres psql -U postgres -d paygate
# Then run the INSERT statement from Step 6
```

## Troubleshooting

### Database Connection Issues

**Error**: "Could not connect to database"

**Solution**: 
- Check PostgreSQL is running: `pg_isready`
- Verify connection string in appsettings.json
- Check firewall settings

### Migration Errors

**Error**: "Unable to create migration"

**Solution**:
```bash
# Clean and rebuild
dotnet clean
dotnet build
dotnet ef migrations add InitialCreate --force
```

### API Key Issues

**Error**: "Invalid API key"

**Solution**:
- Verify merchant record exists in database
- Check `is_active` is true
- Ensure exact match of API key

### bKash/Stripe Errors

**Error**: Gateway authentication failed

**Solution**:
- Verify credentials in appsettings.json
- For bKash: Ensure using sandbox URL and sandbox credentials
- For Stripe: Ensure using test key (sk_test_...)
- Check gateway service is not blocked by firewall

## Testing Webhooks Locally

Since payment gateways need a public URL, use one of these tools:

### Option 1: ngrok (Recommended)

```bash
# Install ngrok
choco install ngrok  # Windows
brew install ngrok   # Mac

# Start tunnel
ngrok http 5000

# Use the ngrok URL in your webhook configuration
# Example: https://abc123.ngrok.io/api/webhooks/bkash
```

### Option 2: webhook.site

1. Visit [webhook.site](https://webhook.site)
2. Copy your unique URL
3. Use as `webhookUrl` in payment requests
4. View incoming webhooks in real-time

## Next Steps

1. ✅ Set up local development environment
2. ✅ Test payment initiation
3. ✅ Test webhook receiving
4. ⬜ Add refund functionality (optional)
5. ⬜ Deploy to cloud (Azure/AWS/Railway)
6. ⬜ Add monitoring and logging
7. ⬜ Write unit tests

## Additional Commands

```bash
# Watch for changes (auto-reload)
dotnet watch run

# Build for production
dotnet publish -c Release -o ./publish

# Run tests (when added)
dotnet test

# Check for package updates
dotnet list package --outdated
```

## Environment Variables (Production)

For production deployment, use environment variables instead of appsettings.json:

```bash
export ConnectionStrings__DefaultConnection="Host=prod-db;..."
export Gateways__Bkash__AppKey="prod_app_key"
export Gateways__Stripe__SecretKey="sk_live_..."
```

## Getting Help

- Check the main [README.md](README.md) for API documentation
- Review [appsettings.json](appsettings.json) for configuration options
- Check logs in `logs/` directory
- Open an issue on GitHub

---

**Ready to build?** Start with Step 1 and work your way through! 🚀
