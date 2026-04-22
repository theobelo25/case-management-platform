FROM node:22-bookworm-slim AS build

RUN corepack enable && corepack prepare pnpm@10.33.0 --activate

WORKDIR /workspace

COPY pnpm-lock.yaml ./
COPY apps/web/package.json ./apps/web/package.json

RUN pnpm --dir apps/web install --frozen-lockfile

COPY apps/web/ ./apps/web/

RUN pnpm --dir apps/web build

FROM nginx:1.27-alpine AS runtime

COPY docker/web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /workspace/apps/web/dist/web/browser /usr/share/nginx/html

EXPOSE 80
