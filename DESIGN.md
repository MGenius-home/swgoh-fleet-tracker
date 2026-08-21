# ccIPD-arena-tracker — Design Document

A basic design document for the fork [`MGenius-home/ccIPD-arena-tracker`](https://github.com/MGenius-home/ccIPD-arena-tracker), which wraps the upstream image `iprobedroid/swgoh-arena-tracker:beta-24`.

This document was rewritten after the upstream binary was decompiled into a buildable .NET 8 solution under `src/`. Every behavioral claim below is now grounded in the recovered C# source with `path:line` citations.

---

## 1. Overview

A long-running .NET console worker that polls Star Wars: Galaxy of Heroes (SWGOH) arena rankings for a configured list of players and posts Discord notifications when their ranks change. It runs as a Heroku container `worker` dyno — there is no web traffic and no inbound HTTP.

```
Heroku worker dyno
└─ SimpleTracker (.NET 8 console, target IpdArenaTracker.sln)
   ├─ TrackerJob            ← main poll loop (every 2s)
   │   └─ Tracker.Track()   ← iterates player settings, fetches ranks, diffs
   │       └─ writes DiscordMessage to channel
   └─ DiscordMessengerJob   ← drains channel, batches ≤10, POSTs webhook
       └─ NewDiscordMessenger (Polly retry, honors 429 Retry-After)
```

---

## 2. Repository contents

| Path | Purpose |
|---|---|
| `Dockerfile` | `FROM iprobedroid/swgoh-arena-tracker:beta-24` (the upstream image) |
| `heroku.yml` | Heroku container stack, builds a `worker` process |
| `app.json` | Heroku app manifest |
| `README.md` | Deploy walkthrough (with deploy button pointing at this fork) |
| `assets/` | PNGs used by the README |
| `DESIGN.md` | This file |
| `src/` | **Recovered .NET 8 source** — decompiled from `beta-24` with ILSpy, modified to retarget `net8.0` and use modern NuGet packages. Builds cleanly. |
| `src/IpdArenaTracker.sln` | Solution file |
| `src/Ipd.GameClient/` | SWGOH protobuf + RPC client |
| `src/Ipd.Core/` | Domain models, services, message templates, background jobs |
| `src/SimpleTracker/` | Entry point + composition root + tracker loop |

The fork ships the upstream Docker image as-is. The `src/` tree is a parallel buildable copy you can modify and replace the image with.

---

## 3. Build / run

```bash
cd src
dotnet restore
dotnet build                # → 0 errors, 4 warnings (all in original code)
dotnet run --project SimpleTracker   # uses env vars from current shell
```

Or build the container:

```bash
docker build -t my-tracker src/
```

(You'll need a Dockerfile that wraps the `dotnet publish` output.)

---

## 4. Runtime topology

```
                            ┌──────────────────────────────────────────┐
                            │  Heroku container dyno (process: worker) │
                            │  ┌────────────────────────────────────┐  │
                            │  │ SimpleTracker (net8.0 console)     │  │
                            │  └────────────────────────────────────┘  │
                            └────────────┬─────────────────────────────┘
                                         │ outbound HTTPS only
                ┌────────────────────────┼────────────────────────┐
                ▼                        ▼                        ▼
    swprod.capitalgames.com:443  swgoh-tracker-stats:443   discord.com:443
    (game RPC, protobuf+gzip)     (player metadata +          (webhook POST
                                  analytics beacon)            JSON content)
```

The entry point is `SimpleTracker/Program.cs:Main`. It:
1. Builds a logger from `LOGGER_TYPE` / `LOGGER_HOOK` env vars (`Program.cs:24-42`).
2. Bails out if `DISCORD_WEB_HOOK` is missing (`Program.cs:63-66`).
3. Calls `CreateHostBuilder(args).Build().Run()` which registers two `IHostedService`s and starts them: `TrackerJob` (poll loop) and `DiscordMessengerJob` (webhook dispatcher) (`Program.cs:44-58`).

Both jobs inherit from `BackgroundService` and run an infinite `while (!stoppingToken.IsCancellationRequested)` loop with `Task.Delay` between iterations.

---

## 5. Source layout

Three projects, in dependency order:

### `Ipd.GameClient` — SWGOH RPC client
Encodes/decodes protobuf for the SWGOH mobile-client wire protocol.
- `Ipd.Game.Protocol/` — protobuf-generated types: `RequestEnvelope`, `ResponseEnvelope`, `PlayerProfileRequest`, `SlimPlayerArenaProfileResponse`, `PlayerArenaStatus`, etc. (originally auto-generated from `.proto` schemas whose `.desc` blobs are embedded in the DLL).
- `Ipd.GameClient/GameClient.cs` — wraps a synchronous HTTP POST to `https://swprod.capitalgames.com/rpc` (`GameClient.cs:102`). Builds an envelope with `ServiceName=PlayerRpc`, `MethodName=GetPlayerArenaProfile`, `Platform=Android`, `ClientExternalVersion=99.99.99`, `Region=NA`, gzip `AcceptEncoding`. Reads `SlimPlayerArenaProfileResponse`, filters `PvpProfile` by `Profilepvpcharacter` / `Profilepvpship` tabs to extract squad and fleet ranks.
- `Ipd.GameClient.Models/PlayerArena.cs` — the domain shape returned by the client.

### `Ipd.Core` — domain + services
- `Ipd.Core.Interfaces/` — abstractions: `IPlayerSettingsProvider`, `IPlayerRankService`, `IArenaRankStorage`, `IDiscordMessenger`, `INewDiscordMessenger`, `IAllyCodesProvider`, `IStatsService`, `ITagsProvider`, `ILog`.
- `Ipd.Core.Models/` — `PlayerSettings`, `PlayerArenaRank`, `ArenaType`, `AuthResponse`, `TrackerStats`.
- `Ipd.Core.Models.Discord/` — `DiscordMessage` (just a URL + a string body), `MessageType`.
- `Ipd.Core.Messages/MessageMap.cs` — the placeholder→value dictionary used to render messages (placeholders: `%NAME%`, `%USER_ICON%`, `%PLAYER_NAME%`, `%ALLY_CODE%`, `%CURRENT_RANK%`, `%PREVIOUS_RANK%`, `%TIME_TO_PO%`, `%TAG_ON_CLIMB%`, `%TAG_ON_DROP%`).
- `Ipd.Core.Messages/MessageGenerator.cs` — three template-string functions (`GenerateStatusMessage`, `GenerateMessageOnClimb`, `GenerateMessageOnDrop`).
- `Ipd.Core.Messages/EnvSettingsService.cs` — reads the env-var-driven config (`MessageFormatOnStatus`, `MessageFormatOnClimb`, `MessageFormatOnDrop`, `IsStatusMessageDisabled`, `TagOnDropRankLimit`, `TagOnClimbRankLimit`, `TagOnDropPayoutLimitMins`).
- `Ipd.Core.Services/`:
  - `PlayerSettingsUrlProvider.cs` — `GET $ALLY_CODES_URL` → `List<PlayerSettings>` (JSON).
  - `PlayerSettingsEnvProvider.cs` — reads env vars (`ALLY_CODES`, `AC_*` prefix, `DISCORD_TAGS`).
  - `PlayerRankService.cs` — adapts `IGameClient.GetSlimPlayerArenaRanks` into `IPlayerRankService.GetPlayerRank`, picking `SquadArenaRank` or `FleetArenaRank` per `ArenaType`.
  - `StatsService.cs` — POSTs a `TrackerStats` beacon to `https://swgoh-tracker-stats.herokuapp.com/stats` on startup.
  - `DiscordMessenger.cs` — legacy webhook sender (creates its own `HttpClient`, no DI).
  - `NewDiscordMessenger.cs` — DI-friendly webhook sender (takes `HttpClient` via ctor).
  - `DiscordLogger.cs` — mirror log lines to a Discord channel.
  - `EnvTagsProvider.cs` — `DiscordId` / `TagIdOnClimb` / `TagIdOnDrop` per player.
  - `RandomPlayerRankService.cs` — test helper (returns random ranks).
- `Ipd.Core.Utils/`:
  - `ExecutionThrottle.cs` — sleeps between iterations to avoid rate-limit storms.
  - `PoUtils.cs` — computes `Duration timeToPo` for the next arena payout. Squad payouts at 18:00 UTC, fleet at 19:00 UTC; subtracts the player's local TZ offset; if the result is in the past, adds 24 h.
- `Ipd.Core.Jobs/DiscordMessengerJob.cs` — the webhook dispatcher BackgroundService.
- `Ipd.Core.Extensions/` — `StringExtensions.NormalizeAllyCode`, `TimeExtension.ToPayoutString` (formats `Duration` as `HH:MM`), `AsyncExtensions.ToListAsync`.

### `SimpleTracker` — entry point + composition
- `SimpleTracker/Program.cs` — env-var parsing + DI wiring + `Main` bootstrap.
- `SimpleTracker/Tracker.cs` — the per-poll orchestrator.
- `SimpleTracker.Infrastructure/TrackerJob.cs` — the poll loop `BackgroundService`.
- `SimpleTracker.Infrastructure/MetricsJob.cs` — unused / disabled (referenced by name but not registered).
- `SimpleTracker.Services/StaticArenaRankStorage.cs` — in-process `ConcurrentDictionary<string, int>` keyed by ally code.
- `SimpleTracker.Services/EnvAllyCodesProvider.cs` — reads `AC_*` env vars (alternate to `ALLY_CODES`).
- `SimpleTracker.Services/EnvCsvAllyCodesProvider.cs` — reads `ALLY_CODES` CSV.

---

## 6. Configuration (environment variables)

All env vars are read in `Program.cs` (entry point) and `EnvSettingsService.cs` (per-poll config). The full list lives in `StatsService.cs:15`:

```
ARENA_TYPE, DISCORD_WEB_HOOK, GAME_CLIENT_VERSION, ALLY_CODES,
DISCORD_TAGS, ALLY_CODES_URL, CUSTOM_MESSAGE_STATUS, CUSTOM_MESSAGE_DROP, CUSTOM_MESSAGE_CLIMB
```

Plus undocumented-but-honored ones: `DISABLE_STATUS_MESSAGE`, `TAG_ON_CLIMB_RANK_LIMIT`, `TAG_ON_DROP_RANK_LIMIT`, `TAG_ON_DROP_PO_LIMIT`, `LOGGER_TYPE`, `LOGGER_HOOK`, `DISABLE_ANALYTICS`, `AC_<digits>` (one var per ally code).

| Variable | Required | Purpose | Read at |
|---|---|---|---|
| `DISCORD_WEB_HOOK` | yes | Full Discord webhook URL. If missing, the process logs and exits (`Program.cs:63-66`). | startup |
| `ALLY_CODES_URL` | one of two | HTTPS URL returning JSON `List<PlayerSettings>`. Wins over `ALLY_CODES` if set. | startup |
| `ALLY_CODES` | one of two | Inline comma-separated ally codes. Each is a 9-digit string. | startup |
| `AC_123456789` | optional | Per-player env var (any number of them) — alternate ally-code source. Up to 75 are taken. `EnvAllyCodesProvider.cs:14-22`. | startup |
| `ARENA_TYPE` | optional | `SQUAD` (default) or `FLEET`. Anything other than `FLEET` (case-insensitive) is `Squad`. `Program.cs:77`. | startup |
| `GAME_CLIENT_VERSION` | optional | Override the spoofed SWGOH client version. Default `99.99.99`. `Program.cs:75`. | startup |
| `DISCORD_TAGS` | optional | Pipe/comma separated `discordId` list, applied per player via `TagIdOnClimb` / `TagIdOnDrop` (those fields are in `PlayerSettings`). | per poll |
| `CUSTOM_MESSAGE_STATUS` | optional | Override status message template. Default `"%USER_ICON%\`%PLAYER_NAME%\` is at %CURRENT_RANK%. payout in \`%TIME_TO_PO%\`"`. | per poll |
| `CUSTOM_MESSAGE_CLIMB` | optional | Override climb template. Default `"%TAG_ON_CLIMB%%USER_ICON%\`%PLAYER_NAME%\` climbed from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in \`%TIME_TO_PO%\`"`. | per poll |
| `CUSTOM_MESSAGE_DROP` | optional | Override drop template. Default `"%TAG_ON_DROP%%USER_ICON%\`%PLAYER_NAME%\` dropped from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in \`%TIME_TO_PO%\`"`. | per poll |
| `DISABLE_STATUS_MESSAGE` | optional | `TRUE` to suppress the periodic status message (only climb/drop events fire). `EnvSettingsService.cs:41-52`. | per poll |
| `TAG_ON_CLIMB_RANK_LIMIT` | optional | Numeric — only attach `@<tagIdOnClimb>` when the new rank is at or below this. Default `1000`. | per poll |
| `TAG_ON_DROP_RANK_LIMIT` | optional | Numeric — only attach `@<tagIdOnDrop>` when the new rank is at or above this AND time-to-payout is below `TAG_ON_DROP_PO_LIMIT`. Default `0`. | per poll |
| `TAG_ON_DROP_PO_LIMIT` | optional | Numeric minutes — payout-urgency gate for drop tagging. Default `1440`. | per poll |
| `LOGGER_TYPE` | optional | `CONSOLE` (default) or `DISCORD`. `Program.cs:26`. | startup |
| `LOGGER_HOOK` | conditional | Webhook URL when `LOGGER_TYPE=DISCORD`. `Program.cs:27`. | startup |
| `DISABLE_ANALYTICS` | optional | Set to non-empty to suppress the `swgoh-tracker-stats.herokuapp.com/stats` beacon. `StatsService.cs:69`. | startup |

### Player JSON schema (`ALLY_CODES_URL` format)

`PlayerSettings` (`PlayerSettings.cs:3-20`):
```json
{
  "name": "string (optional)",
  "allyCode": "123456789",
  "discordId": "string (optional, used by SendTextTaggedMessage)",
  "userIcon": ":emoji_code:",
  "skip": false,
  "comment": "string (optional, unused)",
  "tagIdOnClimb": "string (Discord ID, optional)",
  "tagIdOnDrop":  "string (Discord ID, optional)"
}
```

Discord IDs are trimmed on load (`PlayerSettingsUrlProvider.cs:36-38`). Empty values are safe.

---

## 7. Poll loop — sequence

The two BackgroundServices run concurrently:

### 7.1 `TrackerJob` (the poller)
`SimpleTracker.Infrastructure/TrackerJob.cs:21-36`:
```
while not cancelled:
    try { tracker.Track(); } catch { log "2 seconds sleep to retry"; }
    await Task.Delay(2000, stoppingToken)
```
**Cadence: every 2 seconds** (`TrackerJob.cs:34`).

Inside `Tracker.Track()` (`Tracker.cs:67-79`):
1. Fetch the player settings list (env-var provider or URL provider). Sync `.Result` over an async method — blocking on the current thread, but the upstream `Host` model accepts that.
2. For each non-skipped player, call `TrackOneAllyCode` inside `ExecutionThrottle.ThrottleSync(200, …)` — a 200 ms sleep between players to be polite to the SWGOH API.

Inside `TrackOneAllyCode` (`Tracker.cs:81-115`):
1. `PlayerRankService.GetPlayerRank(allyCode, new AuthResponse())` — actually calls `GameClient.GetSlimPlayerArenaRanks` (sync `HttpWebRequest` to Capital Games).
2. Pick `rank = ArenaType == Fleet ? result.FleetArenaRank : result.SquadArenaRank`.
3. `prev = ArenaRankStorage.GetRank(allyCode)` (from in-memory dict; `null` if first time).
4. `ArenaRankStorage.SaveRank(allyCode, rank)` — always save, regardless of message dispatch.
5. Compute `timeToPo = PoUtils.GetPoTime(result.PayoutOffsetMinutes, ArenaType)` — payout countdown using the player's local TZ offset.
6. Build a `MessageMap` (placeholder→value dict) with name, ranks, time, and conditionally-filled tags.
7. Decide which message (if any) to send (`Tracker.cs:91-109`):
   - `prev == null` (first time we see this player) → status message, unless `DISABLE_STATUS_MESSAGE=TRUE`.
   - `prev != null && prev != rank` → climb if `rank < prev`, drop if `rank > prev`.
   - `prev == rank` → no message (silent tick).
8. The chosen `SendXxxMessage` calls `MessageGenerator.GenerateXxx(map, customFormat)`, which substitutes placeholders, then `WriteDiscordMessage` enqueues a `DiscordMessage { DiscrodHookUrl, Message }` into the bounded `Channel<DiscordMessage>` (capacity 10, see `DiscordMessengerJob.cs:23`).

### 7.2 `DiscordMessengerJob` (the webhook dispatcher)
`Ipd.Core.Jobs/DiscordMessengerJob.cs:36-69`:
```
while not cancelled:
    drain channel → group messages by webhook URL
    for each batch of ≤10 (MoreLinq.Batch(10)):
        text = string.Join('\n', batch.Trim())
        try { await messenger.SendTextMessage(url, text); }
             catch { log }
    await Task.Delay(1000)
```
So the dispatch loop is also a 1-second tick; it batches everything in the channel into 10-line messages and POSTs them as a single Discord webhook call. Multiple status messages per tick are concatenated with newlines.

---

## 8. Data source — `swprod.capitalgames.com`

`Ipd.GameClient/GameClient.cs:100-113` (the only outbound call to Capital Games):
- **Method/URL:** `POST https://swprod.capitalgames.com/rpc`
- **Request body:** protobuf-encoded `RequestEnvelope`
  - `serviceName = "PlayerRpc"`
  - `methodName = "GetPlayerArenaProfile"`
  - `payload = PlayerProfileRequest { playerId="", allyCode=NNNNNNNNN }`
  - `clientVersion = 181815` (hard-coded internal)
  - `clientExternalVersion = clientInternalVersion = GameClientVersion ?? "99.99.99"` — set from `GAME_CLIENT_VERSION` env var or default.
  - `clientStartupTimestamp = floor(DateTime.Now.Ticks/1000) - 10` (millisecond Unix epoch - 10).
  - `currentClientTime = clientStartupTimestamp + 8`
  - `platform = "Android"`
  - `region = "NA"`
  - `acceptEncoding = Gzipacceptencoding`
  - `networkAccess = "W"`
  - `application = "ipd-arena-tracker:" + StatsService.ClientVersion` (`Program.cs:109`)
  - `requestId = Guid.NewGuid().ToString().ToLower()`
- **Headers:** `Content-Type: application/x-protobuf`, `Content-Length`, `Accept-Encoding: gzip`.
- **Response:** `ResponseEnvelope` protobuf; on success the payload is `byte[]` of a gzipped `SlimPlayerArenaProfileResponse`.
- **Rank extraction** (`GameClient.cs:53-64`): finds the `PlayerPvpProfile` in `PvpProfile` whose `Tab` is `Profilepvpcharacter` (squad) or `Profilepvpship` (fleet) and reads its `Rank`. Default `-1` if not present (which is how new players or the start of a season look).
- **Transport:** synchronous `HttpWebRequest` (`SYSLIB0014` warning under .NET 8). No retry at this layer; failures bubble up as `GameClientApiException` (`GameClient.cs:46`) and are caught in `TrackOneAllyCode` (`Tracker.cs:111-114`) which logs `Error processing allyCode:[NNN]:…` and continues.

There is **no authentication**. The Capital Games endpoint accepts `GetPlayerArenaProfile` calls from any client that speaks the protocol. The app spoofs an Android client with `clientVersion=99.99.99`.

### Secondary service — `swgoh-tracker-stats.herokuapp.com`

Two interactions:
- **Analytics beacon** (`StatsService.cs:67-95`): one POST per process startup (called from `Tracker.PostStats()` which is invoked once during DI wiring in `Program.cs:55`). Disabled if `DISABLE_ANALYTICS` is non-empty.
- **Player metadata**: this is *not* actually called from this code path. The `swgoh-tracker-stats.herokuapp.com` URL is hard-coded only in `StatsService.cs`. Player settings come from either env vars (`PlayerSettingsEnvProvider`) or the user-supplied `ALLY_CODES_URL` gist (`PlayerSettingsUrlProvider`). The original description mentioning stats for player metadata was wrong — `ALLY_CODES_URL` is whatever URL the user provides.

---

## 9. Discord webhook update

`Ipd.Core.Services/NewDiscordMessenger.cs:27-79` is the only sender registered for the new channel pipeline:
- **Method:** `POST $discordWebHook`
- **Body:** `application/json` with `{ "content": "..." }` (`NewDiscordMessenger.cs:29-33`).
- **Retry:** `Polly.RetryAsync(3, …)` — up to 3 retries.
  - On `HttpStatusCode.TooManyRequests` (429), parses `retry_after` (snake-case via `Newtonsoft.Json`'s `SnakeCaseNamingStrategy`) and `Task.Delay` for that long, then retries (`NewDiscordMessenger.cs:38-65`).
  - On any other non-success status, falls through with no further delay.
- **Per-tick layout:** the dispatcher joins up to 10 trimmed messages with `\n` and sends them as one webhook call (`DiscordMessengerJob.cs:51-54`).

Discord webhook URLs are self-authenticating — no `Authorization` header.

### Message rendering

Templates from `MessageGenerator.cs:8-12`:
```
STATUS: %USER_ICON%`%PLAYER_NAME%` is at %CURRENT_RANK%. payout in `%TIME_TO_PO%`
CLIMB:  %TAG_ON_CLIMB%%USER_ICON%`%PLAYER_NAME%` climbed from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in `%TIME_TO_PO%`
DROP:   %TAG_ON_DROP%%USER_ICON%`%PLAYER_NAME%` dropped from %PREVIOUS_RANK% to %CURRENT_RANK%. payout in `%TIME_TO_PO%`
```

`MessageGenerator.GenerateXxx` performs plain `string.Replace` per placeholder (`MessageGenerator.cs:17-21`).

`TimeToPo` is formatted by `TimeExtension.ToPayoutString` as `HH:MM` (zero-padded). Payouts are computed in `PoUtils.cs:9-20` (squad at 18:00 UTC, fleet at 19:00 UTC, minus the player's local TZ offset, +24h if in the past).

Tag placeholders are conditionally populated in `Tracker.PopulateMessageMap` (`Tracker.cs:128-135`):
- `%TAG_ON_DROP%` is filled with `<@discordId>` only if `currentRank >= TagOnDropRankLimit` *and* `timeToPo.TotalMinutes < TagOnDropPayoutLimitMins`.
- `%TAG_ON_CLIMB%` is filled with `<@discordId>` only if `currentRank <= TagOnClimbRankLimit`.

---

## 10. State and persistence

`SimpleTracker.Services/StaticArenaRankStorage.cs` is a static `ConcurrentDictionary<string, int>` (`StaticArenaRankStorage.cs:11`). It's process-memory only — no file, no DB.

Consequences:
- Restarting the dyno resets every player's "previous rank" to absent. The first poll tick after restart posts a status message per player (unless `DISABLE_STATUS_MESSAGE=TRUE`), which on a 50-player guild is a flood of ~5 webhook calls (10 messages / call).
- Running two instances in parallel would race and post duplicate messages.

---

## 11. Failure modes

| Failure | Behavior | Source |
|---|---|---|
| `DISCORD_WEB_HOOK` missing at startup | Logs `env variable DISCORD_WEB_HOOK not found`, exits. | `Program.cs:65` |
| `ALLY_CODES` and `ALLY_CODES_URL` both missing | Process boots with zero tracked players; no messages sent. | `Program.cs:93-95` |
| `ALLY_CODES_URL` returns non-2xx | Logs `Status code (NNN)`, returns empty list — no players tracked this run. | `PlayerSettingsUrlProvider.cs:29-30` |
| `ALLY_CODES_URL` returns malformed JSON | Logs `Failed to deserialize player settings: …`, returns empty list. | `PlayerSettingsUrlProvider.cs:42-44` |
| Ally code not 9 digits after normalization | The downstream Capital Games call returns an error envelope, thrown as `GameClientApiException`, caught and logged per ally code. Other players continue. | `Tracker.cs:111-114` |
| Capital Games 5xx / network error | Exception bubbles through `TrackOneAllyCode`, logged as `Error processing allyCode:[NNN]:…`. That player is skipped this tick; others continue. | `Tracker.cs:111-114` |
| Discord webhook returns 429 | `NewDiscordMessenger` reads `retry_after`, sleeps that long, retries up to 3 times. | `NewDiscordMessenger.cs:38-65` |
| Discord webhook returns non-429 non-2xx | Polly retries up to 3 times with no inter-attempt delay; on final failure logs `Request failed with StatusCode(NNN).`. | `NewDiscordMessenger.cs:36-70` |
| Channel is full (10 pending messages) | `Channel.Writer.TryWrite` returns false; logs `Error: failed to enqueue discord message`. The message is dropped. | `Tracker.cs:146-150` |
| TrackerJob outer exception | Caught, logged as `ERROR:…`, sleeps 2 seconds, retries. | `TrackerJob.cs:25-33` |
| DiscordMessengerJob outer exception | Caught, logged, sleeps 1 second, retries. | `DiscordMessengerJob.cs:62-66` |
| Heroku dyno sleep (free tier, no credit card) | Process pauses; no polls; no messages. | Heroku runtime, not in code |

---

## 12. Fork vs upstream

| File | This fork | Upstream |
|---|---|---|
| `Dockerfile` | `FROM iprobedroid/swgoh-arena-tracker:beta-24` | Same |
| `app.json` | unchanged | identical |
| `README.md` | deploy button URL swapped to this fork | identical otherwise |
| Source code | **none in repo** (added in `src/`) | **none in repo** |

Practical implications:
- The image `:beta-24` is pinned. Upstream can move/delete that tag without notice.
- To change runtime behavior you must (a) modify `src/`, (b) build a new image, (c) update the fork's `Dockerfile` to point at your tag.
- The upstream project has effectively been frozen since 2021 (last commit to upstream Dockerfile was `de6fe84` per `git log`); no source was ever published.

---

## 13. Build details for `src/`

The `src/` tree is a faithful decompilation of `beta-24` with retargeting changes:

| Aspect | Original (decompiled from image) | `src/` |
|---|---|---|
| TFM | `net5.0` | `net8.0` |
| NuGet packages | shipped as DLLs in the image, referenced via `<HintPath>` | proper `PackageReference`s with current versions |
| `Ipd.Core.Extensions.StringExtensions.NormalizeAllyCode` etc. | preserved | preserved |
| `GameClient.BasicPostRequest` (`HttpWebRequest`) | preserved with `SYSLIB0014` warning | preserved with same warning |
| Protobuf-generated `Ipd.Game.Protocol.*` | preserved (ILSpy already emitted them) | preserved |

Build result: `0 errors, 4 warnings` — all warnings are in original decompiled code:
- `SYSLIB0014` — `HttpWebRequest` is obsolete (`Ipd.GameClient/GameClient.cs:102`).
- Two `CA2017` warnings — logger template parameter mismatches in `DiscordMessengerJob.cs:58` and `:65`. These are also in the original binary.
- `NU1903` — RestSharp 106.10.1 has a known vulnerability. Acceptable for this personal build but worth bumping if you fork.

The decompiled output is byte-identical in behavior to the upstream image; nothing was added, removed, or rewritten beyond the target-framework swap and the NuGet switch.

---

## 14. Open questions / caveats

- The upstream RPC endpoint (`swprod.capitalgames.com/rpc`) may reject requests with the spoofed `clientVersion=99.99.99` if Capital Games tightens server-side checks. The version is hard-coded in `GameClient.cs:88` and overridable per env var.
- The `AuthResponse` object passed to `PlayerRankService.GetPlayerRank` is always `new AuthResponse()` — empty. Capital Games does not currently require auth for `GetPlayerArenaProfile`, but if they add it, that's the insertion point.
- `MetricsJob` is referenced by name in the decompiled `SimpleTracker.Infrastructure.MetricsJob.cs` but is not registered as a hosted service in `Program.cs:46-58`. It appears to be dead code in the upstream image.
- The `ALLY_CODES_URL` is fetched **synchronously once at startup** (`PlayerSettingsUrlProvider.GetPlayerSettingAsync().Result` at `Tracker.cs:69`). Updating the gist doesn't take effect until the dyno restarts.
