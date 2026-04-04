# Web

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.2.6.

## API base URL (build-time)

`__WEB_API_BASE_URL__` is set by esbuild **`define`**:

- **`angular.json`:** `development` → `http://localhost:5082`; **`production` → `""`** (same origin as the deployed site).
- **`scripts/ng-with-web-env.mjs`:** If **`API_BASE_URL`** is set in the environment or `apps/web/.env`, it adds `--define` and overrides the above. If unset, the configuration-specific values from `angular.json` apply.

Use `pnpm start` / `pnpm build` from this package (they use the script). For a custom API origin in CI or production, set `API_BASE_URL` (no trailing slash).

## Development server

To start a local development server, run:

```bash
pnpm start
```

(or `ng serve --configuration development` if you are not using the pnpm script). Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
pnpm build
```

This runs the production configuration by default (`dist/` output). The bundle does **not** embed `http://localhost:5082` unless you set `API_BASE_URL` or build with `--configuration development`.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
