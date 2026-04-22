FROM node:22-bookworm-slim AS build

RUN corepack enable && corepack prepare pnpm@10.33.0 --activate

WORKDIR /workspace

# Copy workspace metadata and app manifest first for better layer caching
COPY pnpm-lock.yaml pnpm-workspace.yaml ./

COPY apps/web/package.json ./apps/web/package.json

# Install only dependencies needed for the web app, honoring lockfile
RUN pnpm install --frozen-lockfile --filter ./apps/web...

# Copy app source and build
COPY apps/web/ ./apps/web/

RUN pnpm --dir apps/web build

FROM nginx:1.27-alpine AS runtime

COPY docker/web/nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /workspace/apps/web/dist/web/browser /usr/share/nginx/html

EXPOSE 80
