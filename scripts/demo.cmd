@echo off
setlocal
cd /d "%~dp0\.."
docker compose -f docker-compose.yml -f docker-compose.demo.yml --profile demo up --build %*
