# Case Management Platform

Monorepo for a case-management style product: an **Angular** SPA backed by an **ASP.NET Core** API. The UI is organized as a marketing shell, authentication pages, and a protected app area ready for real features.

## Repository layout

| Path | Description |
|------|-------------|
| `apps/web` | Angular 21 frontend (standalone components, Tailwind CSS 4) |
| `apps/api` | ASP.NET Core 10 Web API (OpenAPI + Swagger in development, CORS for the dev server) |
| `packages/*` | Reserved for shared libraries (empty until you add packages) |

The .NET solution file is `case-management-platform.slnx` (includes `apps/api`).

## Prerequisites

- **Node.js** (LTS recommended) and **pnpm** `10.x` (see `package.json` → `packageManager`)
- **.NET SDK** matching **.NET 10** (`net10.0` in `apps/api/api.csproj`)

## Quick start

From the repository root:

```bash
pnpm install
pnpm dev
```

`pnpm dev` runs the Angular dev server and the API together (via `concurrently`): frontend at **http://localhost:4200**, API on the port configured in `apps/api/Properties/launchSettings.json` (HTTPS by default).

### Run apps separately

| Goal | Command |
|------|---------|
| Web only | `pnpm web:start` |
| API only | `pnpm api:start` |
| Production build (web) | `pnpm web:build` |
| Build API | `pnpm api:build` |

`pnpm api:test` runs `dotnet test` from the repo root once you add test projects to the solution.

### Web tests

```bash
pnpm --dir apps/web test
```

Uses the Angular CLI **Vitest** builder (`ng test`).

## Frontend (`apps/web`)

- **Stack:** Angular 21, TypeScript 5.9, RxJS, **Tailwind CSS 4** (PostCSS), **Vitest** for unit tests.
- **Structure:** Feature-based routes under `src/app/features`, shared layouts under `src/app/layouts`, small shared utilities (e.g. validators) under `src/app/shared`.
- **Routes (high level):**
  - `/` — Landing (public layout: header, footer, hero, features, how-it-works).
  - `/demo` — Demo-style marketing page.
  - `/auth/sign-in`, `/auth/sign-up` — Auth layout and reactive forms (sign-in / sign-up are placeholders until wired to the API).
  - `/app` — Dashboard shell (protected layout: sidebar + header); home is a starter page.
  - Unknown paths under the public section resolve to a **404** page inside the public layout.

> **Routing note:** Root routes declare **`auth`** and **`app`** before the empty **`''`** landing route so segments like `/app` are not swallowed by the landing area’s wildcard 404.

## API (`apps/api`)

- **Stack:** ASP.NET Core 10, controllers pipeline enabled (`MapControllers`), **Swagger / OpenAPI** in Development.
- **CORS:** Policy `Frontend` allows `http://localhost:4200` for local Angular development.

There are no domain controllers checked in yet; extend `Program.cs` and add controllers as you build out case-management endpoints.

## Tooling

- **pnpm** workspaces: `pnpm-workspace.yaml` includes `apps/*` and `packages/*`.
- **Editor:** `.editorconfig` at the repo root.
- **Formatting:** Prettier is listed under `apps/web` (run via `pnpm --dir apps/web exec prettier` if you add config/scripts).

## License

ISC (see root `package.json`).

---

Author: Theodore Belo (from `package.json`).
