# FitZone

A gym management system: a layered **ASP.NET Core 8 REST API** (EF Core, SQL Server, JWT) and an **MVC/Razor web UI** that talks to it over HTTP.

Members browse trainers and workout sessions, buy a membership, book sessions and check in at the gym. Admins manage trainers, sessions, plans and users, and see every booking.

> Georgian version: [README.ka.md](README.ka.md)

<!-- Add screenshots here, e.g. ![Trainers](docs/trainers.png) -->

## Features

- Registration with 6-digit email verification, login, JWT access tokens (15 min) and refresh tokens (7 days), logout
- Roles: **Guest**, **User**, **Admin**
- Trainers: list, search, details, admin CRUD (soft delete)
- Workout sessions: schedule, filter by trainer and date, admin CRUD, overlap check per trainer
- Memberships: plans, activation, remaining days/visits, cancellation, history
- Bookings: book / cancel, capacity and overlap checks, upcoming and history views
- Visits: check-in (once per 12 hours), history, admin edit/delete with refund of a visit on limited plans
- Profile page for members; admin Users section (search, details, delete user)
- Global error handling with RFC 7807 problem details (400 / 401 / 403 / 404 / 409)

## Tech stack

ASP.NET Core 8 (Web API + MVC/Razor), Entity Framework Core, SQL Server, JWT bearer auth, AutoMapper, FluentValidation, Swagger/OpenAPI, Bootstrap, xUnit, Moq, FluentAssertions, Docker.

## Architecture

| Project | Responsibility |
|---|---|
| `FitZone.Domain` | Entities, enums, repository interfaces, custom exceptions (no dependencies) |
| `FitZone.Application` | Services (business rules), DTOs, FluentValidation validators, AutoMapper profile |
| `FitZone.Infrastructure` | EF Core `DbContext`, migrations, repositories, JWT service, email senders |
| `FitZone.WebApi` | Controllers, Swagger, authorization, global exception handler |
| `FitZone.Web` | UI: Razor views, cookie auth, typed `HttpClient` for the API |
| `FitZone.Testing` | xUnit tests (services with Moq, repositories with EF InMemory) |

Dependencies point inward: `WebApi -> Application -> Domain` and `Infrastructure -> Application/Domain`. The UI knows the API only through HTTP and never touches the database.

## Run locally

Requirements: .NET 8 SDK, SQL Server (LocalDB, Express or Docker), `dotnet-ef` (`dotnet tool install --global dotnet-ef`), trusted dev certificate (`dotnet dev-certs https --trust`).

```bash
# 1. (optional) override connection string / JWT key with user secrets, never commit real secrets
cd FitZone.WebApi
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your connection string>"
dotnet user-secrets set "JwtTokenSettings:SecretKey" "<random string, 32+ chars>"
cd ..

# 2. create the database (applies migrations and seed data)
dotnet ef database update --project FitZone.Infrastructure --startup-project FitZone.WebApi

# 3. API  -> https://localhost:7284/swagger
dotnet run --project FitZone.WebApi --launch-profile https

# 4. UI (second terminal) -> https://localhost:7300
dotnet run --project FitZone.Web --launch-profile https
```

In Visual Studio use *Configure Startup Projects -> Multiple startup projects* and start both `FitZone.WebApi` and `FitZone.Web`.

### Accounts

| Role | Login | Password |
|---|---|---|
| Admin (seed data) | `admin@fitzone.local` | `Admin123!` |
| User | register in the UI | your choice (8+ chars, Latin letters, one upper-case letter, one symbol) |

The admin account is demo seed data. Change it before any real deployment.

### Email verification

New users must verify their email before logging in. By default (`UseRealEmail: false`) no email is sent: the API prints the message to the console. Look for `FAKE EMAIL to ...` and use the 6-digit code (valid 15 minutes, resend after 60 seconds).

To send real email set `UseRealEmail` to `true` and store the Gmail credentials in user secrets (never in git):

```bash
cd FitZone.WebApi
dotnet user-secrets set "EmailSettings:SenderEmail" "you@gmail.com"
dotnet user-secrets set "EmailSettings:AppPassword" "<Gmail app password>"
```

## Run with Docker

> The Docker setup was written without being run on the author's machine yet. If something fails, please open an issue.

```bash
cp .env.example .env        # then edit the password and JWT key
docker compose up --build
```

| Service | URL |
|---|---|
| Web UI | http://localhost:8081 |
| API + Swagger | http://localhost:8080/swagger |

The stack starts SQL Server 2022, the API and the UI. The API applies migrations and seed data at startup when `Database__MigrateOnStartup=true` (set in `docker-compose.yml`; off by default everywhere else). Emails are printed to the API logs (`docker compose logs api`). Stop with `docker compose down`; add `-v` to delete the database volume.

## Tests

```bash
dotnet test
```

## Business rules (enforced by the API)

- Booking needs an active membership with visits left, a session that has not started, a free seat, no existing booking for that session and no time overlap with the user's other bookings. Sessions starting after the membership expires are rejected.
- One active membership per user. On limited plans each check-in uses one visit; when none are left the plan counts as finished.
- Check-in is allowed once per 12 hours (otherwise 409).
- Only admins can edit (`PUT api/visit/{id}`) or delete (`DELETE api/visit/{id}`) a visit; deleting a visit on an active limited plan refunds it.
- A trainer cannot have two overlapping sessions. Trainers are soft-deleted, and only if they have no future sessions. Plans and sessions with bookings cannot be deleted. Bookings are cancelled, never deleted.
- Admins cannot be deleted; a user with active future bookings cannot be deleted.

## UI map

| Page | Who | Main API endpoints |
|---|---|---|
| `/Trainers`, `/Trainers/Details/{id}` | everyone | `GET trainers`, `trainers/search`, `trainers/{id}` |
| `/Sessions`, `/Sessions/Details/{id}` | logged-in | `GET training-sessions`, `POST/DELETE bookings` |
| `/Plans`, `/Plans/Details/{id}` | everyone / users | `GET memberships/plans`, `POST memberships/activate` |
| `/Membership` | user | `GET/DELETE memberships/my`, `POST visit/check-in`, `GET visit/my` |
| `/Bookings/My`, `/Bookings/All` | user / admin | `GET bookings/my`, `GET bookings` |
| `/Profile` | logged-in | `GET profile`, `memberships/history`, `bookings/upcoming` |
| `/Users`, `/Users/Details/{id}` | admin | `GET users`, `users/{id}/...`, `DELETE users/{id}`, `PUT/DELETE visit/{id}` |
| `/Account/Register`, `Verify`, `Login` | guests | `POST auth/register`, `verify-email`, `login` |

## Time zone and currency

The API stores and returns **UTC**. The UI converts to the zone in `FitZone.Web/appsettings.json` (`Display:TimeZone`, default `Asia/Tbilisi`); `Display:Currency` sets the currency symbol.

## Known limitations

- Seat and visit counters are not protected against concurrent requests (two simultaneous requests can take the last seat or visit).
- Only one refresh token is stored per user, so logging in on a second device signs the first one out once its access token expires.
- A membership record does not store the price paid; the UI shows the current price of the plan with the same name.
- No CI pipeline yet.
