# Matcha Dating App - Architecture Document

## 1. Executive Summary

This architecture implements a scalable dating web application using .NET 9 modular monolith with Angular 19 frontend. The design prioritizes maintainability through Clean Architecture for core user management, vertical slicing for feature modules, and modern patterns like CQRS and FastEndpoints. PostgreSQL with EF Core provides robust data persistence, while SignalR enables real-time interactions. The modular approach allows independent scaling and future microservices migration without architectural rewrites.

## 2. Modules & Responsibilities

### Users Module
- **API Boundary**: `/api/users/*` - User CRUD, profile management, authentication
- **Storage**: `Users` aggregate, `UserProfiles` table, `AuthTokens` table
- **Pattern**: Clean Architecture + CQRS with MediatR

### Matching Module  
- **API Boundary**: `/api/matching/*` - Suggestions, compatibility scoring, match creation
- **Storage**: `Matches` table, `CompatibilityScores` table, `UserPreferences` table
- **Pattern**: Vertical Slicing with FastEndpoints

### Likes Module
- **API Boundary**: `/api/likes/*` - Like/unlike actions, like history, mutual likes
- **Storage**: `Likes` table, `LikeHistory` table
- **Pattern**: Vertical Slicing with FastEndpoints

### Chat Module
- **API Boundary**: `/api/chat/*` - Message CRUD, chat rooms, typing indicators
- **Storage**: `ChatRooms` table, `Messages` table, `ChatParticipants` table
- **Pattern**: Vertical Slicing with FastEndpoints

### Notifications Module
- **API Boundary**: `/api/notifications/*` - Push notifications, email alerts, in-app notifications
- **Storage**: `Notifications` table, `NotificationSettings` table
- **Pattern**: Vertical Slicing with FastEndpoints

### Search Module
- **API Boundary**: `/api/search/*` - Advanced filtering, geospatial queries, text search
- **Storage**: Search indexes, `SearchHistory` table
- **Pattern**: Vertical Slicing with FastEndpoints

## 3. Database & Migrations

**Schema Management**: Use EF Core migrations for development with `dotnet ef migrations add` commands. Maintain separate migration folders per module to avoid conflicts.

**Seeding Strategy**: Implement `IDataSeeder` interface with module-specific seeders. Use `IHostedService` for automatic seeding on startup in development.

**Empty SQL Backup**: Run `pg_dump --schema-only --no-owner --no-privileges matcha_db > empty_schema.sql` to generate deployment-ready empty database structure.

```csharp
// Example migration command
dotnet ef migrations add InitialUsersModule --project src/Users.Infrastructure --startup-project src/API
```

## 4. Matching & Fame Rating

**Fame Formula**: Weighted combination of profile completeness (30%), photo quality (25%), response rate (20%), activity level (15%), and user feedback (10%).

**Trade-offs**: Simple formula ensures transparency but may be gamed. Consider machine learning enhancement for production.

**Calculation Steps**:
1. Profile completeness: (filled_fields / total_fields) × 30
2. Photo quality: (verified_photos / total_photos) × 25  
3. Response rate: (responses / messages_received) × 20
4. Activity level: (logins_last_30_days / 30) × 15
5. User feedback: (positive_ratings / total_ratings) × 10

```csharp
public class FameCalculator
{
    public decimal CalculateFameRating(UserProfile profile)
    {
        var profileScore = (profile.FilledFields / 12m) * 30m; // 12 total fields
        var photoScore = (profile.VerifiedPhotos / profile.TotalPhotos) * 25m;
        var responseScore = (profile.ResponseCount / profile.ReceivedMessages) * 20m;
        var activityScore = (profile.LoginsLast30Days / 30m) * 15m;
        var feedbackScore = (profile.PositiveRatings / profile.TotalRatings) * 10m;
        
        return Math.Round(profileScore + photoScore + responseScore + activityScore + feedbackScore, 2);
    }
}
```

## 5. Location & Privacy

**GPS Opt-in**: Explicit consent required with clear privacy policy. Store coordinates with precision masking (city-level accuracy).

**IP Geolocation Fallback**: Use MaxMind GeoIP2 or IPinfo.io for approximate location when GPS unavailable.

**Manual Override**: Allow users to set custom location with radius preferences.

**Privacy Flags**: 
- `LocationSharingEnabled`: Boolean flag for GPS consent
- `LocationPrecision`: Enum (Exact, City, Region, Country)
- `LocationOverride`: Custom coordinates when GPS disabled

**UX Notes**: Show location accuracy indicator, allow temporary location hiding, implement location-based search radius controls.

## 6. API & Auth

**JWT + Refresh Flow**: Short-lived access tokens (15min) with refresh tokens (7 days). Refresh endpoint exchanges refresh token for new access token.

**Main Endpoints**:

```yaml
POST /api/auth/register:
  body: { username, email, password, confirmPassword }
  response: { userId, verificationRequired }

POST /api/auth/verify-email:
  body: { token }
  response: { success }

POST /api/auth/login:
  body: { username, password }
  response: { accessToken, refreshToken, expiresIn }

POST /api/auth/refresh:
  body: { refreshToken }
  response: { accessToken, expiresIn }

GET /api/matching/suggestions:
  headers: { Authorization: Bearer {token} }
  response: { users: [{ id, username, photos, fameRating }] }

POST /api/likes/{userId}:
  headers: { Authorization: Bearer {token} }
  response: { isMatch: boolean }

POST /api/chat/rooms:
  body: { participantIds: [userId1, userId2] }
  response: { roomId }

POST /api/chat/rooms/{roomId}/messages:
  body: { content, messageType }
  response: { messageId, timestamp }
```

## 7. Realtime & Events

**SignalR Hub Contracts**:

```csharp
public interface IMatchaHub
{
    Task JoinUserGroup(string userId);
    Task LeaveUserGroup(string userId);
    Task SendMessage(string roomId, string content);
    Task TypingIndicator(string roomId, bool isTyping);
    Task NewMatch(string matchedUserId);
    Task NewLike(string likedUserId);
    Task ProfileView(string viewedUserId);
}
```

**Event Envelope Example**:

```csharp
public class DomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; }
    public string AggregateId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public object Data { get; set; }
    public string CorrelationId { get; set; }
}
```

## 8. Media Storage

**S3-Compatible Storage**: Use AWS S3 or DigitalOcean Spaces for production. MinIO for development.

**Resizing Rules**: 
- Profile photos: 400x400px (square crop)
- Thumbnails: 150x150px (square crop)
- Chat images: 800px max width, maintain aspect ratio

**Signed URLs**: Generate time-limited URLs (1 hour) for secure access. Implement CDN caching for thumbnails.

**Development Setup**: Docker Compose with MinIO service for local development.

## 9. Dev & Repo Conventions

**Backend Structure**:
```
src/
├── API/                    # FastEndpoints startup project
├── Users.Application/      # Clean Architecture layers
├── Users.Infrastructure/
├── Matching/               # Vertical slice modules
├── Likes/
├── Chat/
└── Shared/
```

**Frontend Structure**:
```
src/app/
├── core/                   # Auth, interceptors, layout
├── features/               # Feature modules
│   ├── user/
│   ├── chat/
│   └── settings/
└── shared/                 # Common components
```

**Naming**: PascalCase for C#, camelCase for TypeScript. Use descriptive names: `GetUserSuggestionsQuery` not `GetUsers`.

**Branch Strategy**: `feature/`, `bugfix/`, `hotfix/` prefixes. Main branch for production releases.

## 10. Testing, CI/CD, Observability

**Testing Strategy**:
- Unit tests: 80%+ coverage with xUnit/NUnit
- Integration tests: API endpoints with TestContainers
- E2E tests: Playwright for critical user flows

**CI/CD Pipeline** (GitHub Actions):
```yaml
- Build & test backend (.NET)
- Build & test frontend (Angular)
- Run integration tests with PostgreSQL
- Deploy to staging
- Run E2E tests
- Deploy to production
```

**Observability**: Serilog for structured logging, OpenTelemetry for tracing, Prometheus metrics for performance monitoring.

## 11. Security Checklist

**OWASP Top 10**:
- Input validation on all endpoints
- SQL injection prevention (EF Core parameterized queries)
- XSS protection (Angular sanitization)
- CSRF tokens for state-changing operations
- Secure headers (HSTS, CSP, X-Frame-Options)

**Rate Limiting**: 100 requests/minute per IP, 1000 requests/hour per user.

**Brute Force Protection**: Account lockout after 5 failed attempts, exponential backoff.

**GDPR Compliance**: Data export/deletion endpoints, consent management, location data anonymization.

## 12. Deliverables & Acceptance Criteria

**Required Artifacts**:
- [ ] This architecture document (Markdown)
- [ ] Sample FastEndpoints route with validation
- [ ] EF Core migration example with seeding
- [ ] Docker Compose development template
- [ ] Angular standalone component scaffold
- [ ] Authentication service implementation
- [ ] SignalR hub with basic events
- [ ] Database schema documentation
- [ ] API documentation (OpenAPI/Swagger)
- [ ] Deployment scripts and configuration

```
