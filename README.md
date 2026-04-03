# Case Management Platform

Monorepo for a case-management style product: an **Angular** SPA backed by an **ASP.NET Core** API. The UI is a marketing shell, authentication pages, and a protected dashboard area ready for real features.

## Repository layout

| Path | Description |
| ---- | ----------- |
| `apps/web` | Angular 21 frontend (standalone components, lazy routes, Tailwind CSS 4) |
| `apps/api` | .NET 10 backend split into layered projects (see below) |
| `packages/*` | Reserved for shared libraries (nothing checked in yet) |

### Backend projects (`apps/api`)

| Project | Role |
| ------- | ---- |
| `CaseManagement.Api` | ASP.NET Core host: controllers, middleware, OpenAPI/Swagger, JWT bearer setup |
| `CaseManagement.Application` | Use cases and application services (e.g. auth) |
| `CaseManagement.Domain` | Domain model |
| `CaseManagement.Infrastructure` | EF Core (Npgsql), repositories, JWT/password services, health checks |
| `CaseManagement.ArchitectureTests` | xUnit + NetArchTest rules for layer dependencies |

The repo-level solution is `case-management-platform.slnx` and references all of the projects above. There is also `apps/api/CaseManagement.slnx` if you prefer working from the API folder only.

## Prerequisites

- **Node.js** (LTS recommended) and **pnpm** 10.x (see root `package.json` → `packageManager`, currently `pnpm@10.33.0`)
- **.NET SDK** for **.NET 10** (`net10.0` in `apps/api/CaseManagement.Api/CaseManagement.Api.csproj`)
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
| Production build (web) | `pnpm web:build` |
| Build API (whole solution) | `pnpm api:build` |

`pnpm api:test` runs `dotnet test` from the repo root. Today that executes **CaseManagement.ArchitectureTests** (xUnit + NetArchTest layering rules).

### Web tests

```bash
pnpm --dir apps/web test
```

Uses the Angular CLI **Vitest** builder (`ng test`). Sample specs: `app.spec.ts`, `home.component.spec.ts`.

## Frontend (`apps/web`)

- **Stack:** Angular 21, TypeScript 5.9, RxJS, **Tailwind CSS 4** (PostCSS via `@tailwindcss/postcss`), **Vitest** for unit tests, **`@angular/build`** application builder.
- **Structure:** Feature areas under `src/app/features` (`landing`, `auth`, `dashboard`), layouts under `src/app/layouts`, shared pieces under `src/app/shared`.
- **Routes (high level):**
  - `/` — Landing (public layout: header, footer, hero, features, how-it-works).
  - `/demo` — Demo-style marketing page (same public layout).
  - `/auth` — Auth layout; **`/auth` redirects to `/auth/sign-in`**. `/auth/sign-in` and `/auth/sign-up` use reactive forms (placeholders until wired to the API).
  - `/app` — Dashboard (protected layout: sidebar + header); default child is a starter home page.
  - Unknown paths under the public section resolve to a **404** page inside the public layout.

> **Routing note:** Root routes declare **`auth`** and **`app`** before the empty **`''`** landing route so segments like `/app` are not swallowed by the landing area’s wildcard 404.

## API (`apps/api`)

- **Shape:** Controller-based ASP.NET Core 10 host with **Application** / **Domain** / **Infrastructure** projects; startup wires `AddWeb()`, `AddApplication()`, `AddInfrastructure()`, and JWT bearer authentication in `CaseManagement.Api/Program.cs`.
- **Persistence:** **EF Core** with **Npgsql** (`UseNpgsql`). In Development, the database is initialized via a development initializer (see `DevelopmentDatabaseExtensions` / `IDevelopmentDatabaseInitializer`).
- **Stack (host):** **OpenAPI** (`MapOpenApi` in Development), **Swagger** (Swashbuckle: JSON + UI in Development), **Problem Details** and a **global exception handler**, **health checks** at **`/health`** (including PostgreSQL).
- **CORS:** Policy `Frontend` allows `http://localhost:4200` for local Angular development.
- **Auth (HTTP):**
  - `POST /auth/sign-in` — issue tokens (anonymous).
  - `GET /auth/me` — current user (requires JWT).
- **Configuration:** `appsettings.json` commits **no secrets**. For local development, set **`Database:ConnectionString`** and **`Jwt:Secret`** with [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) from `apps/api/CaseManagement.Api` (see `appsettings.Development.example.json` for the shape). For production, use environment variables or your host’s secret store (e.g. `Database__ConnectionString`, `Jwt__Secret`).

The API validates JWT options at startup and requires a non-empty database connection string before registering EF Core.

## Tooling

- **pnpm** workspaces: `pnpm-workspace.yaml` includes `apps/*` and `packages/*`.
- **Editor:** `.editorconfig` at the repo root.
- **Formatting:** Prettier is a dev dependency under `apps/web` (run via `pnpm --dir apps/web exec prettier` if you add config or scripts).

## License

ISC (see root `package.json`).

---

Author: Theodore Belo (from `package.json`).
