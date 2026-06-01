# =============================================================================
# Hood CMS — Docker build (local dev / containerised run)
# Runs the Hood.Development web host (the runnable app; the rest are libraries).
# Usage: docker compose up --build
# Target framework: net9.0 (interim baseline — HOOD-57 takes this to net10).
# =============================================================================

# ---------------------------------------------------------------------------
# Stage 1: restore (cached unless a .csproj or the .sln changes)
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution + central build files + every project file first so restore layers cache well.
# Directory.Build.props carries the TargetFramework and global.json pins the SDK — both are
# required for `dotnet restore` to resolve, so they must be copied before it runs.
COPY Hood.sln Directory.Build.props global.json ./
COPY projects/Hood/Hood.csproj                             projects/Hood/
COPY projects/Hood.Core/Hood.Core.csproj                   projects/Hood.Core/
COPY projects/Hood.Admin/Hood.Admin.csproj                 projects/Hood.Admin/
COPY projects/Hood.UI.Core/Hood.UI.Core.csproj             projects/Hood.UI.Core/
COPY projects/Hood.UI.Admin/Hood.UI.Admin.csproj           projects/Hood.UI.Admin/
COPY projects/Hood.UI.Bootstrap3/Hood.UI.Bootstrap3.csproj projects/Hood.UI.Bootstrap3/
COPY projects/Hood.UI.Bootstrap4/Hood.UI.Bootstrap4.csproj projects/Hood.UI.Bootstrap4/
COPY projects/Hood.Development/Hood.Development.csproj      projects/Hood.Development/

RUN dotnet restore Hood.sln

# ---------------------------------------------------------------------------
# Stage 2: publish the web host
# ---------------------------------------------------------------------------
COPY . .
RUN dotnet publish projects/Hood.Development/Hood.Development.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Stage 3: runtime image
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "Hood.Development.dll"]
