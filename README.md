# aspnetcore-webapi-starter

ASP.NET Core 8 Web API starter using Clean Architecture. JWT authentication, EF Core with SQL Server, repository pattern, DTOs with validation, Serilog logging, global exception handling, and Swagger.

## Projects

| Project | Layer | Contains |
|---|---|---|
| `src/Api` | Presentation | Controllers, exception middleware, `Program.cs` |
| `src/Application` | Application | DTOs, services, repository interfaces, validation |
| `src/Domain` | Domain | Entities (`Product`, `User`) |
| `src/Infrastructure` | Infrastructure | EF Core `DbContext`, repositories, JWT + password hashing |

Dependency rule: Api depends on Application and Infrastructure. Application and Infrastructure depend on Domain. Domain depends on nothing.

## Setup

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download).
2. Update `ConnectionStrings:DefaultConnection` in `src/Api/appsettings.json` to point at your SQL Server instance.
3. Replace `Jwt:Key` with your own secret (at least 32 characters). Never commit a real key.
4. From the repo root:

```bash
dotnet restore
dotnet ef database update --project src/Infrastructure --startup-project src/Api
dotnet run --project src/Api
```

Migrations also apply automatically on startup, so step 2 of the commands is optional if the database is reachable.

5. Open Swagger: http://localhost:5023/swagger

## Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | No | Register a user, returns a JWT |
| POST | `/api/auth/login` | No | Login, returns a JWT |
| GET | `/api/products` | No | List all products |
| GET | `/api/products/paged?page=1&pageSize=20` | No | List products, paginated (page ≥ 1, pageSize 1–100) |
| GET | `/api/products/{id}` | No | Get one product |
| GET | `/api/products/search?term=` | No | Search products by name |
| POST | `/api/products` | Yes | Create a product |
| PUT | `/api/products/{id}` | Yes | Update a product |
| DELETE | `/api/products/{id}` | Yes | Delete a product |

In Swagger, click Authorize and paste `Bearer {your-token}`.

## Notes

- Passwords are hashed with PBKDF2 (SHA-256, 100,000 iterations). No plain-text passwords anywhere.
- DTO validation uses DataAnnotations; invalid requests return 400 automatically via `[ApiController]`.
- All unhandled exceptions go through `ExceptionHandlingMiddleware` and return a consistent `{ statusCode, message }` JSON shape.
- Logs go to the console and to `logs/app-*.log` (daily rolling files, 7 days kept).
- The database is seeded with 3 sample products on first migration.
