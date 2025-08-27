
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
