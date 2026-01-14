# PayGate - Simple Payment Gateway Aggregator

A clean payment API that integrates bKash (Bangladesh) and Stripe (International) behind one unified interface.

## 🚀 Features

- ✅ **Unified Payment API** - One API for multiple gateways
- ✅ **bKash Integration** - Bangladesh's most popular mobile payment
- ✅ **Stripe Integration** - International payment processing
- ✅ **Webhook Handling** - Async payment notifications
- ✅ **Transaction Management** - Complete payment lifecycle
- ✅ **API Key Authentication** - Secure merchant access
- ✅ **Logging with Serilog** - Production-ready logging
- ✅ **Docker Support** - Easy deployment

## 🛠️ Technology Stack

- .NET 9 Web API
- Entity Framework Core 9 with PostgreSQL
- Serilog for logging
- Swagger/OpenAPI
- Docker & Docker Compose

## 📁 Project Structure

```
PayGate/
├── Controllers/         # API endpoints
├── Services/           # Business logic & gateway integration
├── Models/             # Database entities
├── DTOs/               # Request/Response objects
├── Data/               # EF Core DbContext
├── Middleware/         # Authentication & error handling
├── appsettings.json    # Configuration
├── Program.cs          # Application entry point
└── Dockerfile          # Container configuration
```

## 🏗️ Setup & Installation

### Prerequisites

- .NET 9 SDK
- PostgreSQL 16
- Docker (optional)
- bKash Sandbox credentials
- Stripe test API key

### 1. Clone & Restore

```bash
cd PayGate
dotnet restore
```

### 2. Configure Database

Update connection string in [appsettings.json](appsettings.json):

**Better approach:** Use `.env` file (industry standard):

```bash
# Copy the example file
cp .env.example .env

# Edit .env with your credentials
DB_HOST=localhost
DB_PORT=5432
DB_NAME=paygate
DB_USER=postgres
DB_PASSWORD=your_actual_password
```

### 3. Configure Payment Gateways

Add your gateway credentials to `.env`:

```bash
# bKash Sandbox
BKASH_APP_KEY=your_app_key
BKASH_APP_SECRET=your_app_secret
BKASH_USERNAME=your_username
BKASH_PASSWORD=your_password

# Stripe Test
STRIPE_SECRET_KEY=sk_test_your_real_key
```

**Note:** The `.env` file is gitignored and will never be committed. Safe! 🔒

### 4. Run Migrations

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### 5. Seed Test Merchant

```sql
INSERT INTO merchants (id, name, email, api_key, is_active)
VALUES (
    gen_random_uuid(),
    'Test Merchant',
    'test@merchant.com',
    'test_key_12345',
    true
);
```

### 6. Run the Application

```bash
dotnet run
```

The API will be available at: `https://localhost:5001` or `http://localhost:5000`

Swagger UI: `https://localhost:5001/swagger`

## 🐳 Docker Deployment

### Using Docker Compose

```bash
# Copy environment file
cp .env.example .env

# Edit .env with your credentials
# Then start the services
docker-compose up -d
```

This will start:

- PostgreSQL database on port 5432
- PayGate API on port 5000

### Run migrations in Docker

```bash
docker-compose exec api dotnet ef database update
```

## 📡 API Usage

### 1. Initiate Payment

```bash
curl -X POST http://localhost:5000/api/payments/initiate \
  -H "Content-Type: application/json" \
  -H "X-API-Key: test_key_12345" \
  -d '{
    "amount": 1000,
    "currency": "BDT",
    "gateway": "bkash",
    "orderId": "ORD-123",
    "customerEmail": "customer@test.com",
    "customerPhone": "+8801712345678",
    "callbackUrl": "https://yoursite.com/success",
    "webhookUrl": "https://webhook.site/your-unique-id"
  }'
```

**Response:**

```json
{
  "transactionId": "TXN-20260101-abc123",
  "paymentUrl": "https://bkash.com/pay/xyz",
  "status": "pending"
}
```

### 2. Get Transaction Status

```bash
curl -X GET http://localhost:5000/api/payments/TXN-20260101-abc123 \
  -H "X-API-Key: test_key_12345"
```

**Response:**

```json
{
  "transactionId": "TXN-20260101-abc123",
  "gateway": "bkash",
  "orderId": "ORD-123",
  "amount": 1000,
  "status": "success",
  "paidAt": "2026-01-01T10:30:00Z",
  "createdAt": "2026-01-01T10:25:00Z"
}
```

### 3. Webhook Endpoints

PayGate receives webhooks from payment gateways:

- **bKash**: `POST /api/webhooks/bkash`
- **Stripe**: `POST /api/webhooks/stripe`

When a payment is completed, PayGate will forward the notification to your merchant webhook URL.

## 🔐 Authentication

All payment endpoints require an API key header:

```
X-API-Key: your_api_key_here
```

Webhook endpoints are public (no authentication required).

## 📊 Database Schema

### Merchants

- Stores merchant information and API keys

### Transactions

- Tracks all payment transactions
- Links to merchant and gateway

### Refunds

- Records refund requests and status

### Webhook Logs

- Logs all webhook events for debugging

## 🧪 Testing

### Test with bKash Sandbox

1. Register for bKash sandbox credentials at [https://developer.bka.sh](https://developer.bka.sh)
2. Use test credentials in appsettings.json
3. Test payments will be in sandbox mode

### Test with Stripe

1. Get test API keys from [https://stripe.com](https://stripe.com)
2. Use `sk_test_...` key in appsettings.json
3. Use test card numbers (e.g., `4242 4242 4242 4242`)

### Webhook Testing

Use [webhook.site](https://webhook.site) to test webhook forwarding:

1. Generate a unique URL
2. Use it as `webhookUrl` in payment request
3. View webhook payloads in real-time

## 📝 Development Roadmap

**Phase 1: Core Features** ✅

- Payment initiation
- Webhook handling
- Transaction tracking

**Phase 2: Enhancements** (Optional)

- Refund processing
- Payment analytics dashboard
- Multi-currency support
- Rate limiting
- Caching with Redis

**Phase 3: Advanced** (Optional)

- Recurring payments
- Payment links
- QR code generation
- Mobile SDKs

## 🤝 Contributing

This is a portfolio project, but contributions are welcome:

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 📄 License

MIT License - feel free to use this project for learning and portfolio purposes.

## 👨‍💻 Author

Built as a portfolio project to demonstrate:

- Payment gateway integration
- Financial transaction handling
- Webhook processing
- Clean .NET 9 architecture
- Production-ready patterns

## 🔗 Useful Resources

- [bKash Tokenized API Docs](https://developer.bka.sh/docs/tokenized-checkout-overview)
- [Stripe API Documentation](https://stripe.com/docs/api)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [Serilog Documentation](https://serilog.net/)

## 📞 Support

For issues or questions:

- Open an issue on GitHub
- Check existing documentation
- Review code comments

---

**Note**: This is a demonstration project. For production use, add:

- Enhanced security measures
- Input validation
- Rate limiting
- Monitoring & alerting
- Comprehensive error handling
- Unit & integration tests
