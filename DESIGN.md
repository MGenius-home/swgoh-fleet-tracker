# swgoh-fleet-tracker — Design Document

Design and architecture of [`MGenius-home/swgoh-fleet-tracker`](https://github.com/MGenius-home/swgoh-fleet-tracker) as of **v0.1.0**.

**Provenance:** the source tree in `src/` descends from [`iprobedroid/swgoh-arena-tracker`](https://github.com/iprobedroid/swgoh-arena-tracker), which itself grew out of [`DV1231/ccIPD-Arena-Tracker`](https://github.com/DV1231/ccIPD-Arena-Tracker) (GPL-3.0). This fork has substantially diverged: it compiles its own .NET 8 source (it no longer consumes any upstream image), tracks fleet arena only (squad arena is being removed from the game), and adds payout-shift detection, scheduled roster posts, weekly attack metrics, file-backed persistence, and a multi-arch build pipeline.

---

## 1. Overview

A single-process, long-running console worker. No inbound HTTP, no database. It polls Capital Games' SWGOH RPC for fleet arena ranks of a configured player list, announces rank climbs/falls and payout shifts to Discord webhooks, and supports a scheduled roster post plus a weekly attack summary.

## 2. Repository contents

| Path | Purpose |
|---|---|
| `Dockerfile` | Multi-stage build: `sdk:8.0` compiles `src/`, `runtime:8.0-noble-chiseled` runs it (shell-less; `busybox`+`gosu` and `zoneinfo` are copied in). Entrypoint starts as root, chowns `/app/data` to `PUID`/`PGID` (default 1654), then drops privileges via gosu before launching dotnet - bind mounts need no manual chown. |
| `docker-compose.yml` | Sample deployment with every optional feature documented. |
| `.github/workflows/docker-publish.yml` | Builds and publishes multi-arch (`linux/amd64`, `linux/arm64`) images to GHCR on pushes to `master` (tag `latest`) and on `v*` tags (semver tag). |
| `src/Ipd.GameClient` | Protobuf RPC client for Capital Games' SWGOH endpoint (hand-maintained generated protocol types). |
| `src/Ipd.Core` | Domain services, background jobs, message templates, models, cron/schedule utilities. |
| `src/SimpleTracker` | Composition root: `Program` (host wiring), `Tracker` (poll logic), `TrackerJob`, `WeeklyAttackSummaryJob`, `ScheduledStatusJob`, `FileStorageService`. |
| `README.md` / `DESIGN.md` | User documentation / this document. |
| `LICENSE` | GPL-3.0 (inherited obligation from DV1231 lineage). |

## 3. Runtime topology

```
┌───────────────────────────────────────────────┐
│ Docker host (x86 or arm64)                    │
│  ┌─────────────────────────────────────────┐  │
│  │ ghcr.io/mgenius-home/swgoh-fleet-       │  │
│  │ tracker  (.NET 8 console, single proc)  │  │
│  │  TrackerJob ── poll loop                │  │
│  │  WeeklyAttackSummaryJob ─ cron          │  │
│  │  ScheduledStatusJob ─ cron              │  │
│  │  DiscordMessengerJob ─ channel consumer │  │
│  │  state: /app/data/state.json            │  │
│  └─────────────────────────────────────────┘  │
└───────────────────────────────────────────────┘
     │ outbound HTTPS only
     ▼
 swprod.capitalgames.com/rpc   (protobuf game API)
 discord.com/api/webhooks/...  (one or two webhooks)
 swgoh-tracker-stats.herokuapp.com  (opt-in analytics beacon only)
```

## 4. Configuration

All configuration is environment-based (see README for the user-facing table). Internally `EnvSettingsService` (implementing `ISettingsService`) reads and clamps values; notable defaults:

| Variable | Default | Notes |
|---|---|---|
| `POLL_INTERVAL_SECONDS` | `15` | clamped to 2..3600 |
| `ENABLE_PAYOUT_TRACKING` | `FALSE` | all "new" features are opt-in |
| `ENABLE_WEEKLY_ATTACK_SUMMARY` | `FALSE` | |
| `WEEKLY_ATTACK_SUMMARY_CRON` | `0 0 * * 0` | only used when enabled |
| `STATUS_MESSAGE_CRON` | *(unset = off)* | |
| `SCHEDULE_TIMEZONE` | `UTC` | IANA id; unknown ids fall back to UTC with a logged warning |
| `STORAGE_FILE_PATH` | `/app/data/state.json` | |
| `ARENA_TYPE` | ignored | deprecated; logged as such when present |

## 5. Data source

`GameClient.GetSlimPlayerArenaRanks(allyCode)` POSTs a protobuf `RequestEnvelope` (method `PlayerRpc/GetPlayerArenaProfile`, platform spoofed as Android, `ClientExternalVersion` default `99.99.99`) to `https://swprod.capitalgames.com/rpc` and parses the gzip'd `SlimPlayerArenaProfileResponse`. The four values consumed are:

| Field | Meaning |
|---|---|
| `Name` | in-game name |
| `PvpProfile[Profilepvpship].Rank` | fleet arena rank (`-1` if absent) |
| `PvpProfile[Profilepvpcharacter].Rank` | squad rank (parsed but unused; squad is gone) |
| `LocalTimeZoneOffsetMinutes` | player's effective UTC offset, which encodes their chosen payout window |

There is **no payout timestamp and no time-to-payout field** in the protocol. Fleet payouts occur at 19:00 in the player's own (player-adjustable, in-game *Time Settings*) clock, so the tracker derives each player's UTC payout slot as `(19:00 − offset) mod 24h` (`PayoutService.GetUtcPayoutTime`). A player changing their Time Settings (or device timezone) changes the offset, which is what payout-shift detection observes.

## 6. Poll loop (`TrackerJob` → `Tracker.Track`)

`TrackerJob` runs `Track()` every `POLL_INTERVAL_SECONDS` (default 15 s). Each pass iterates players sequentially with a 200 ms throttle and, per player:

1. Fetch rank (`PlayerRankService` → `GameClient`). Per-player errors are logged and skipped; the loop never aborts.
2. **Bad-rank guard:** if the API returns no fleet rank (`-1`), keep the last known rank, refresh name/offset metadata only, and skip diffing — a transient API gap cannot fabricate a climb/drop or erase a player.
3. Load `TrackerState` once, mutate that in-memory copy for the player (rank, payout slot, attack counters), then save once. All persistence for a tick happens in this single pass.
4. Diff rank vs stored `CurrentRank`:
   - climb → `AttackTracker.ShouldCountAttack(offset)` (skips the 60-minute post-payout window, since shard reshuffles there are not attacks) then `WeeklyAttacks++`, and a climb message;
   - drop → drop message.
5. Payout shift: `RegisterPayoutObservation` requires the **same new slot on two consecutive polls** before announcing (candidate held in `PlayerState.PendingUtcPayoutTime`); one-poll bad data can never produce a shift embed.
6. Players no longer in the tracked list are pruned from state at the start of each pass (`PruneRemovedPlayers`); skipped when the tracked list is empty to protect against transient source failures.
7. Save state atomically (see §9).

## 7. Payout shift notifications

On a confirmed shift an embed is enqueued to `PAYOUT_WEBHOOK_URL` (falls back to `DISCORD_WEB_HOOK`): Player, Shift Delta (±h, wraparound-aware), New UTC Payout Time, Shared Payout Group (all other tracked players on the new slot), and — with `POST_FULL_PAYOUT_LIST_ON_CHANGE=TRUE` — the full payout order. Field values are truncated to Discord's 1024-char field limit.

## 8. Scheduled roster post (`ScheduledStatusJob`)

Off unless `STATUS_MESSAGE_CRON` is set. On a cron match (deduplicated via `LastScheduledStatusPost`), it renders every player through `CUSTOM_MESSAGE_STATUS`, sorts by time-to-payout (ties by rank), prepends a header, and enqueues the result as **one pre-chunked message** (≤25 lines / ≤1800 chars per chunk) so the channel consumer can never split or interleave a post. Players with rank ≤ 0 are excluded.

## 9. State and persistence (`FileStorageService`)

Single JSON file (schema below), written with temp-file + `File.Move` (unique temp per attempt), up to 5 retries with linear backoff, stale-temp sweep, and a process-wide lock. A **failed read of an existing file throws after retries** rather than returning an empty state — the tracker can never silently wipe good history; missing file = clean first run.

```json
{
  "Players": {
    "116563768": {
      "PlayerName": "Wayfayer",
      "CurrentRank": 4,
      "PreviousRank": 4,
      "UtcPayoutTime": "00:00",
      "PendingUtcPayoutTime": null,
      "TimezoneOffsetMinutes": -300,
      "WeeklyAttacks": 1,
      "LastAttackTimestamp": "2026-08-22T04:53:09Z"
    }
  },
  "LastWeeklySummaryPost": null,
  "LastScheduledStatusPost": null
}
```

Consequences: restarts are silent (baselines survive); only one container may share a state file.

## 10. Messaging pipeline (`DiscordMessengerJob`)

All outbound posts flow through an unbounded `Channel<DiscordMessage>`; the consumer drains it every second, groups by webhook URL, sends embeds individually, and batches plain-text messages into single POSTs capped at 25 lines **and** 1800 characters. Transport is `NewDiscordMessenger`: Polly retry (3×) with explicit 429 `RetryAfter` handling; every batch logs success (`Sent batch of N`) or failure. Successful sends are logged to make long-run auditing possible.

## 11. Scheduling engine

`CronExpression` implements standard 5-field cron (numeric, lists/ranges/steps, dom/dow OR rule) plus friendly forms (`SUNDAY 18:00`, `SUN 20:30`, `DAILY 12:00`, `DAILY`, `HOURLY`, `WEEKLY`). Both cron jobs tick every 60 s, evaluate against `SCHEDULE_TIMEZONE` (IANA; DST-aware), and deduplicate on a stored last-post timestamp so restarts cannot double-fire. Invalid expressions are logged and **disable the job** — they never crash the host.

## 12. Weekly attack summary (`WeeklyAttackSummaryJob`)

Off unless `ENABLE_WEEKLY_ATTACK_SUMMARY=TRUE`. On schedule: leaderboard embed (players sorted by `WeeklyAttacks`) to the payout webhook; only on successful delivery are counters reset (`ResetWeeklyCounters`) and `LastWeeklySummaryPost` recorded — a failed post leaves counters intact for the next tick.

## 13. Analytics beacon (`StatsService`)

**Unrelated to attack tracking.** When `ENABLE_ANALYTICS=TRUE`, one POST at startup sends usage stats (arena type, player count, which env-var names are set, tracker version, webhook URL hash, and — note — the raw webhook URL) to the upstream author's service `swgoh-tracker-stats.herokuapp.com/stats`. Default is off. Known wart: the raw URL should be hash-only; see §16.

## 14. Failure modes

| Failure | Behavior |
|---|---|
| `DISCORD_WEB_HOOK` missing | Logs and exits at startup. |
| Invalid `ALLY_CODES` entry | Player skipped with logged error. |
| Game API error per player | Logged (`Error processing allyCode:[...]`); loop continues. |
| Game API returns no fleet rank | Last known rank kept; metadata refreshed; no messages. |
| Discord 429 | Sleeps `RetryAfter`, then Polly retries (3×). |
| Discord send failure after retries | Logged; message dropped; state already saved (no resend). |
| State file unreadable | Load retries 3× then throws (job logs it); **never** overwritten with empty state. |
| State save transient failure | Retries 5× with unique temp files; throws to caller on exhaustion. |
| Invalid schedule expression | Job logs and disables itself; host keeps running. |
| Unknown `SCHEDULE_TIMEZONE` | Falls back to UTC with logged warning. |
| Host down | No polls; on return, state resumes from disk. |

## 15. Sequence diagram — one poll tick

```
 TrackerJob(15s)      Tracker                    Capital Games            Discord
      │                  │                             │                     │
      │─ Track() ───────►│                             │                     │
      │                  │─ Load state.json ──────────►│ (FileStorage)       │
      │                  │─ POST /rpc (protobuf) ─────►│                     │
      │                  │◄── SlimPlayerArenaProfile ──│                     │
      │                  │  per player:                │                     │
      │                  │   rank diff / payout slot   │                     │
      │                  │   attack++ (climb, outside  │                     │
      │                  │    payout window)           │                     │
      │                  │─ enqueue messages ─────────────────────────────► │ Channel
      │                  │─ Save state.json ──────────►│                     │
      │                  │                             │   DiscordMessengerJob(1s)
      │                  │                             │   batches ≤25 lines/≤1800 chars
      │                  │                             │──────── POST ─────►│
      │                  │                             │◄── 200/204 ────────│
```

## 16. Known debt / future work

- `GameClient` still uses `HttpWebRequest` (obsolete, no explicit timeout/retry) — migration to `IHttpClientFactory` + Polly is planned.
- `RestSharp` 106.x remains referenced by `StatsService` (known vulnerability advisory NU1903); replacing it with plain `HttpClient` also enables dropping the package.
- Analytics beacon includes the raw webhook URL; should send the hash only.
- Polling, storage, and Discord transport are synchronous-over-async in places (`Task.Result`); an async refactor is planned.
- No automated test project in-repo (verification currently via external harnesses).
