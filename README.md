# Simple SWGOH Arena Tracker

A long-running worker that polls Star Wars: Galaxy of Heroes (SWGOH) arena rankings for a configured list of players and posts Discord notifications when their ranks change.

This repository builds a Docker image that wraps the upstream [`iprobedroid/swgoh-arena-tracker`](https://github.com/iprobedroid/swgoh-arena-tracker) image (pinned at `beta-24`). There is no application source in this repo — it is purely a thin Docker packaging layer.

## Build

```bash
docker build -t ccIPD-arena-tracker .
```

The `Dockerfile` is a single-line `FROM iprobedroid/swgoh-arena-tracker:beta-24`, so no source compilation happens at build time.

## Run

```bash
docker run -d --name swgoh-arena-tracker \
  -e DISCORD_WEB_HOOK="https://discord.com/api/webhooks/..." \
  -e ALLY_CODES="123456789,123456788" \
  -e ARENA_TYPE="SQUAD" \
  --restart unless-stopped \
  ccIPD-arena-tracker
```

### Notes

- Run a single container. The tracker keeps "previous rank" state in process memory, so running more than one instance will cause duplicate/conflicting Discord messages.
- Restarting the container wipes the rank baselines - the next poll tick will post a status message for every player instead of a diff. This is expected.

## Configuration

All configuration is via environment variables.

| Variable | Required | Description |
|---|---|---|
| `DISCORD_WEB_HOOK` | yes | Full Discord webhook URL. |
| `ALLY_CODES_URL` | one of two | HTTPS URL that returns a JSON list of players (the "gist" workflow). |
| `ALLY_CODES` | one of two | Inline comma-separated ally codes (the simple workflow). Ignored when `ALLY_CODES_URL` is set. |
| `ARENA_TYPE` | optional | `SQUAD` (default) or `FLEET` - selects which arena rank column to track. |
| `CUSTOM_MESSAGE_STATUS` | optional | Override the status (no-change) message template. |
| `CUSTOM_MESSAGE_CLIMB` | optional | Override the climb message template. |
| `CUSTOM_MESSAGE_DROP` | optional | Override the drop message template. |
| `TAG_ON_CLIMB_RANK_LIMIT` | optional | Numeric rank threshold - tag the player on Discord only if rank climbed past it. |
| `TAG_ON_DROP_RANK_LIMIT` | optional | Numeric rank threshold - tag the player on Discord only if rank dropped past it. |
| `TAG_ON_DROP_PO_LIMIT` | optional | Time-to-payout threshold (minutes) for drop-side tagging. |
| `DISABLE_STATUS_MESSAGE` | optional | Set `TRUE` to suppress periodic status messages when nothing has changed. |
| `DISCORD_TAGS` | optional | Discord role IDs / user IDs to mention on tag-worthy events. |
| `DISABLE_ANALYTICS` | optional | Set `TRUE` to opt out of analytics beacons. |
| `LOGGER_TYPE` | optional | `CONSOLE` (default) or `DISCORD` (mirror logs to a Discord channel). |
| `LOGGER_HOOK` | conditional | Discord webhook for the logger when `LOGGER_TYPE=DISCORD`. |
| `GAME_CLIENT_VERSION` | optional | Override the spoofed SWGOH client version (default `99.99.99`). |

### Example `ALLY_CODES` workflow

```bash
docker run -d --name swgoh-arena-tracker \
  -e DISCORD_WEB_HOOK="https://discord.com/api/webhooks/..." \
  -e ALLY_CODES="123456789,123456788,123456999" \
  --restart unless-stopped \
  ccIPD-arena-tracker
```

### Example `ALLY_CODES_URL` workflow

`https://gist.github.com/<user_name>/<gist_id>/raw` returning a JSON list of players. `name` and `discordId` are optional (set to `""` if not used). `userIcon` is a Discord emoji id in the form `:emoji_code:`.

```bash
docker run -d --name swgoh-arena-tracker \
  -e DISCORD_WEB_HOOK="https://discord.com/api/webhooks/..." \
  -e ALLY_CODES_URL="https://gist.github.com/<user>/<id>/raw" \
  --restart unless-stopped \
  ccIPD-arena-tracker
```

## Discord

Join the upstream Discord channel for support: https://discord.gg/xcjvKPM

## License & credits

Upstream project: [iprobedroid/swgoh-arena-tracker](https://github.com/iprobedroid/swgoh-arena-tracker).
