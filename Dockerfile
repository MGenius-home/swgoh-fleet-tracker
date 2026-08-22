# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution and restore dependencies
COPY src/ ./
RUN dotnet restore IpdArenaTracker.sln

# Build release artifacts
RUN dotnet publish SimpleTracker/SimpleTracker.csproj -c Release -o /app/out

# Stage 2: Runtime Execution
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build-env /app/out .

RUN mkdir -p /app/data
VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "SimpleTracker.dll"]
