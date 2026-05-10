# MyFinances — Backend

ASP.NET Core 9 Web API for the MyFinances personal finance management app. Built with clean architecture, EF Core + PostgreSQL, JWT authentication, and Supabase file storage.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 9 / ASP.NET Core |
| ORM | Entity Framework Core 9 + Npgsql |
| Auth | JWT Bearer + Google OAuth |
| Mapping | AutoMapper 16 |
| Storage | Supabase (profile images) |
| Email | MailKit (SMTP) |
| Export | ClosedXML (Excel) |
| Docs | Swagger / OpenAPI |

---

## Project Structure

```
MyFinances/
├── Api/            → Controllers, DTOs, AutoMapper profiles, Middleware
├── App/            → Services, Queries, Filters, Abstractions (interfaces)
├── Domain/         → Entities, Enums, Exceptions
└── Infrastructure/ → EF Core, Repositories, JWT, Email, Supabase
```

### Layers

- **Api** — HTTP concerns only: request parsing, response shaping, route definitions.
- **App** — Business logic: service classes, query handlers, filters.
- **Domain** — Pure entities (`User`, `Account`, `Category`, `Transaction`, `PasswordResetToken`), enums (`TransactionType`, `AccountType`), and custom exceptions.
- **Infrastructure** — Data access (`FinanceDbContext`, generic `Repository<T>`, `UnitOfWork`), JWT generation, SMTP email, Supabase storage.

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL instance
- (Optional) Supabase project for file storage
- (Optional) SMTP credentials for email features

### Running the API

```bash
# From finances-backend/MyFinances/
dotnet run          # http://localhost:5276
dotnet watch run    # with hot reload
```

Swagger UI is available at `http://localhost:5276/swagger` in the development environment.

---

## Configuration

All configuration lives in `appsettings.json` / `appsettings.Development.json`. Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for sensitive values in development.

| Key | Description | Default |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | — |
| `Jwt__Key` | JWT signing key | built-in dev key |
| `Jwt__ExpirationMinutes` | Token lifetime in minutes | `60` |
| `Jwt__Issuer` | JWT issuer | `finances` |
| `Jwt__Audience` | JWT audience | `finances-client` |
| `Cors__AllowedOrigins` | Comma-separated origins (dev) | `localhost:5173` |
| `Cors__ProductionOrigin` | Single allowed origin (prod) | — |
| `Google__ClientId` | Google OAuth Client ID | — |
| `Supabase__Url` | Supabase project URL | — |
| `Supabase__ServiceKey` | Supabase service role key | — |
| `Supabase__Bucket` | Storage bucket name | `profile-images` |
| `Email__SmtpHost` | SMTP server | `smtp.gmail.com` |
| `Email__SmtpPort` | SMTP port | `587` |
| `Email__SmtpUser` | SMTP username | — |
| `Email__SmtpPassword` | SMTP password | — |
| `Email__FromAddress` | Sender email address | — |

---

## Database Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName>

# Apply pending migrations
dotnet ef database update
```

---

## API Overview

All endpoints require JWT Bearer authentication (`[Authorize]`). Routes follow the pattern `/api/{resource}`.

| Controller | Route | Responsibility |
|---|---|---|
| `AuthController` | `/api/auth` | Register, login, Google OAuth, password reset |
| `AccountController` | `/api/accounts` | CRUD for financial accounts |
| `CategoryController` | `/api/categories` | CRUD for transaction categories |
| `TransactionController` | `/api/transactions` | CRUD, filters, pagination, Excel export |
| `SummaryController` | `/api/summary` | Aggregated income/expense totals |
| `CategoryReportController` | `/api/category-report` | Breakdown by category for a date range |

### Error Responses

The global exception middleware maps domain exceptions to HTTP status codes:

| Exception | HTTP Status |
|---|---|
| `BadRequestException` | 400 |
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `ValidationException` | 422 |

---

## Security

- Every data query is filtered by the authenticated user's `UserId` — there is no cross-user data access.
- `ICurrentUserService` resolves the user from the JWT claims and is injected into every service.
- Passwords are hashed with BCrypt.
- JWT signing key should be rotated and stored securely in production (never commit it).
