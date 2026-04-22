# Case Management Platform

[![CI](https://github.com/theobelo25/case-management-platform/actions/workflows/ci.yml/badge.svg)](https://github.com/theobelo25/case-management-platform/actions/workflows/ci.yml)

Portfolio full-stack app: a small **case management** workspace with organizations, members, SLAs, and case lifecycle (create, assign, comment, archive, analytics).

## Stack

| Layer | Tech |
|--------|------|
| API | ASP.NET Core **.NET 10**, minimal hosting, controllers under `/api/*` |
| Data | **PostgreSQL**, **EF Core** (`CaseManagementDbContext`), migrations in Infrastructure |
| Auth | **JWT** access tokens + **httpOnly** refresh cookies, Argon2 password hashing |
| Realtime | **SignalR** hub at `/hubs/notifications` |
| Web | **Angular 21**, dev proxy to the API |

Architecture: **Domain** → **Application** (use cases, ports) → **Infrastructure** (EF, auth adapters) → **Api** (HTTP, validation, OpenAPI/Swagger in Development).

### Threat model lite (reviewer notes)

This is **not** a full STRIDE analysis; it records **what the API actually does** so reviewers do not have to read controllers and DI line by line.

| Area | Choices in this repo |
|------|----------------------|
| **Authentication** | **JWT** access tokens (Bearer, short-lived) issued after login/register/refresh. **Refresh** tokens are **opaque**, **stored server-side** (hashed at rest), and delivered to the browser in an **`httpOnly`** cookie (**`SameSite=Lax`**, **`Secure`** when not Development, path **`/`**). Passwords hashed with **Argon2**. See `RefreshTokenCookieService`, `AuthController`, Infrastructure JWT + refresh token registration. |
| **Authorization** | **Default fallback policy** requires an authenticated user for every endpoint unless explicitly **`[AllowAnonymous]`** (`AuthorizationPolicies`). **Organization context** comes from the user’s **`ActiveOrganizationId`** and/or an optional **claimed organization id** on case requests; **`CaseAccessResolver`** resolves the active org, and case reads/writes load data **scoped to that organization id** (e.g. `GetByIdsInOrganizationAsync`, `caseEntity.OrganizationId` checks in `CaseQueryService`). **Org-level roles** (owner/admin vs member) for sensitive org operations are enforced in **`OrganizationPolicies`** + repositories—not only route-level “logged in”. |
| **Rate limiting** | **`AddRateLimiter`** with two policies: **`auth`** (partitioned by **client IP**) on register/login/refresh; **`cases`** (partitioned by **authenticated user id**, else IP) on case-creation routes. Limits and windows are **configurable** under **`RateLimiting`** (`RateLimitingOptions`, defaults e.g. 20 requests per 60s per partition). **Not** applied to every API route—only where **`[EnableRateLimiting(...)]`** is set (see `AuthController`, `CaseCreationController`). Rejection returns **429** with ProblemDetails (`OnRateLimitRejected`). |
| **CORS** | **Explicit allowlist** of origins from config (**`Cors:AllowedOrigins`**), **`AllowCredentials`**, required for cookie + Angular same-site usage. See `AddFrontendCorsPolicy`. |
| **TLS / reverse proxy** | **`UseHsts`** when **`Hsts:Enabled`** (max-age, subdomains, preload, excluded hosts via **`HstsSettings`** / `HstsConfiguration`). **`UseForwardedHeaders`** when **`ForwardedHeaders:Enabled`**, so **`X-Forwarded-For`** / **`X-Forwarded-Proto`** (and optionally **`X-Forwarded-Host`**) match the real client when behind a proxy; **known proxies/networks** are configurable (`ForwardedHeadersSettings`, `ForwardedHeadersConfiguration`). HTTPS redirection is enabled in non-dev when HTTPS is configured (`WebApplicationExtensions`). |

**Out of scope for this lite list (not implied):** blanket rate limits on all endpoints, separate service-to-service auth, WAF rules, dependency scanning, secrets rotation runbooks, formal penetration test, or **CSP** for the API (typically an Angular concern). Treat those as deployment and process layers on top of the above.

**API errors:** Failures use **`application/problem+json`** ([RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) `ProblemDetails`) with a **`traceId`** for correlation—validation, auth, and domain failures are mapped consistently (see `GlobalExceptionHandler` and `IExceptionToProblemDetailsMapper` in the API project). OpenAPI annotations on controllers use `ProducesResponseType` so Swagger reflects success and error shapes.

## CI & quality

On push/PR to `main`, GitHub Actions builds the **API** solution with **warnings treated as errors**, runs all **.NET test** projects (domain, application, architecture rules, and HTTP integration tests where the host is available), collects **Cobertura** coverage with an emphasis on **Application** and **Api** assemblies, and uploads **trx** + coverage XML as workflow artifacts. The **Angular** job installs dependencies, lints, runs **Vitest** via **`ng test`** (`@angular/build:unit-test`), then builds the app.

### Tests & coverage (intent)

**API:** Automated tests prioritize **application services and policy/authorization paths**, **HTTP behavior** (including ProblemDetails + `traceId`), and **architecture** constraints—not blanket line coverage on DTOs or thin mappers.

**Web:** **Vitest** runs through **`ng test`** (Angular’s **`@angular/build:unit-test`** builder). Add **`*.spec.ts`** next to the code you care about; optional coverage: `pnpm --filter web exec ng test --coverage --watch=false`. How much of the UI is exercised grows with those specs—same idea as the API side: cover meaningful behavior, not every template line.

## API (current surface)

All JSON endpoints are under **`/api`** (except health and SignalR):

- **`/api/auth`** — register, login, refresh, logout, current user (`me`)
- **`/api/cases`** — list/detail (cursor filters), create/bulk, patch, comments, assignee, delete/archive, analytics (`volume-over-time`, `first-response-time-over-time`, `count-by-status`)
- **`/api/organizations`** — list/detail, create, SLA policy, archive/unarchive, members, ownership transfer
- **`/api/users`** — search (for invites/assignment)

**Health / observability**

- **Logs:** In **Production**, console output uses the **JSON** formatter with **scopes enabled** so each line can include request **`TraceId`** (same value as `traceId` on `application/problem+json` responses—`Activity.Current?.Id` when present, otherwise ASP.NET Core’s request trace id).
- **Endpoints (all `GET`, anonymous):**
  - **`/api/health`** — Runs every registered check (process **liveness** + **PostgreSQL** readiness).
  - **`/api/health/live`** — **Liveness** only: confirms the process is running (no I/O). Use for orchestrator **liveness** probes (e.g. Kubernetes `livenessProbe`).
  - **`/api/health/ready`** — **Readiness** only: **EF Core `DbContext` ping** against PostgreSQL (`AddDbContextCheck`). Use when the app must not receive traffic until the database is reachable (e.g. Kubernetes `readinessProbe`).

Configure **`ConnectionStrings:DefaultConnection`** and **`Jwt:SigningKey`** (see Infrastructure registration); use **user secrets** for local API runs.

### Demo: API + database (one command)

For reviewers who want **Postgres + API** with **no manual migration or secrets wiring**:

```bash
pnpm demo
```

Same stack without pnpm: **`scripts/demo.sh`** (Unix) or **`scripts\demo.cmd`** (Windows). This runs **`docker compose -f docker-compose.yml -f docker-compose.demo.yml --profile demo up --build`** (see `docker-compose.demo.yml`). On startup the API applies **EF Core migrations** (`Database:ApplyMigrationsOnStart`) and, when **`ASPNETCORE_ENVIRONMENT=Development`**, optionally seeds a **demo user** (`Demo:Seed`):

| | |
|--|--|
| **Email** | `demo@example.local` |
| **Password** | `DemoPass1!` |

Registration already creates a **personal organization** and owner membership; the demo seed uses the same path. Override email/password via environment variables `Demo__UserEmail` and `Demo__UserPassword` in the compose overlay if needed.

**HTTP flows:** `apps/api/docs/demo.http` — **register → login → `GET /api/auth/me` → create/list/get case** (works with **VS Code REST Client** or **JetBrains HTTP Client**). Set `@host` if the API is not on `http://localhost:5202`. The project `.http` stub in the API project points here.

**Postman / Bruno / Insomnia:** in **Development**, import **`/swagger/v1/swagger.json`** (Swagger) or the OpenAPI endpoint from **`MapOpenApi`** (same environment), or recreate the requests from `demo.http`.

## Run locally

**Database:** `pnpm docker:db` (or `docker compose up -d`) — Postgres on port **5432**. Copy `.env.example` → `.env` if you customize credentials.

**Apply migrations** (Postgres up), if you are **not** using `pnpm demo` or `Database:ApplyMigrationsOnStart`:

```bash
dotnet ef database update --project apps/api/src/CaseManagement.Infrastructure/CaseManagement.Infrastructure.csproj --startup-project apps/api/src/CaseManagement.Api/CaseManagement.Api.csproj
```

**API:** `pnpm api:run` — default HTTP **`http://localhost:5202`** (see `launchSettings.json`).

**Web:** `pnpm web:start` — **`http://localhost:4200`**, proxies `/api` and `/hubs` to the API.

**Optional — full stack in Docker:** `pnpm docker:full` (Postgres + API + Angular; see `docker-compose.yml`).

**Tests:** `pnpm api:test` runs `dotnet test` on `apps/api/CaseManagement.slnx` (see **Tests & coverage** above). For coverage locally: `dotnet test apps/api/CaseManagement.slnx -c Release --collect:"XPlat Code Coverage" --settings apps/api/coverlet.runsettings`. **Web:** `pnpm web:test` (interactive watch in a terminal) or `pnpm web:test:ci` for a single run (e.g. scripts/CI).

## Deploy on Dokploy (Docker Compose)

This repository includes a production-focused compose file at `docker-compose.dokploy.yml`:

- API image: `docker/api/Dockerfile` (`dotnet publish`, no file watchers)
- Web image: `docker/web/Dockerfile` (Angular production build served by Nginx)
- Edge route: expose only the `web` service (port `80` in-container)
- API + SignalR are proxied by Nginx from `/api/*` and `/hubs/*` to the internal `api` service

### 1) Configure environment variables

Start from `.env.dokploy.example` and set real secrets/hostnames in Dokploy:

- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `POSTGRES_DB`
- `JWT_SIGNING_KEY`
- `PUBLIC_APP_ORIGIN` (for API CORS, e.g. `https://cases.example.com`)

### 2) Create the Dokploy compose app

Use `docker-compose.dokploy.yml` as the compose file.

Set your Dokploy domain to the `web` service.

### 3) Runtime behavior

- API runs with `ASPNETCORE_ENVIRONMENT=Production`
- `Database__ApplyMigrationsOnStart=true` is enabled for automatic EF migrations on boot
- `ForwardedHeaders__Enabled=true` and `Hsts__Enabled=true` are enabled for reverse-proxy/TLS deployments

For local smoke-testing of the Dokploy stack outside Dokploy:

```bash
docker compose -f docker-compose.dokploy.yml --env-file .env.dokploy.example up --build
```
