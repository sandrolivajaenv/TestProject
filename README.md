# Products API – Technical Assessment

## Overview
A RESTful API built with **.NET 8** following Clean/Onion architecture principles.  
Implements **Products** and **Items** CRUD with **JWT authentication**, **idempotency**, **rate limiting**, and **structured logging**.  

---

## Architecture

```
Solution/
├── src/
│   ├── API/                  # ASP.NET Core Web API
│   │   ├── Controllers/      # Products & Items controllers
│   │   ├── Middleware/       # Custom middlewares (Idempotency, SecurityHeaders, CorrelationId)
│   │   └── Program.cs        # Entry point
│   ├── Application/          # Business logic layer
│   │   ├── DTOs/             # Data Transfer Objects
│   │   ├── Interfaces/       # Service/repository interfaces
│   │   ├── Services/         # Services (ProductService, ItemService)
│   │   └── Validators/       # FluentValidation validators
│   ├── Domain/               # Domain entities, exceptions
│   └── Infrastructure/       # EF Core DbContext, repositories, identity
├── tests/
│   ├── API.Tests/            # Integration tests with WebApplicationFactory
│   ├── Application.Tests/    # Unit tests (services with Moq)
│   └── Infrastructure.Tests/ # Repo/DbContext tests with SQLite
└── docker-compose.yml        # Docker setup (API + SQL Server)
```

- **Domain** → Entities (`Product`, `Item`), exceptions  
- **Application** → Services, DTOs, interfaces, validators  
- **Infrastructure** → EF Core DbContext, repositories, UoW, AuthService  
- **API** → Controllers, filters, middleware, DI composition  

---

## Features

- **CRUD** for Products and nested Items  
- **Pagination** on list endpoints  
- **Idempotency** for POSTs (via `Idempotency-Key` header)  
- **JWT auth** with refresh token rotation (stored in DB)  
- **Role-based authorization** (`[Authorize(Roles="Admin")]` on sensitive ops)  
- **Rate limiting** (429 + Retry-After header)  
- **Response compression** (gzip + brotli)  
- **Structured logging** with correlation IDs (Serilog)  
- **Security headers** (CSP, HSTS, X-Frame-Options, etc.)  
- **Health checks** at `/health` and `/health/db`  
- **Swagger/OpenAPI** with XML comments & JWT support  

---

## API Endpoints

### Products
- `GET    /api/v1/products?page=1&pageSize=20&includeItems=true`
- `GET    /api/v1/products/{id}`
- `POST   /api/v1/products` *(requires [Authorize], supports `Idempotency-Key`)*
- `PUT    /api/v1/products/{id}`
- `DELETE /api/v1/products/{id}`

### Items (nested under Product)
- `GET    /api/v1/products/{productId}/items`
- `GET    /api/v1/products/{productId}/items/{itemId}`
- `POST   /api/v1/products/{productId}/items` *(requires [Authorize], supports `Idempotency-Key`)*
- `DELETE /api/v1/products/{productId}/items/{itemId}`

### Auth
- `POST   /api/auth/login` → `{ access_token, refresh_token }`  
- `POST   /api/auth/refresh` → rotate refresh token  

---

## Authentication Flow

1. `POST /api/auth/login` with username/password → receive access + refresh tokens.  
2. Send access token in header:  
   ```
   Authorization: Bearer <access_token>
   ```  
3. When expired, call `POST /api/auth/refresh` with refresh token → receive new tokens.  
4. Refresh tokens are rotated & old ones revoked.  

---

## Validation & Errors

- All input validated with **FluentValidation**.  
- Errors returned in **ProblemDetails** format:  

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "ProductName": [ "Product name is required" ]
  }
}
```

- Domain exceptions (`NotFound`, `InvalidOperation`) are mapped to 404 / 409.

---

## Performance Considerations

- `.AsNoTracking()` on queries to reduce EF overhead  
- SQL **indexes**:  
  - Unique on `Product.ProductName`  
  - Index on `Item.ProductId`  
- **Pagination** on all collection endpoints  
- **Response compression** (gzip, brotli)  
- Async/await everywhere for scalability  

---

## Security Measures

- Short-lived **JWT** + refresh token rotation  
- **Role-based authorization**  
- **FluentValidation** input checks  
- **CORS** restricted by config  
- **HTTPS + HSTS** enforced  
- **Security headers**: CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, etc.  
- **Rate limiting** returns 429 + Retry-After  

---

## Health & Observability

- `/health` → global health  
- `/health/db` → DB connectivity  
- **Serilog** structured logging (JSON, correlation IDs, request context)  
- Logs include request path, status, elapsed ms, user, correlation ID  

---

## Running Locally

### Prerequisites
- .NET 8 SDK  
- Docker  

### Run with Docker Compose
```bash
docker compose up --build
```

- API → http://localhost:8080/swagger  
- SQL Server → localhost:1433  

### Run locally (without Docker)
1. Update `appsettings.Development.json` with a valid SQL connection string.  
2. Run migrations:
   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/API
   ```
3. Start API:
   ```bash
   dotnet run --project src/API
   ```
4. Open Swagger: https://localhost:5001/swagger  

---

## Deployment

- **Containerized** via multi-stage Dockerfile (.NET SDK → runtime).  
- **Healthcheck** hits `/health` for liveness.  
- **Env vars** configure connection strings, JWT keys, and CORS origins.  
- **Migrations** can be applied on startup (`Database.Migrate()`) or run separately in CI/CD.  

---

## Testing

- **xUnit + Moq** unit tests (Application services).  
- **Integration tests** with `WebApplicationFactory`, SQLite in-memory, and a test auth scheme.  
- **Infrastructure tests** for EF repositories and UoW with SQLite fixture.  
- Scenarios covered: CRUD, idempotency (201/409), auth (401/403), rate limiting (429 + Retry-After).  

Run all tests:

```bash
dotnet test
```

---

## Future Improvements

- OpenTelemetry tracing & metrics  
- Caching / ETag support  
- More granular rate limit policies  
- Multi-tenant auth & user management  
- CI/CD pipeline with migrations + container publishing  
