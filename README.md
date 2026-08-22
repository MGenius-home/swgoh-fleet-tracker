# SWGOH Fleet Tracker

A long-running worker that polls Star Wars: Galaxy of Heroes (SWGOH) **fleet arena** rankings for a configured list of players and posts Discord notifications when their ranks change.

> Squad arena will be removed from the game shortly, so this tracker is fleet-only. The old `ARENA_TYPE` variable is no longer needed and is ignored.

Built from the .NET 8 source in `src/` (a fork of the upstream [`iprobedroid/swgoh-arena-tracker`](https://github.com/iprobedroid/swgoh-arena-tracker)) with payout-shift tracking, scheduled roster posts, weekly attack metrics, file-based state persistence, and multi-arch images for x86 and Raspberry Pi.

## Credits

This project stands on the shoulders of:

- [iprobedroid/swgoh-arena-tracker](https://github.com/iprobedroid/swgoh-arena-tracker) - the upstream project; the .NET tracker source in `src/` and the original Docker image are based on its work.
- [DV1231/ccIPD-Arena-Tracker](https://github.com/DV1231/ccIPD-Arena-Tracker) - the original ccIPD Arena Tracker (GPL-3.0) that this project descends from.
- [iprobedroid's Discord channel](https://discord.gg/xcjvKPM) - community support channel for the tracker family.

Other Docker distributions of the same tracker family (not used by this project, which compiles its own source): [robpurcell/swgoh-fleet-tracker](https://hub.docker.com/r/robpurcell/swgoh-fleet-tracker) and [monzal/swgoh-tracker](https://hub.docker.com/r/monzal/swgoh-tracker) - an ARM64/Alpine repackaging.

## Build

```bash
docker build -t swgoh-fleet-tracker .
```

The image is compiled from this repo's source using the official .NET 8 SDK (multi-stage build) on a slim Ubuntu "chiseled" base, running as a non-root user (~120MB).

## Run (Docker Compose)

A sample [`docker-compose.yml`](docker-compose.yml) is included. Copy it, fill in your webhook URLs and ally codes, then:

```bash
docker compose up -d --build
```

## Run

```bash
docker run -d --name swgoh-fleet-tracker \
  -e DISCORD_WEB_HOOK="https://discord.com/api/webhooks/..." \
  -e ALLY_CODES="123456789,123456788" \
  -v swgoh-fleet-tracker-data:/app/data \
  --restart unless-stopped \
  swgoh-fleet-tracker
```

### Notes

- Run a single container. Running more than one instance against the same tracked players will cause duplicate/conflicting Discord messages.
- Mount `/app/data` (or set `STORAGE_FILE_PATH` to a mounted path) so rank baselines, payout baselines, and attack counters survive container restarts.

## Raspberry Pi / ARM64

Published images are multi-arch (`linux/amd64` + `linux/arm64`), so they run natively on Raspberry Pi 4/5, Apple Silicon, and ARM64 cloud instances. On a Pi with a 64-bit OS, just pull and run:

```bash
docker run -d --name swgoh-fleet-tracker \
  -e DISCORD_WEB_HOOK="https://discord.com/api/webhooks/..." \
  -e ALLY_CODES_URL="https://gist.github.com/<user>/<id>/raw" \
  -v swgoh-fleet-tracker-data:/app/data \
  --restart unless-stopped \
  ghcr.io/mgenius-home/swgoh-fleet-tracker:latest
```

Multi-arch images are published automatically by GitHub Actions on every push to `master`. To build for the Pi yourself instead:

```bash
docker buildx build --platform linux/arm64 -t swgoh-fleet-tracker:pi --load .
```

(32-bit Raspberry Pi OS is not supported - use the 64-bit image.)

## Discord webhooks

Everything the tracker posts goes to one of two webhooks. `PAYOUT_WEBHOOK_URL` falls back to `DISCORD_WEB_HOOK` when unset, so a single-webhook setup works fine.

| Message | Webhook | Controlled by |
|---|---|---|
| Rank climb alerts | `DISCORD_WEB_HOOK` | always on when a rank improves |
| Rank drop alerts | `DISCORD_WEB_HOOK` | always on when a rank worsens |
| Payout shift notifications | `PAYOUT_WEBHOOK_URL` | off unless `ENABLE_PAYOUT_TRACKING=TRUE` |
| Scheduled roster post (rank + time-to-payout for everyone) | `PAYOUT_WEBHOOK_URL` | off unless `STATUS_MESSAGE_CRON` is set |
| Weekly attack summary + counter reset | `PAYOUT_WEBHOOK_URL` | off unless `ENABLE_WEEKLY_ATTACK_SUMMARY=TRUE`; schedule via `WEEKLY_ATTACK_SUMMARY_CRON` |

## Configuration

All configuration is via environment variables.

| Variable | Required | Description |
|---|---|---|
| `DISCORD_WEB_HOOK` | yes | Main Discord webhook URL (rank climbs/drops). |
| `PAYOUT_WEBHOOK_URL` | optional | Dedicated webhook for payout shifts, roster posts, and the weekly attack summary. Falls back to `DISCORD_WEB_HOOK`. |
| `ALLY_CODES_URL` | one of two | HTTPS URL that returns a JSON list of players (the "gist" workflow). |
| `ALLY_CODES` | one of two | Inline comma-separated ally codes (the simple workflow). Ignored when `ALLY_CODES_URL` is set. |
| `POLL_INTERVAL_SECONDS` | optional | Seconds between polls of the game server. Default `15`, minimum `2`, maximum `3600`. |
| `SCHEDULE_TIMEZONE` | optional | IANA time zone used to evaluate `STATUS_MESSAGE_CRON` and `WEEKLY_ATTACK_SUMMARY_CRON` (e.g. `America/New_York`). Default `UTC`. DST changes are handled automatically. |
| `STORAGE_FILE_PATH` | optional | Path for persistent state storage across container updates. Default `/app/data/state.json` inside the container - no configuration needed for a basic setup. |
| `ENABLE_ANALYTICS` | optional | Set `TRUE` to enable the startup analytics beacon sent to the upstream stats service. Default `FALSE` (off). |
| `LOGGER_TYPE` | optional | `CONSOLE` (default) or `DISCORD` (mirror logs to a Discord channel). |
| `LOGGER_HOOK` | conditional | Discord webhook for the logger when `LOGGER_TYPE=DISCORD`. |
| `GAME_CLIENT_VERSION` | optional | Override the spoofed SWGOH client version (default `99.99.99`). |

`ARENA_TYPE` from older versions is ignored (fleet is tracked unconditionally).

Feature-specific variables are documented in their sections below: [rank alerts](#discord-web-hook-rank-alerts), [payout tracking](#payout-webhook-features), [scheduled roster](#scheduled-roster-post), [weekly summary](#weekly-attack-summary).

<a id="discord-web-hook-rank-alerts"></a>
## `DISCORD_WEB_HOOK` - rank alerts

Climb and drop alerts are posted here whenever a tracked player's fleet arena rank changes between polls. Templates are customizable via `CUSTOM_MESSAGE_CLIMB` / `CUSTOM_MESSAGE_DROP`; tagging behavior via the `TAG_ON_*` limits. See [Custom message templates](#custom-message-templates).

| Variable | Default | Description |
|---|---|---|
| `CUSTOM_MESSAGE_CLIMB` | see templates | Override the climb message template. |
| `CUSTOM_MESSAGE_DROP` | see templates | Override the drop message template. |
| `TAG_ON_CLIMB_RANK_LIMIT` | `1000` | `%TAG_ON_CLIMB%` mentions render only while the player's rank number is at or below this value (inside the top N). |
| `TAG_ON_DROP_RANK_LIMIT` | `0` | `%TAG_ON_DROP%` mentions render only when the player's rank number is at or above this value (fell to position N or worse). |
| `TAG_ON_DROP_PO_LIMIT` | `1440` | Time-to-payout threshold (minutes) for drop-side tagging. |
| `DISCORD_TAGS` | - | Discord role IDs / user IDs to mention on tag-worthy events. |

## `PAYOUT_WEBHOOK_URL` features

### Payout tracking

Fleet arena pays out at 19:00 in **each player's own local timezone**, so every tracked player has a different payout moment in UTC. Each poll reads the player's local timezone offset from the game API and derives that player's UTC payout slot as `(19:00 local - timezone offset) mod 24h`. When a player's derived UTC payout time changes (typically because they changed their game/device timezone), an embed notification is sent containing:

- The player name / ally code.
- The shift delta (+/-X hours).
- The new UTC payout time (`HH:mm UTC`).
- The shared payout group: all other tracked players at the same payout slot.
- Optionally (when `POST_FULL_PAYOUT_LIST_ON_CHANGE=TRUE`) the full payout order for all tracked ally codes.

| Variable | Default | Description |
|---|---|---|
| `POST_FULL_PAYOUT_LIST_ON_CHANGE` | `FALSE` | Append the full ordered payout list to every payout-shift embed. |
| `ENABLE_PAYOUT_TRACKING` | `FALSE` | Set `TRUE` to enable payout shift detection and notifications (attack tracking unaffected). |

### Scheduled roster post

Off by default. Set `STATUS_MESSAGE_CRON` to periodically post the full roster - one line per player (name, current rank, time-to-payout) using the `CUSTOM_MESSAGE_STATUS` template - as a single batched message. This replaces the old beta-24 behavior of posting the roster on every container start, but on *your* schedule instead of every restart.

```yaml
STATUS_MESSAGE_CRON: "DAILY 04:00"   # friendly schedule or cron - see below
CUSTOM_MESSAGE_STATUS: "Fleet-[%PLAYER_NAME%](<https://swgoh.gg/p/%ALLY_CODE%>) is at %CURRENT_RANK% <:crystals:825970086401277983> %TIME_TO_PO%"
```

### Weekly attack summary

Off by default. Rank climbs observed during polls increment a per-player weekly attack counter (climbs that coincide with the player's own daily payout-reset window are ignored, since shard reshuffles there are not attacks). When enabled, a ranked leaderboard of all tracked ally codes ordered by attacks performed is posted at the configured schedule, then all counters are reset.

| Variable | Default | Description |
|---|---|---|
| `WEEKLY_ATTACK_SUMMARY_CRON` | `0 0 * * 0` | Schedule for the summary + reset. Only used when `ENABLE_WEEKLY_ATTACK_SUMMARY=TRUE`. Accepts a friendly schedule or cron - see [Schedules](#schedules). |
| `ENABLE_WEEKLY_ATTACK_SUMMARY` | `FALSE` | Set `TRUE` to enable the weekly attack summary and counter reset. |

### Schedules

Both `STATUS_MESSAGE_CRON` and `WEEKLY_ATTACK_SUMMARY_CRON` accept either a friendly schedule or a standard 5-field cron expression.

By default schedules are evaluated in **UTC**. Set `SCHEDULE_TIMEZONE` (IANA name) to evaluate them in your local wall-clock time instead - for example, with `SCHEDULE_TIMEZONE=America/New_York`, `"SUNDAY 16:00"` fires every Sunday at 4:00 PM Eastern, correctly across daylight-saving changes.

| Format | Example | Meaning |
|---|---|---|
| `<day> <HH:mm>` | `SUNDAY 18:00`, `SUN 20:30` | Every week on that day at that time |
| `DAILY <HH:mm>` | `DAILY 12:00` | Every day at that time |
| `DAILY` | `DAILY` | Every day at midnight |
| `HOURLY` | `HOURLY` | On the hour, every hour (handy for testing) |
| `WEEKLY` | `WEEKLY` | Sunday at midnight |
| cron expression | `0 0 * * 0`, `30 9 * * 1-5` | Full cron syntax (`minute hour day-of-month month day-of-week`) |

Day names can be written in full (`SUNDAY`) or abbreviated (`SUN`). For example, to post the attack summary every Friday evening in Eastern time:

```yaml
SCHEDULE_TIMEZONE: "America/New_York"
WEEKLY_ATTACK_SUMMARY_CRON: "FRIDAY 19:00"
```

## Custom message templates

Status (roster), climb, and drop messages can be restyled with `CUSTOM_MESSAGE_STATUS`, `CUSTOM_MESSAGE_CLIMB`, and `CUSTOM_MESSAGE_DROP`. Messages are posted as plain Discord content, so any Discord markdown works: links, bold, custom emojis (`<:name:id>`), etc.

Placeholders available (substituted before sending):

| Placeholder | Value |
|---|---|
| `%PLAYER_NAME%` | In-game name reported by the game API |
| `%NAME%` | Custom display name from player settings (`ALLY_CODES_URL` metadata), if set |
| `%USER_ICON%` | Custom emoji from player settings (`userIcon`), if set |
| `%ALLY_CODE%` | The player's 9-digit ally code |
| `%CURRENT_RANK%` | Current fleet arena rank |
| `%PREVIOUS_RANK%` | Rank before this change (empty context on roster posts) |
| `%TIME_TO_PO%` | Time until this player's next payout, as `HH:mm` |
| `%TAG_ON_CLIMB%` / `%TAG_ON_DROP%` | A Discord mention of the player - rendered **only** if the corresponding `TAG_ON_*` limit condition is met |

Example setup with swgoh.gg links and custom emojis:

```yaml
CUSTOM_MESSAGE_STATUS: "Fleet-[%PLAYER_NAME%](<https://swgoh.gg/p/%ALLY_CODE%>) is at %CURRENT_RANK% <:crystals:825970086401277983> %TIME_TO_PO%"
CUSTOM_MESSAGE_CLIMB: "<:b_up:806635916352946176> Fleet-[%PLAYER_NAME%](<https://swgoh.gg/p/%ALLY_CODE%>) %PREVIOUS_RANK%  <:r_up:817234194010341407> %CURRENT_RANK% <:crystals:825970086401277983> %TIME_TO_PO%"
CUSTOM_MESSAGE_DROP: "<:b_down:806635945469280276> Fleet-[%PLAYER_NAME%](<https://swgoh.gg/p/%ALLY_CODE%>) %CURRENT_RANK% <:l_down:817234324201275402> %PREVIOUS_RANK% <:crystals:825970086401277983> %TIME_TO_PO%"
```

Notes:

- Mentions are driven by the placeholders. If your template does not contain `%TAG_ON_CLIMB%` / `%TAG_ON_DROP%`, players are never pinged regardless of the `TAG_ON_*_RANK_LIMIT` values. Add the placeholder to enable pings; the limits then decide *when* they fire.
- `%PLAYER_NAME%` comes straight from the game API. For stable names/links across name changes, per-player settings from an `ALLY_CODES_URL` gist (`name`) are a better fit.

### Migrating from `iprobedroid/swgoh-arena-tracker:beta-24`

- `ARENA_TYPE` is gone: squad arena no longer exists in the game, so this fork tracks fleet only. Remove the variable from your compose.
- `CUSTOM_MESSAGE_STATUS` still works, but it now drives the opt-in **scheduled roster post** (`STATUS_MESSAGE_CRON`) instead of firing automatically on every container start.
- `PUID` / `PGID` / `TZ` are not needed by this image and are ignored. All payout math is done in UTC internally; use `SCHEDULE_TIMEZONE` to control the schedule clock.
- This image runs as a non-root user (uid `1654`). If you bind-mount a host directory over `/app/data`, make it writable by that uid (named volumes handle ownership automatically).

## State persistence

Player ranks, payout slots, attack counters, and schedule bookkeeping are stored in a single JSON file, defaulting to `/app/data/state.json` inside the container. You do not have to set anything for this to work - out of the box the tracker uses its internal container storage and survives container restarts.

Startup is silent: players are re-baselined from stored state without posting anything, so unlike the upstream image the tracker does not flood Discord with a full ally-code/payout list on every start. New players added to `ALLY_CODES` are also recorded silently; only climbs, drops, payout shifts, scheduled roster posts, and weekly summaries generate messages.

Be aware: if you remove the container (`docker rm` / `docker compose down --remove-volumes`), Docker discards that anonymous internal volume and state is lost. To keep history across full rebuilds/recreations, mount a named volume or host directory over `/app/data` (or point `STORAGE_FILE_PATH` at a mounted path):

```bash
-v swgoh-fleet-tracker-data:/app/data
```

## Example `ALLY_CODES` workflow

```bash
docker run -d --name swgoh-fleet-tracker \
  -e DISCORD_WEB_HOOK="https://discord.com/api/webhooks/..." \
  -e ALLY_CODES="123456789,123456788,123456999" \
  --restart unless-stopped \
  swgoh-fleet-tracker
```

## Example `ALLY_CODES_URL` workflow

`https://gist.github.com/<user_name>/<gist_id>/raw` returning a JSON list of players. `name` and `discordId` are optional (set to `""` if not used). `userIcon` is a Discord emoji id in the form `:emoji_code:`.

```bash
docker run -d --name swgoh-fleet-tracker \
  -e DISCORD_WEB_HOOK="https://discord.com/api/webhooks/..." \
  -e ALLY_CODES_URL="https://gist.github.com/<user>/<id>/raw" \
  --restart unless-stopped \
  swgoh-fleet-tracker
```

## Discord

Join the upstream Discord channel for support: https://discord.gg/xcjvKPM

## License & credits

- Upstream project: [iprobedroid/swgoh-arena-tracker](https://github.com/iprobedroid/swgoh-arena-tracker).
- Original project: [DV1231/ccIPD-Arena-Tracker](https://github.com/DV1231/ccIPD-Arena-Tracker) (GPL-3.0).

This repository is a derivative of GPL-3.0 code and is distributed under the GNU General Public License v3.0 - see [LICENSE](LICENSE).
