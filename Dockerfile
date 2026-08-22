# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution and restore dependencies
COPY src/ ./
RUN dotnet restore IpdArenaTracker.sln

# Build release artifacts and prepare the state directory for the runtime's 'app' user (uid 1654)
RUN dotnet publish SimpleTracker/SimpleTracker.csproj -c Release -o /app/out && mkdir -p /app/data

# Stage 2: Runtime Execution (Ubuntu chiseled: small footprint, shell-less, non-root 'app' user)
FROM mcr.microsoft.com/dotnet/runtime:8.0-noble-chiseled
WORKDIR /app
COPY --from=build-env /app/out .
COPY --from=build-env --chown=1654:1654 /app/data /app/data
VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "SimpleTracker.dll"]
