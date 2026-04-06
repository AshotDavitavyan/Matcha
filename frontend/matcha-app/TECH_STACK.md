# Matcha Dating App - Tech Stack

## 1. Executive Summary

This tech stack delivers a scalable dating web application using .NET 9 modular monolith with Angular 19 frontend. The architecture prioritizes developer productivity through FastEndpoints for rapid API development, Clean Architecture for core user management, and modern patterns like CQRS and SignalR for real-time interactions. PostgreSQL provides robust data persistence while NgRx manages complex frontend state. The modular approach enables independent scaling and future microservices migration without architectural rewrites.

## 2. Backend Stack

**Runtime**: .NET 9 SDK (latest LTS)
**Framework**: ASP.NET Core 9 with minimal APIs
**Key NuGet Packages**:
- `FastEndpoints` (4.0+) - Rapid API development with validation
- `Microsoft.EntityFrameworkCore` (9.0+) - ORM with migrations
- `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0+) - PostgreSQL provider
- `Microsoft.AspNetCore.SignalR` (9.0+) - Real-time communication
- `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0+) - JWT authentication
- `MediatR` (12.0+) - CQRS pattern implementation
- `FluentValidation` (11.0+) - Input validation
- `Serilog.AspNetCore` (8.0+) - Structured logging

```csharp
// Program.cs snippet
var builder = WebApplication.CreateBuilder(args);

// FastEndpoints
builder.Services.AddFastEndpoints();

// EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// SignalR
builder.Services.AddSignalR();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT config */ });

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.MapHub<MatchaHub>("/hubs/matcha");
```

## 3. Frontend Stack

**Node Version**: 20.x LTS
**Angular**: 19.x with standalone components
**State Management**: NgRx 18.x
**Styling**: Tailwind CSS 4.x + PrimeNG 19.x
**Key Dependencies**:

```json
{
  "dependencies": {
    "@angular/core": "^19.2.0",
    "@ngrx/store": "^18.0.0",
    "@ngrx/effects": "^18.0.0",
    "@ngrx/router-store": "^18.0.0",
    "primeng": "^19.1.3",
    "tailwindcss": "^4.1.10",
    "rxjs": "~7.8.0"
  }
}
```

**CLI Commands**:
```bash
ng generate component features/user/user-profile --standalone
ng generate service core/services/user --skip-tests
ng generate store features/user/user --skip-tests
```

## 4. Database & Persistence

**PostgreSQL**: 16.x (latest stable)
**EF Core Provider**: `Npgsql.EntityFrameworkCore.PostgreSQL`
**Connection String**:
```csharp
"DefaultConnection": "Host=localhost;Database=matcha_db;Username=postgres;Password=dev_password"
```

**Migrations Strategy**: Module-specific migration folders to avoid conflicts. Use `dotnet ef migrations add ModuleName_Initial --project src/Module.Infrastructure`.

**Dev Seeding**: Implement `IDataSeeder` interface with `IHostedService` for automatic development data population.

## 5. Messaging & Realtime

**SignalR**: Built-in WebSocket support with automatic fallback to Server-Sent Events/Long Polling.

**Message Broker Adapters** (optional):
- **RabbitMQ**: `RabbitMQ.Client` (6.0+) for reliable message queuing
- **Kafka**: `Confluent.Kafka` (2.0+) for high-throughput event streaming

```csharp
// Abstract broker interface
public interface IMessageBroker
{
    Task PublishAsync<T>(string topic, T message);
    Task SubscribeAsync<T>(string topic, Func<T, Task> handler);
}

// SignalR usage
public class MatchaHub : Hub
{
    public async Task JoinUserGroup(string userId) => 
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
}
```

## 6. Storage & Media

**S3-Compatible Providers**: AWS S3, DigitalOcean Spaces (examples)
**Development**: MinIO (Docker container)
**Signed URL Flow**:

```csharp
public class MediaService
{
    public async Task<string> GenerateSignedUrl(string objectKey, TimeSpan expiry)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.Add(expiry)
        };
        return await _s3Client.GetPreSignedURLAsync(request);
    }
}
```

## 7. Dev Tooling & Local Dev

**Docker Compose** (dev environment):
```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: matcha_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: dev_password
    ports:
      - "5432:5432"
  
  minio:
    image: minio/minio
    command: server /data --console-address ":9001"
    ports:
      - "9000:9000"
      - "9001:9001"
```

**CLI Tools**: `dotnet` CLI, `ng` CLI, `npm` scripts
**IDE Extensions**: C# Dev Kit, Angular Language Service, Tailwind CSS IntelliSense

## 8. Testing & QA

**Backend**: NUnit (3.0+) with TestContainers for integration tests
**Frontend**: Jest + Angular Testing Library
**E2E**: Playwright (latest)
**Test Commands**:
```bash
dotnet test --logger "console;verbosity=detailed"
ng test --watch=false --browsers=ChromeHeadless
npx playwright test
```

## 9. CI/CD & Releases

**Pipeline Steps**: Build → Test → Migrate → Publish Image
**Image Registry**: GitHub Container Registry or Docker Hub
**Migration Strategy**: Blue-green deployments with automated rollback. Database migrations run in separate step before application deployment. Always backup production database before releases.

## 10. Versioning & Upgrade Policy

**Pinned Versions**: .NET SDK, PostgreSQL, Node.js (exact versions)
**Major Upgrades**: Plan quarterly reviews for Angular, NgRx, EF Core
**Patch Updates**: Automated via Dependabot with security-only policy
**Breaking Changes**: Test in staging environment for 48 hours before production

## 11. Alternatives & Trade-offs

**RabbitMQ vs Kafka**: RabbitMQ for reliability, Kafka for high-throughput event streaming. Choose based on message volume and consistency requirements.

**S3 vs Managed Storage**: S3 for cost control and flexibility, managed solutions (Cloudinary) for built-in image processing. S3 requires more implementation work but offers better cost scaling.

**NgRx vs Simple State**: NgRx for complex state management and time-travel debugging, simple services for basic CRUD operations. NgRx adds complexity but provides better testability and state predictability.

## 12. Deliverables

**Required Artifacts**:
- [ ] This Tech Stack document (Markdown)
- [ ] `Program.cs` snippet with FastEndpoints, EF Core, SignalR, JWT setup
- [ ] `docker-compose.yml` development template with PostgreSQL and MinIO
- [ ] Minimal `package.json` with Angular 19, NgRx, PrimeNG, Tailwind dependencies
- [ ] CI pipeline skeleton (GitHub Actions YAML) with build/test/migrate/publish steps
- [ ] Sample FastEndpoints route with validation and CQRS pattern
- [ ] EF Core migration example with seeding strategy
- [ ] Angular standalone component scaffold with NgRx store
- [ ] SignalR hub implementation with basic event contracts
- [ ] Authentication service with JWT + refresh token flow
- [ ] Database schema documentation with module boundaries
- [ ] API documentation template (OpenAPI/Swagger)
- [ ] Deployment configuration and environment variables template
