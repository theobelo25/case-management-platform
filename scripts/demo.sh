#!/usr/bin/env sh
set -e
cd "$(dirname "$0")/.."
exec docker compose -f docker-compose.yml -f docker-compose.demo.yml --profile demo up --build "$@"
