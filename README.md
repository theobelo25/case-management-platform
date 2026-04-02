# Case Management Platform

Monorepo for a case-management style product: an **Angular** SPA backed by an **ASP.NET Core** API. The UI is a marketing shell, authentication pages, and a protected dashboard area ready for real features.

## Repository layout

| Path | Description |
|------|-------------|
| `apps/web` | Angular 21 frontend (standalone components, lazy routes, Tailwind CSS 4) |
| `apps/api` | ASP.NET Core 10 Web API (OpenAPI + Swagger in Development, CORS for the dev server) |
| `packages/*` | Reserved for shared libraries (nothing checked in yet) |

The .NET solution file is `case-management-platform.slnx` and currently includes only `apps/api`.

## Prerequisites

- **Node.js** (LTS recommended) and **pnpm** 10.x (see root `package.json` → `packageManager`, currently `pnpm@10.33.0`)
- **.NET SDK** for **.NET 10** (`net10.0` in `apps/api/api.csproj`)

## Quick start

From the repository root:

```bash
pnpm install
pnpm dev
```

`pnpm dev` runs the Angular dev server and the API together (via `concurrently`): frontend at **http://localhost:4200**, API using the default launch profile in `apps/api/Properties/launchSettings.json`.

**API URLs in development**

- Default `dotnet run` profile (`http`): **http://localhost:5082**
- With HTTPS profile: `dotnet run --project apps/api --launch-profile https` → **https://localhost:7277** and **http://localhost:5082**

### Run apps separately

| Goal | Command |
|------|---------|
| Web only | `pnpm web:start` |
| API only | `pnpm api:start` |
| Production build (web) | `pnpm web:build` |
| Build API | `pnpm api:build` |

`pnpm api:test` runs `dotnet test` from the repo root. There is no separate test project in the solution yet, so this is a no-op until you add one.

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

- **Stack:** ASP.NET Core 10, **controllers** pipeline (`AddControllers` / `MapControllers`), **OpenAPI** (`MapOpenApi` in Development), **Swagger** (Swashbuckle: JSON + UI in Development).
- **CORS:** Policy `Frontend` allows `http://localhost:4200` for local Angular development.
- **Configuration:** `appsettings.json` commits **no secrets**. For local development, set **`Database:ConnectionString`** and **`Jwt:Secret`** with [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) from `apps/api` (see `appsettings.Development.example.json` for the shape). For production, use environment variables or your host’s secret store (e.g. `Database__ConnectionString`, `Jwt__Secret`).

The API validates these values at startup; extend `Program.cs` and add controllers as you build case-management endpoints.

## Tooling

- **pnpm** workspaces: `pnpm-workspace.yaml` includes `apps/*` and `packages/*`.
- **Editor:** `.editorconfig` at the repo root.
- **Formatting:** Prettier is a dev dependency under `apps/web` (run via `pnpm --dir apps/web exec prettier` if you add config or scripts).

## License

ISC (see root `package.json`).

---

Author: Theodore Belo (from `package.json`).
