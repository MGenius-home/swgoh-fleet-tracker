# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution and restore dependencies
COPY src/ ./
RUN dotnet restore IpdArenaTracker.sln

# Build release artifacts, prepare state dir, and grab a static busybox
# (the chiseled runtime is shell-less; busybox provides the entrypoint's
#  chown + privilege-drop so bind-mounted data dirs need no manual chown)
RUN dotnet publish SimpleTracker/SimpleTracker.csproj -c Release -o /app/out \
    && mkdir -p /app/data \
    && apt-get update \
    && apt-get install -y --no-install-recommends busybox-static gosu \
    && cp /bin/busybox /busybox \
    && cp /usr/sbin/gosu /gosu

# Stage 2: Runtime Execution (Ubuntu chiseled: small footprint, shell-less)
FROM mcr.microsoft.com/dotnet/runtime:8.0-noble-chiseled
WORKDIR /app
COPY --from=build-env /app/out .
COPY --from=build-env --chown=1654:1654 /app/data /app/data
COPY --from=build-env /busybox /bin/busybox
COPY --from=build-env /gosu /usr/bin/gosu
COPY --from=build-env /app/SimpleTracker/entrypoint.sh /entrypoint.sh

# Timezone database so SCHEDULE_TIMEZONE works (chiseled base ships without one)
COPY --from=build-env /usr/share/zoneinfo /usr/share/zoneinfo

# Entrypoint starts as root only to fix data-dir ownership, then drops
# to PUID:PGID (default 1654:1654) before launching the app.
USER root

VOLUME ["/app/data"]

ENTRYPOINT ["/bin/busybox", "sh", "/entrypoint.sh"]
