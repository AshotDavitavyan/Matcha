# Matcha

Dating web application with a .NET 9 backend, an Angular frontend, and PostgreSQL. The backend uses raw Npgsql SQL, MediatR, JWT access tokens, and refresh tokens.

## Prerequisites

- .NET 9 SDK
- Docker with Docker Compose
- Node.js and npm for frontend development

## Local configuration

Create the ignored local environment file from the committed template:

```bash
cp .env.example .env
```

Replace every placeholder in `.env`. Generate a JWT signing key with:

```bash
openssl rand -base64 32
```

The PostgreSQL username, password, and database in the connection string must match `POSTGRES_USER`, `POSTGRES_PASSWORD`, and `POSTGRES_DB`.

Do not commit `.env`. The `.env.example` file documents required variables and must contain placeholders only.

## Database and pgAdmin

Docker Compose automatically reads the root `.env` file:

```bash
docker compose up -d
```

PostgreSQL is exposed on `localhost:5432`. pgAdmin is available at [http://localhost:8080](http://localhost:8080).

Check container status with:

```bash
docker compose ps
```

The schema in `init.sql` is applied when PostgreSQL initializes a new data volume.

## Backend

In Development, the API loads the nearest `.env` file automatically. Existing environment variables take precedence over values in the file.

```bash
dotnet run --project backend/matcha-app
```

The API is available at `http://localhost:5298` under the default HTTP launch profile. Production configuration must be supplied through the deployment environment; the application does not load `.env` outside Development.

Startup fails immediately when the database connection string or JWT signing key is missing.

## Tests

Run both backend test projects from the repository root:

```bash
dotnet test backend/Tests
dotnet test backend/Matcha.Tests
```

Tests provide safe test-only configuration and do not require values from the local `.env` file.
