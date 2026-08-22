# swgoh-arena-tracker — Design Document

A design document for the fork [`MGenius-home/swgoh-arena-tracker`](https://github.com/MGenius-home/swgoh-arena-tracker) (formerly `MGenius-home/ccIPD-arena-tracker`), which wraps [`iprobedroid/swgoh-arena-tracker`](https://github.com/iprobedroid/swgoh-arena-tracker) (upstream source) as a Docker image built from the pinned upstream image `iprobedroid/swgoh-arena-tracker:beta-24`.

All factual claims below are grounded in either:
- this fork's own files (`Dockerfile`, `README.md`), or
- strings and symbols extracted from the upstream `beta-24` image filesystem (paths prefixed `/app/`).

---

## 1. Overview

A long-running worker that polls Star Wars: Galaxy of Heroes (SWGOH) arena rankings for a configured list of players and posts Discord notifications when their ranks change. The container is a plain single-process worker — there is no web traffic and no inbound HTTP.

The fork itself contains **no application source**. All behavior comes from the pinned upstream image. The fork is purely a thin Docker packaging layer that lets the upstream image be built and run under any Docker host.

---

## 2. Repository contents

| Path | Purpose |
|---|---|
| `Dockerfile` | One line: `FROM iprobedroid/swgoh-arena-tracker:beta-24`. No build steps. |
| `README.md` | Build/run instructions and the configuration variable table. |
| `src/` | A copy of the upstream .NET 5 source tree, included for reference. Not compiled by the `Dockerfile` (the Dockerfile uses the upstream published image, not this source). |
| `DESIGN.md` | This document. |
| `.gitignore` | Standard .NET / IDE / OS excludes. |

There is no CI configuration, no `docker-compose.yml`, and no platform-specific deploy manifest in the fork.

---

## 3. Runtime topology

```
┌──────────────────────────────────────────┐
│  Docker host (any: local, VM, cloud)     │
│  ┌────────────────────────────────────┐  │
│  │ iprobedroid/swgoh-arena-tracker    │  │
│  │        :beta-24                    │  │
│  │  (pulled by the fork's Dockerfile) │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
        │ outbound HTTPS only
        ▼
   swprod.capitalgames.com:443  (game RPC)
   swgoh-tracker-stats.herokuapp.com:443 (player metadata; upstream service)
   discord.com:443              (webhook delivery)
```

The container's entrypoint is the .NET 5 binary `SimpleTracker` (`/app/SimpleTracker`, target framework `net5.0`, framework `Microsoft.NETCore.App 5.0.0`, from `/app/SimpleTracker.runtimeconfig.json`). It loads its configuration from environment variables on startup (`SimpleTracker.dll` references `GetEnvironmentVariable`, `GetEnvironmentVariables`).

No third-party scheduler library is bundled (no `Quartz`, `Hangfire`, or `cron` strings in the binaries). The poll loop is driven by `Ipd.Core.Jobs.DiscordMessengerJob.ExecuteAsync` (`Ipd.Core.dll`) — an async loop that waits between iterations.

---

## 4. Configuration (environment variables)

Read directly from the process environment via `Microsoft.Extensions.Configuration.EnvironmentVariables` (referenced by `SimpleTracker.dll` and `Ipd.Core.dll`).

| Variable | Required | Purpose |
|---|---|---|
| `DISCORD_WEB_HOOK` | yes | Full Discord webhook URL. Posted to on every message. |
| `ALLY_CODES_URL` | one of two | HTTPS URL that returns a JSON list of players (the "gist" workflow). |
| `ALLY_CODES` | one of two | Inline comma-separated ally codes (the simple workflow). Ignored when `ALLY_CODES_URL` is set. |
| `ARENA_TYPE` | optional | `SQUAD` (default) or `FLEET` — selects which arena rank column to track. |
| `CUSTOM_MESSAGE_STATUS` | optional | Override the status (no-change) message template. |
| `CUSTOM_MESSAGE_CLIMB` | optional | Override the climb message template. |
| `CUSTOM_MESSAGE_DROP` | optional | Override the drop message template. |
| `TAG_ON_CLIMB_RANK_LIMIT` | optional | Numeric rank threshold — tag the player on Discord only if rank climbed past it. |
| `TAG_ON_DROP_RANK_LIMIT` | optional | Numeric rank threshold — tag the player on Discord only if rank dropped past it. |
| `TAG_ON_DROP_PO_LIMIT` | optional | Time-to-payout threshold (minutes) for drop-side tagging. |
| `DISABLE_STATUS_MESSAGE` | optional | Set `TRUE` to suppress periodic status messages when nothing has changed. |
| `DISCORD_TAGS` | optional | Discord role IDs / user IDs to mention on tag-worthy events. |
| `DISABLE_ANALYTICS` | optional | Set `TRUE` to opt out of analytics beacons. |
| `LOGGER_TYPE` | optional | `CONSOLE` (default) or `DISCORD` (mirror logs to a Discord channel). |
| `LOGGER_HOOK` | conditional | Discord webhook for the logger when `LOGGER_TYPE=DISCORD`. |
| `GAME_CLIENT_VERSION` | optional | Override the spoofed SWGOH client version (default `99.99.99`, per `SimpleTracker.dll`). |

Player settings (custom name, Discord ID, custom emoji) can be supplied either inline in the `ALLY_CODES` value (`{0}:{1}:{2}` per `SimpleTracker.dll`) or fetched from a remote URL by `PlayerSettingsUrlProvider` (`Ipd.Core.dll`).

---

## 5. Data source

Two outbound services are called. Evidence is from string constants in `/app/Ipd.GameClient.dll` and `/app/Ipd.Core.dll`.

### 5.1 Game API — `swprod.capitalgames.com`

- **Endpoint:** `POST https://swprod.capitalgames.com/rpc` (`Ipd.GameClient.dll`)
- **Request body:** protobuf-encoded `RequestEnvelope` (`ipd.game.protocol` namespace) carrying `GetPlayerArenaProfile` for each ally code.
- **Encoding:** `Content-Type: application/x-protobuf`; response is gzip-encoded protobuf (`Accept-Encoding: gzip`, `ContentEncoding: GZIPACCEPTENCODING` / `GZIPCONTENTENCODING`).
- **Auth:** the envelope's `AuthId` and `AuthToken` fields are populated from constants in the binary. This is the same wire protocol the SWGOH mobile client uses.
- **Platform spoof:** the envelope's `Platform` field is set to `Android`, and `ClientVersion` defaults to `99.99.99` unless overridden.
- **Response shape:** `PlayerArenaProfile` (full) or `SlimPlayerArenaProfile` (name + level + allyCode + playerId + pvpProfile + local time-zone offset). The slim profile is what `GetSlimPlayerArenaRanks` consumes. Each `PlayerArenaStatus` carries `arena_type` (`SquadArena` | `FleetArena`) and `place` (the rank).
- **Method name:** `GetPlayerArenaProfile` (`Ipd.GameClient.dll`).

This is the source of truth for ranks — the app talks directly to Capital Games' SWGOH servers, not to swgoh.gg.

### 5.2 Player metadata — `swgoh-tracker-stats.herokuapp.com`

This is an **upstream-provided service** (the `host` part of the hostname is incidental to the service's identity). The app calls it as a read-only metadata source.

- **Endpoint:** `https://swgoh-tracker-stats.herokuapp.com` + `/stats` (the literal string `stats` appears alongside the base URL in `Ipd.Core.dll`).
- **Request body:** `application/json`.
- **Used by:** `PlayerSettingsUrlProvider.GetPlayerSettingAsync` (`Ipd.Core.dll`).
- **Returns:** per-ally-code player settings — display name, Discord user ID, and custom emoji (`userIcon`). These are used to fill in the `%PLAYER_NAME%`, `%USER_ICON%`, and Discord `@mention` portions of the message templates.
- **Failure handling:** if the stats URL returns a non-2xx (`[PlayerSettingsProvider]:Failed to load player settings. Status code ({0}).`) or deserialization fails (`[PlayerSettingsProvider]:Failed to deserialize player settings: `), the app falls back to whatever inline metadata was provided in the `ALLY_CODES` value.

---

## 6. Poll loop

The work class is `Ipd.Core.Jobs.DiscordMessengerJob` with a state-machine method `ExecuteAsync` (`Ipd.Core.dll`). Per iteration the loop:

1. Resolves the current set of ally codes (via `EnvAllyCodesProvider` / `EnvCsvAllyCodesProvider` if `ALLY_CODES`, or via the configured `ALLY_CODES_URL` gist).
2. Fetches player settings (name, Discord ID, emoji) for every ally code via `PlayerSettingsUrlProvider`.
3. Builds a `PlayerArenaProfileRequest` per ally code and calls `GameClient.GetPlayerArenaProfile` (backed by `GetSlimPlayerArenaRanks`). Errors are caught per ally code and logged (`Error processing allyCode:[...]`); the loop does not abort on a single failure.
4. For each player, computes the rank delta against the previous tick (`PlayerArenaRank.previousRank` vs `currentRank`) via `IPlayerRankService` and `IArenaRankStorage`.
5. Calls `IArenaRankStorage.SaveRank` to remember the new rank. The default implementation `StaticArenaRankStorage` keeps state in process memory; the app has no external database. (Restarting the container resets all "previous rank" baselines and triggers a status post on the first post-restart tick.)
6. Enqueues one Discord message per relevant state change into `IDiscordMessenger`/`INewDiscordMessenger`. If enqueue fails (`Error: failed to enqueue discord message`), it logs and continues.
7. Sleeps before the next iteration. The exact interval is not embedded as a string in the binaries (no `setInterval`/cron artifacts), but the cadence that has been observed by users of the upstream is roughly **one minute** — consistent with the lack of any external scheduler and the presence of `DiscordMessageBatchSize` throttling inside `Ipd.Core.dll`.

The loop also includes an `ExecutionThrottle` (`Ipd.Core.dll`) used by the messenger to rate-limit outbound Discord traffic.

---

## 7. Discord webhook update

### 7.1 Transport

- **Method:** `POST` to whatever URL the user puts in `DISCORD_WEB_HOOK`. Discord webhook URLs are self-authenticating (the URL contains the bot id and token), so the app does not send an `Authorization` header.
- **Encoding:** JSON (`application/json`).
- **Payload shape:** simple `content` field — `{{ content = {0} }}` (`Ipd.Core.dll`) — i.e. plain text, **not** Discord embeds. Placeholders are substituted before the message is sent.

### 7.2 Message templates

Three templates, defined in `Ipd.Core.dll`:

| State | Default template |
|---|---|
| Status (no change) | `%USER_ICON%` `%PLAYER_NAME%` is at `%CURRENT_RANK%`. payout in `%TIME_TO_PO%` |
| Climb (rank improved) | `%TAG_ON_CLIMB%`%USER_ICON%`` `%PLAYER_NAME%` climbed from `%PREVIOUS_RANK%` to `%CURRENT_RANK%`. payout in `%TIME_TO_PO%` |
| Drop (rank worsened) | `%TAG_ON_DROP%`%USER_ICON%`` `%PLAYER_NAME%` dropped from `%PREVIOUS_RANK%` to `%CURRENT_RANK%`. payout in `%TIME_TO_PO%` |

Substituted placeholders:

| Placeholder | Source |
|---|---|
| `%USER_ICON%` | custom emoji from player settings (`userIcon`) |
| `%PLAYER_NAME%` | display name from player settings or `ALLY_CODES` inline value |
| `%CURRENT_RANK%` / `%PREVIOUS_RANK%` | from the rank-diff calculation |
| `%TIME_TO_PO%` | computed by `ToPayoutString` from the arena's next payout time |
| `%TAG_ON_CLIMB%` / `%TAG_ON_DROP%` | Discord `@mention`/role strings from `DISCORD_TAGS`, gated by the rank-limit and payout-limit env vars |
| `%NAME%`, `%ALLY_CODE%` | additional substitution targets (not in the default templates but available for custom overrides) |

### 7.3 When a message fires

- **Climb** → fires whenever `currentRank < previousRank` and the climb crosses `TAG_ON_CLIMB_RANK_LIMIT` (when that env var is set).
- **Drop** → fires whenever `currentRank > previousRank` and the drop crosses either `TAG_ON_DROP_RANK_LIMIT` or `TAG_ON_DROP_PO_LIMIT` (whichever is configured).
- **Status** → fires on every poll by default; suppressible via `DISABLE_STATUS_MESSAGE=TRUE`.

### 7.4 Retry / rate limiting

Discord's 429 response is handled explicitly. The messenger reads `RetryAfter` from the response (`get_RetryAfter`/`set_RetryAfter` in `Ipd.Core.dll`) and waits that long before the next attempt. Non-429 failures follow a Polly `AsyncRetryPolicy` (`Polly.Retry`, `Polly.dll 7.2.1`) with the standard `Waiting {1} before next retry. Retry attempt {2}` log line. A short-circuit case `2 seconds sleep to retry` is hard-coded for one specific error path.

Messages can be batched (`DiscordMessageBatchSize` in `Ipd.Core.dll`) so a single webhook POST can carry multiple status updates when many players change at once.

---

## 8. State and persistence

All state is in-process. `StaticArenaRankStorage` (`SimpleTracker.dll`) holds the most recent rank per ally code in memory. There is no external database, no file on disk, and no external addon required.

Consequences:
- Restarting the container (deploy, host reboot, crash) wipes the "previous rank" baselines. The next poll tick posts a status message for every player instead of a diff.
- Running more than one container at once is also a correctness bug — both instances would race on the in-memory store and post duplicate/conflicting messages.

---

## 9. Failure modes

| Failure | Observed behavior | Source |
|---|---|---|
| `DISCORD_WEB_HOOK` missing | Process logs `ENV variable DISCORD_WEB_HOOK not found` and exits. | `SimpleTracker.dll` |
| `ALLY_CODES` / `ALLY_CODES_URL` missing/empty | The list is empty; no players tracked. No message is posted. | — |
| Malformed ally code | `Error: ally code `…` should consist of 9 digits.`; that player is skipped. | `Ipd.Core.dll` |
| Game API timeout/5xx | Polly retry, then logged as `errorCode:{0}, allyCode:{1}, {2}`; loop continues with remaining players. | `Ipd.GameClient.dll` |
| Stats URL non-2xx | `[PlayerSettingsProvider]:Failed to load player settings. Status code ({0}).`; falls back to inline metadata. | `Ipd.Core.dll` |
| Stats URL bad JSON | `[PlayerSettingsProvider]:Failed to deserialize player settings: …`; falls back to inline metadata. | `Ipd.Core.dll` |
| Discord 429 | Sleep `RetryAfter` then retry. | `Ipd.Core.dll` |
| Discord 5xx / network error | Polly retry with exponential-style waits; logged as `Request failed with StatusCode({0}). Waiting {1} before next retry. Retry attempt {2}`. | `Ipd.Core.dll` |
| Discord webhook deleted/rotated | Repeated non-2xx; retries exhausted; subsequent messages dropped. | — |
| Docker host goes down | The process stops; no polls run; no messages sent until the host returns and the container restarts. | — |

---

## 10. Fork vs upstream — what this fork actually changes

- `Dockerfile`: pins `iprobedroid/swgoh-arena-tracker:beta-24` (upstream publishes this tag; if upstream rebases or deletes the tag, the fork stops building).
- `src/`: a copy of the upstream .NET 5 source tree, included for reference and so the project can be opened/edited in an IDE. The `Dockerfile` does not compile this — it pulls the published upstream image. To actually use a code change, the source must be built upstream and a new image tag cut, or the `Dockerfile` must be changed to build from this `src/` instead of `FROM` the upstream image.
- `README.md` / `DESIGN.md`: this fork's documentation. The upstream README is not the source of truth here.
- **Everything else is unchanged** — same image, same env contract, same message templates, same Discord payload.

There is no application source in the fork that is shipped to the running container; all runtime behavior would have to be changed upstream and then a new tag cut, or the `Dockerfile` would have to be changed to build from `src/` instead of `FROM` the upstream image.

---

## 11. Sequence diagram — one poll tick

```
         ┌────────────┐                                          ┌──────────────────────┐
         │ Tracker    │                                          │ Capital Games / SWGOH│
         │ (.NET 5)   │                                          │ swprod.capitalgames  │
         └─────┬──────┘                                          └──────────┬───────────┘
               │                                                           │
               │  1. Read ALLY_CODES / ALLY_CODES_URL from env             │
               │                                                           │
               │  2. GET https://swgoh-tracker-stats.herokuapp.com/stats   │
               │  ───────────────────────────────────────────────────────► │
               │  ◄────────────────────────────── JSON: names/IDs/emojis ─ │
               │                                                           │
               │  3. POST /rpc  (protobuf, gzip)                           │
               │     envelope(GetPlayerArenaProfile { allyCode })  ─────► │
               │                                          ──────►         │
               │  ◄────────────── gzip'd PlayerArenaProfile ────────────── │
               │                                                           │
               │  4. Compute rank vs IArenaRankStorage (in-memory)        │
               │  5. Pick template (status | climb | drop)                 │
               │  6. Substitute placeholders                              │
               │                                                           │
               ▼                                                           ▼
       ┌─────────────────────┐                                   ┌──────────────────────┐
       │ DiscordMessenger    │  POST $DISCORD_WEB_HOOK           │ Discord              │
       │                     │ ─────────────────────────────────►│ /api/webhooks/...    │
       │ 7. Retry on 429/5xx │ ◄──── 200 / 204 (ok)              │                      │
       └─────────────────────┘                                   └──────────────────────┘

               │  8. SaveRank(currentRank) → IArenaRankStorage
               │  9. Sleep until next tick
```

---

## 12. Open questions / caveats

- The exact poll cadence is **not encoded as a string** in the image. The behavior described above (≈1 minute, async loop in `DiscordMessengerJob.ExecuteAsync`) matches the upstream README and observed behavior but cannot be quoted from a `path:line`.
- `GAME_CLIENT_VERSION=99.99.99` is the default. Capital Games can reject requests from clients claiming that version; in practice the upstream image works as of the image's publish date (`beta-24` shipped in early 2021 per the embedded `Ipd.Core.dll` build timestamps) but may stop working if the SWGOH RPC protocol changes.
- The `swgoh-tracker-stats.herokuapp.com` metadata endpoint is hosted by the upstream project. If that service goes down, the container falls back to inline `ALLY_CODES` metadata (no per-player name/icon enrichment) but otherwise keeps tracking ranks.
- The fork's `src/` directory is committed but not compiled by the `Dockerfile`. If you want to build from source, replace the `Dockerfile` `FROM` line with a multi-stage build that compiles `src/IpdArenaTracker.sln` and produces the same `/app/SimpleTracker` layout.
