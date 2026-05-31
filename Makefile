# Hood CMS — local dev quick start.
# Wraps the docker compose + dotnet flow. See DOCKER.md for the full walkthrough.

SHELL := /bin/bash
COMPOSE := docker compose
DB_SERVICE := sqlserver
APP_SERVICE := app
SA_PASSWORD := Hood_Dev_Passw0rd!

.DEFAULT_GOAL := help

help: ## Show this help
	@grep -hE '^[a-zA-Z_-]+:.*?## ' $(MAKEFILE_LIST) \
		| awk 'BEGIN{FS=":.*?## "}{printf "  \033[36m%-16s\033[0m %s\n", $$1, $$2}'

build: ## Build the whole solution (Release)
	dotnet build Hood.sln -c Release

up: ## Build + start the full stack (app + SQL Server) in the background
	$(COMPOSE) up -d --build

db-up: ## Start SQL Server only and wait until healthy
	$(COMPOSE) up -d $(DB_SERVICE)
	@echo -n "Waiting for SQL Server to be healthy"
	@until [ "$$($(COMPOSE) ps -q $(DB_SERVICE) | xargs -r docker inspect -f '{{.State.Health.Status}}' 2>/dev/null)" = "healthy" ]; do \
		printf '.'; sleep 2; \
	done; echo " ready."

run: ## Run the app natively against the Docker SQL Server (port 14331)
	ConnectionStrings__DefaultConnection="Server=localhost,14331;Database=Hood.Web;User Id=sa;Password=$(SA_PASSWORD);TrustServerCertificate=True;MultipleActiveResultSets=True" \
		dotnet run --project projects/Hood.Development

down: ## Stop containers (keeps the DB volume)
	$(COMPOSE) down

clean: ## Stop containers and drop the DB volume
	$(COMPOSE) down -v
	dotnet clean Hood.sln

logs: ## Tail app + DB container logs
	$(COMPOSE) logs -f

sql: ## Open a sqlcmd shell inside the SQL container
	$(COMPOSE) exec $(DB_SERVICE) /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '$(SA_PASSWORD)' -C

.PHONY: help build up db-up run down clean logs sql
