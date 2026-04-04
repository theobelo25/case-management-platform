# Case Management Platform

Monorepo for a case-management style product: an **Angular** SPA backed by an **ASP.NET Core** API. The UI includes a marketing shell, sign-in / sign-up wired to the API, JWT access tokens with **HTTP-only refresh cookies**, and a protected dashboard area ready for product features.

## Repository layout

| Path | Description |
| ---- | ----------- |
| `apps/web` | Angular 21 frontend (standalone components, lazy routes, Tailwind CSS 4) |
| `apps/api` | .NET 10 backend split into layered projects (see below) |
| `packages/*` | Reserved for shared libraries (nothing checked in yet) |

### Backend projects (`apps/api`)

| Project | Role |
| ------- | ---- |
| `CaseManagement.Api` | ASP.NET Core host: controllers, middleware, OpenAPI/Swagger, JWT bearer, FluentValidation auto-validation, rate limiting (refresh/sign-out), CORS |
| `CaseManagement.Application` | Use cases and application services (auth, refresh rotation, ports) |
| `CaseManagement.Domain` | Domain model |
| `CaseManagement.Infrastructure` | EF Core (Npgsql), users/refresh sessions, JWT and refresh-token services, password hashing, health checks |
| `CaseManagement.Application.Tests` | xUnit tests for application services |
| `CaseManagement.Infrastructure.Tests` | xUnit tests for infrastructure adapters (e.g. refresh token handling) |
| `CaseManagement.Api.Tests` | xUnit tests for API-layer concerns (e.g. validators) |
| `CaseManagement.ArchitectureTests` | xUnit + NetArchTest rules for layer dependencies |

The repo-level solution is `case-management-platform.slnx` and references all projects above. There is also `apps/api/CaseManagement.slnx` if you prefer working from the API folder only.

## Prerequisites

- **Node.js** 22.x (matches CI) and **pnpm** 10.x (see root `package.json` → `packageManager`, currently `pnpm@10.33.0`)
- **.NET SDK** for **.NET 10** (`net10.0` in API projects)
- **PostgreSQL** for the API (connection string in configuration; see below)

## Quick start

From the repository root:

```bash
pnpm install
pnpm dev
```

`pnpm dev` runs the Angular dev server and the API together (via `concurrently`): frontend at **http://localhost:4200**, API using the default launch profile in `apps/api/CaseManagement.Api/Properties/launchSettings.json`.

**API URLs in development**

- Default `dotnet run` profile (`http`): **http://localhost:5082**
- With HTTPS profile: `dotnet run --project apps/api/CaseManagement.Api --launch-profile https` → **https://localhost:7277** and **http://localhost:5082**

### Run apps separately

| Goal | Command |
| ---- | ------- |
| Web only | `pnpm web:start` |
| API only | `pnpm api:start` |
| Production build (web) | `pnpm web:build` (same-origin API base by default; see **Web API base URL** below) |
| Build API (whole solution) | `pnpm api:build` |
| Test API (all test projects) | `pnpm api:test` (`dotnet test` from repo root discovers test projects; CI uses `dotnet test case-management-platform.slnx`) |

### Web tests

```bash
pnpm --dir apps/web test
```

Uses the Angular CLI **Vitest** builder (`ng test`). Specs live next to features and under `src/app/core` (e.g. `app.spec.ts`, auth/guard/interceptor specs, `home.component.spec.ts`).

## Frontend (`apps/web`)

- **Web API base URL:** The SPA reads the API origin from a compile-time constant `__WEB_API_BASE_URL__`. **Production** builds default to an **empty** base (requests go to the same origin as the static app, e.g. behind a reverse proxy). **Development** uses `http://localhost:5082` via `angular.json` → `build.configurations.development.define`. To override any configuration, set **`API_BASE_URL`** in `apps/web/.env` (or the environment); `pnpm web:start` / `pnpm web:build` run `scripts/ng-with-web-env.mjs`, which passes `--define` only when that variable is non-empty. Raw `ng build` / `ng serve` without the wrapper use the same `angular.json` rules. CI runs a production web build so release artifacts are not built with localhost baked in unless you opt in via env.

- **Auth client:** Sign-in and sign-up call the API; access tokens are stored in session (in-memory) and attached by an HTTP interceptor. Refresh uses **`POST /auth/refresh`** with **credentials** so the HTTP-only refresh cookie is sent; a refresh coordinator avoids stampedes on 401. Additional pieces include 401 handling, a dedicated refresh interceptor for API calls, session restore on load, and **`AuthGuard`** / **`GuestGuard`** on routes.

- **Stack:** Angular 21, TypeScript 5.9, RxJS, **Tailwind CSS 4** (PostCSS via `@tailwindcss/postcss`), **Vitest** for unit tests, **`@angular/build`** application builder, **ESLint** + **Prettier** (see **Tooling**).

- **Structure:** Feature areas under `src/app/features` (`landing`, `auth`, `dashboard`), layouts under `src/app/layouts`, shared pieces under `src/app/shared`, cross-cutting auth/API/navigation under `src/app/core`.

- **Routes (high level):**
  - `/` — Landing (public layout: header, footer, hero, features, how-it-works).
  - `/demo` — Demo-style marketing page (same public layout).
  - `/auth` — Auth layout; **`/auth` redirects to `/auth/sign-in`**. `/auth/sign-in` and `/auth/sign-up` use reactive forms and the live API.
  - `/app` — Dashboard (protected layout: sidebar + header); default child is a starter home page.
  - Unknown paths under the public section resolve to a **404** page inside the public layout.

> **Routing note:** Root routes declare **`auth`** and **`app`** before the empty **`''`** landing route so segments like `/app` are not swallowed by the landing area’s wildcard 404.

## API (`apps/api`)

- **Shape:** Controller-based ASP.NET Core 10 host with **Application** / **Domain** / **Infrastructure** projects; startup wires `AddWeb(builder.Configuration)`, `AddApplication()`, `AddInfrastructure()`, and JWT bearer authentication in `CaseManagement.Api/Program.cs`.

- **Persistence:** **EF Core** with **Npgsql**. Users and **refresh sessions** (rotating refresh tokens, optional client audit fields) live in PostgreSQL. In Development, the database is initialized via a development initializer (see `DevelopmentDatabaseExtensions` / `IDevelopmentDatabaseInitializer`).

- **Stack (host):** **OpenAPI** (`MapOpenApi` in Development), **Swagger** (Swashbuckle: JSON + UI in Development), **FluentValidation** with automatic MVC validation, **Problem Details** and a **global exception handler**, **ASP.NET Core rate limiting** on sensitive auth endpoints (fixed window per client IP for refresh/sign-out), **health checks** at **`/health`** (including PostgreSQL).

- **CORS:** Policy `Frontend` reads **`Cors:AllowedOrigins`** from configuration (`CorsOptions`). When origins are configured, the policy uses **`AllowCredentials()`** so the SPA can send cookies on `/auth/*`. `appsettings.Development.json` typically includes `http://localhost:4200` for the Angular dev server; base `appsettings.json` uses an empty list (set origins per environment when you deploy).

- **Auth (HTTP):** Access token is a **JWT** (Bearer). **Refresh token** is stored in an **HTTP-only, Secure, SameSite=Lax** cookie (name/path from `Jwt:RefreshToken`). Endpoints:

  | Method | Path | Notes |
  | ------ | ---- | ----- |
  | `POST` | `/auth/sign-in` | Anonymous; sets refresh cookie; returns access token payload. |
  | `POST` | `/auth/sign-up` | Anonymous; conflict if email exists; sets refresh cookie. |
  | `GET` | `/auth/me` | Requires JWT. |
  | `POST` | `/auth/refresh` | Anonymous; uses refresh cookie; rate limited; returns new access payload and rotated cookie. |
  | `POST` | `/auth/sign-out` | Anonymous or authenticated; optional body `revokeAllSessions`; clears cookie; rate limited. |

- **Configuration:** `appsettings.json` commits **no secrets**. Shape (see committed file for defaults):

  - **`Database:ConnectionString`** — required for EF Core.
  - **`Jwt`** — `Issuer`, `Audience`, `Secret`, `ExpiryMinutes`, and nested **`RefreshToken`**: `CookieName`, `ExpiryDays`, `CookiePath` (bound as section `Jwt:RefreshToken`).
  - **`Cors:AllowedOrigins`** — array of allowed frontend origins.

  For local development, use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) from `apps/api/CaseManagement.Api` (see `appsettings.Development.example.json` for the same `Jwt` / `Jwt:RefreshToken` shape as `appsettings.json`, with placeholder values). For production, use environment variables or your host’s secret store (e.g. `Database__ConnectionString`, `Jwt__Secret`). Adjust **`Cors:AllowedOrigins`** for each deployed frontend origin.

The API validates JWT and refresh options at startup (including a minimum UTF-8 byte length for the HMAC secret suitable for HS256) and requires a non-empty database connection string before registering EF Core.

## Tooling

- **pnpm** workspaces: `pnpm-workspace.yaml` includes `apps/*` and `packages/*`.
- **Editor:** `.editorconfig` at the repo root.
- **Web:** Under `apps/web` — **`pnpm --dir apps/web format`** (Prettier), **`pnpm --dir apps/web lint`** (ESLint), **`pnpm --dir apps/web format:check`** for CI parity.
- **API:** **`dotnet format case-management-platform.slnx`** (CI uses `--verify-no-changes`). **`dotnet build`** / **`dotnet test`** against `case-management-platform.slnx` in CI.
- **API analyzers:** `apps/api/Directory.Build.props` sets `EnableNETAnalyzers` and `AnalysisLevel` `latest` for all projects under `apps/api`.

## License

ISC (see root `package.json`).

---

Author: Theodore Belo (from `package.json`).
