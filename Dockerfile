# =============================================================================
# Hood CMS — Docker build (local dev / containerised run)
# Runs the Hood.Development web host (the runnable app; the rest are libraries).
# Usage: docker compose up --build
# Target framework: net10.0 (HOOD-57).
# =============================================================================
#
# Frontend assets: Hood's core CSS/JS ship as the external `hoodcms` npm package
# (normally served from jsDelivr, keyed by the backend assembly version). With the
# backend now on 7.0.0-rc and `hoodcms` still published at 6.1.x, that CDN path
# 404s — and the dev appsettings runs with "BypassCDN": true, which expects the
# assets locally under wwwroot. So we pull the stable `hoodcms` package at build
# time (frontend stage below) and bundle its src/ + dist/ into wwwroot, keeping
# the dev rig fully self-contained. Bump HOODCMS_VERSION as the frontend releases.
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
COPY projects/Hood.Tests/Hood.Tests.csproj                 projects/Hood.Tests/
COPY projects/Hood.SchemaTool/Hood.SchemaTool.csproj       projects/Hood.SchemaTool/

RUN dotnet restore Hood.sln

# ---------------------------------------------------------------------------
# Stage 2: publish the web host
# ---------------------------------------------------------------------------
COPY . .
RUN dotnet publish projects/Hood.Development/Hood.Development.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Stage 3: frontend assets (the `hoodcms` npm package — Hood's core CSS/JS)
# ---------------------------------------------------------------------------
FROM node:20-alpine AS frontend
ARG HOODCMS_VERSION=6.1.8
WORKDIR /fe
# `npm pack` downloads the published tarball without installing anything; extracting
# it yields ./package/{src,dist,images,...} — the same paths the Razor views request.
RUN npm pack hoodcms@${HOODCMS_VERSION} && tar -xzf hoodcms-*.tgz

# ---------------------------------------------------------------------------
# Stage 4: runtime image
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

# Bundle the core frontend so the static-file middleware serves /src/** and /dist/**
# locally (BypassCDN=true). The dev host keeps its own wwwroot/images, so only the
# package's src/ and dist/ are layered in.
COPY --from=frontend /fe/package/src  ./wwwroot/src
COPY --from=frontend /fe/package/dist ./wwwroot/dist

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "Hood.Development.dll"]
