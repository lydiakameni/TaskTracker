# TaskTracker API

[![CI](https://github.com/lydiakameni/TaskTracker/actions/workflows/ci.yml/badge.svg)](https://github.com/lydiakameni/TaskTracker/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)
![Clean Architecture](https://img.shields.io/badge/architecture-Clean-blue)

🇫🇷 [Version française](README.fr.md)

A secure REST API for task management, built with **ASP.NET Core (.NET 10)** following **Clean Architecture** and **CQRS** principles.

Administrators create and assign tasks; users view and complete the tasks assigned to them.

![Swagger UI](docs/swagger.png)

---

## Features

- **JWT authentication** with short-lived access tokens (15 min)
- **Refresh token rotation** stored in an `HttpOnly`, `Secure`, `SameSite=Strict` cookie
- **Logout with token revocation** in the database
- **Role-based authorization** (`Admin` / `User`)
- **Ownership rules**: a user can only access their own tasks (admins can access all)
- **Input validation** with FluentValidation (password strength, username format, etc.)
- **Standardized errors** following RFC 7807 (`ProblemDetails`) with a `traceId`
- **Soft delete** for tasks
- **Interactive documentation** with Swagger / OpenAPI, generated from XML comments

## Tech stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core 10, C# |
| Architecture | Clean Architecture, CQRS with MediatR |
| Data | Entity Framework Core 10, SQL Server |
| Security | JWT Bearer, BCrypt, refresh token rotation |
| Validation | FluentValidation (MediatR pipeline behavior) |
| Testing | xUnit, Moq, WebApplicationFactory, EF Core InMemory |
| CI | GitHub Actions |

## Architecture

```
TaskTracker.Domain          Entities, repository interfaces, constants (no dependencies)
TaskTracker.Application     Commands, queries, handlers, validators, DTOs, exceptions
TaskTracker.Infrastructure  EF Core, repositories, JWT generation, password hashing
TaskTracker.API             Controllers, global exception handler, Swagger
tests/
  TaskTracker.Application.Tests     Unit tests (handlers and validators)
  TaskTracker.API.IntegrationTests  End-to-end tests of the HTTP API
```

Dependencies point inward: the **Domain** knows nothing about the database or the web.
This keeps business rules simple to test and independent of technical choices.

## Security choices

- **Passwords** are hashed with BCrypt, never stored in plain text.
- **The JWT secret key** is never committed: it comes from `dotnet user-secrets` in development
  and from an environment variable (`Jwt__SecretKey`) in production.
  The API refuses to start without a valid key (32+ bytes).
- **Refresh tokens** are rotated on each use: a stolen token can only be used once.
- **Role assignment** is done by the server only: new accounts always get the `User` role.
- **Tasks owned by other users** return `404 Not Found` (not `403`), so their existence is never revealed.
- **Server errors** return a generic message: no internal details are exposed to the client.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express or LocalDB)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### 1. Clone the repository

```bash
git clone https://github.com/lydiakameni/TaskTracker.git
cd TaskTracker
```

### 2. Configure the JWT secret key

```bash
dotnet user-secrets set "Jwt:SecretKey" "<a-random-key-of-at-least-32-characters>" --project TaskTracker.API
```

Tip: generate a secure key with `openssl rand -base64 48`.

### 3. Create the database

Update the connection string in `TaskTracker.API/appsettings.json` if needed, then run:

```bash
dotnet ef database update --project TaskTracker.Infrastructure --startup-project TaskTracker.API
```

### 4. Run the API

```bash
dotnet run --project TaskTracker.API
```

Open Swagger at `http://localhost:5118/swagger`.

### 5. Create an administrator

New accounts always get the `User` role. To promote an account to `Admin`, update it directly in the database:

```sql
UPDATE Users SET Role = 'Admin' WHERE Username = 'your_username';
```

Then log in again to get a token with the new role.

## API endpoints

| Method | Route | Access | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Create an account (`User` role) |
| `POST` | `/api/auth/login` | Public | Log in and receive tokens |
| `POST` | `/api/auth/refresh_token` | Refresh cookie | Get a new access token |
| `POST` | `/api/auth/logout` | Refresh cookie | Log out and revoke the refresh token |
| `GET` | `/api/tasks` | Authenticated | List my tasks |
| `GET` | `/api/tasks/{id}` | Owner or Admin | Get a task |
| `PATCH` | `/api/tasks/{id}/complete` | Owner or Admin | Mark a task as completed |
| `POST` | `/api/tasks` | Admin | Create and assign a task |
| `DELETE` | `/api/tasks/{id}` | Admin | Delete a task (soft delete) |

## Tests

```bash
dotnet test
```

- **Unit tests**: handlers and validators tested in isolation, with Moq for dependencies.
- **Integration tests**: the whole API runs in memory (`WebApplicationFactory`) with an in-memory database.
  They check real HTTP status codes, JWT authentication, roles and ownership rules.

No SQL Server and no secrets are needed to run the tests, which is why they also run on GitHub Actions.

## Author

**Lydia Garcia MBOUWE KAMENI**, C# / .NET backend developer

GitHub: [@LydiaKameni](https://github.com/LydiaKameni)
