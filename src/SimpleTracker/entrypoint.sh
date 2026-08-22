PUID=${PUID:-1654}
PGID=${PGID:-1654}

chown -R "$PUID:$PGID" /app/data 2>/dev/null

exec /usr/bin/gosu "$PUID:$PGID" dotnet SimpleTracker.dll
