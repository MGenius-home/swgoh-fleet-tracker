# Pulling SWGOH Game Data Directly from the Server

How to fetch arena data straight from Capital Games' SWGOH servers — the same RPC the mobile game client uses — with **no game DLLs, no third-party services, and no login credentials**. Everything below is reconstructed from this repo's own protocol code (`src/Ipd.GameClient/Ipd.Game.Protocol/*`, `src/Ipd.GameClient/Ipd.GameClient/GameClient.cs`), which is what `swgoh-fleet-tracker` itself runs on.

---

## 1. The endpoint

```
POST https://swprod.capitalgames.com/rpc
Content-Type: application/x-protobuf
Accept-Encoding: gzip
```

- The request body is a **protobuf-encoded `RequestEnvelope`**.
- The response body is a protobuf-encoded `ResponseEnvelope`; its payload field is a **gzip-compressed, protobuf-encoded result message**.
- No authentication is needed for arena profile reads — no account, no password, no token. You identify as an anonymous "client", not as a player.

Because the body is binary protobuf, plain `curl` is awkward (you must feed it a pre-built byte file). Any protobuf library makes this trivial; a complete Python example is in §5.

---

## 2. The protocol messages (.proto reconstruction)

```proto
syntax = "proto3";

// ---- request wrapper -------------------------------------------------
message RequestEnvelope {
  int32    correlation_id          = 1;
  string   service_name            = 4;   // "PlayerRpc"
  string   method_name             = 5;   // "GetPlayerArenaProfile"
  bytes    payload                 = 6;   // inner PlayerProfileRequest
  string   auth_id                 = 7;   // unused for reads (empty)
  string   auth_token              = 8;   // unused for reads (empty)
  int32    client_version          = 9;   // 181815
  int64    client_startup_timestamp = 11;
  string   platform                = 12;  // "Android"
  string   region                  = 13;  // "NA"
  string   client_external_version = 14;  // "99.99.99"
  string   client_internal_version = 15;  // "99.99.99"
  string   request_id              = 16;  // lowercase GUID string
  enum AcceptEncoding { DEFAULTACCEPTENCODING = 0; GZIPACCEPTENCODING = 1; }
  AcceptEncoding accept_encoding   = 17;  // GZIPACCEPTENCODING
  int64    current_client_time     = 20;
  string   network_access          = 25;  // "W"
  string   application             = 37;  // free-form app id string
}

// ---- inner request (the envelope payload) ----------------------------
message PlayerProfileRequest {
  string player_id = 1;   // empty when looking up by ally code
  string ally_code = 2;   // 9-digit ally code AS A STRING (not int!)
}

// ---- response wrapper (complete field inventory) ----------------------
message ResponseEnvelope {
  int32  correlation_id       = 1;
  int64  current_server_time  = 2;
  bytes  payload              = 4;   // gzip'd inner response (see below)
  enum Code {
    CODE_DEFAULT = 0;
    OK = 1; ERROR = 2; SERVERERROR = 3; SESSIONEXPIRED = 4; AUTHFAILED = 5;
    RATEEXCEEDED = 6; SERVERUNAVAILABLE = 7; INVALIDREQUEST = 8;
    INVALIDDATA = 9; UNAUTHORIZED = 11; SUSPENDED = 12; RECORDNOTFOUND = 32;
    INVALIDCLIENTVERSION = 50; /* ...others exist */
  }
  Code   code                 = 5;   // 1 == success
  string message              = 6;
  enum ContentEncoding { DEFAULTCONTENTENCODING = 0; GZIPCONTENTENCODING = 1; }
  ContentEncoding content_encoding = 7;
  string stack_trace          = 8;   // server-side error detail (error paths only)
  bytes  dynamic_message      = 9;   // unused for this call
  string maintenance_message  = 10;
  string maintenance_link     = 11;
  int32  sub_code             = 12;
}

// ---- inner response (after gunzip) — complete field inventory ---------
message SlimPlayerArenaProfileResponse {
  string name                        = 1;   // in-game name
  int32  level                       = 2;   // account level (proto3: absent == 0)
  int64  ally_code                   = 3;
  string player_id                   = 4;   // CG internal player id
  repeated PlayerPvpProfile pvp_profile = 5;
  sint32 local_timezone_offset_minutes = 6;   // player's effective UTC offset
}

message PlayerPvpProfile {
  enum PlayerProfileTab {
    PLAYERPROFILETAB_DEFAULT = 0;
    PROFILEPVPCHARACTER      = 1;   // squad arena
    PROFILEPVPSHIP           = 2;   // fleet arena
    PROFILEPVPTOURNAMENT     = 3;
  }
  PlayerProfileTab tab  = 1;
  int32 rank            = 2;      // current rank in that tab (-style absent => not set)
  string event_id       = 4;
}
```

> Note `local_timezone_offset_minutes` is a **zigzag (sint32)** field — negative offsets like `-300` are encoded as `599` on the wire. Most protobuf libraries handle this automatically when the type is declared `sint32`.

> **Field inventory:** the `.proto` above is the complete set of fields these three messages carry — there is no hidden payload beyond what's listed. `SlimPlayerArenaProfileResponse` has exactly the 6 fields shown; `PlayerPvpProfile` exactly 3 (`tab`, `rank`, `event_id` — field number 3 does not exist). Remember proto3 semantics: a field that is zero/empty on the wire is indistinguishable from "not set", so a missing `rank` means the player has no standing in that tab rather than rank 0.

---

## 3. Building the request

Per-player request values that work in practice (these are exactly what this tracker sends):

| Envelope field | Value |
|---|---|
| `service_name` | `"PlayerRpc"` |
| `method_name` | `"GetPlayerArenaProfile"` |
| `payload` | encoded `PlayerProfileRequest { player_id: "", ally_code: <code> }` |
| `client_version` | `181815` |
| `platform` | `"Android"` |
| `region` | `"NA"` |
| `client_external_version` / `client_internal_version` | `"99.99.99"` |
| `request_id` | fresh lowercase GUID per call |
| `accept_encoding` | `GZIPACCEPTENCODING` (= 1) |
| `correlation_id` | `0` |
| `client_startup_timestamp` / `current_client_time` | any plausible epoch-style numbers |
| `network_access` | `"W"` |
| `application` | any app-id string |

The server does not appear to validate most of these for read calls — but it does reject requests missing the envelope structure entirely.

---

## 4. Reading the response

1. Parse the body as `ResponseEnvelope`.
2. Check `code == 1` (OK). Anything else is an error — common ones:
   - `RECORDNOTFOUND (32)` — ally code doesn't exist
   - `RATEEXCEEDED (6)` — you polled too aggressively
   - `INVALIDCLIENTVERSION (50)` — CG tightened version checks
3. If OK: **gunzip** `payload`, then parse the decompressed bytes as `SlimPlayerArenaProfileResponse`.
4. Extract fleet rank from the `pvp_profile` entry whose `tab == PROFILEPVPSHIP (2)`.
5. Derive payout time: fleet payouts occur at **19:00 in the player's own local clock**, so:

```
utc_payout_slot_minutes = ((19 * 60 − local_timezone_offset_minutes) mod 1440 + 1440) mod 1440
→ "HH:mm" UTC payout moment for that specific player
```

Every player has a different slot because each sets their own reward time in-game (Time Settings screen), which is what changes `local_timezone_offset_minutes`.

---

## 5. Complete Python example

Requires only `pip install protobuf` and the `.proto` above compiled once (`protoc --python_out=. swgoh.proto`):

```python
import gzip, time, uuid, requests
import swgoh_pb2 as pb

URL = "https://swprod.capitalgames.com/rpc"

def get_arena(ally_code: str):
    startup = int((time.time() + 62135596800) * 1000)   # .NET-style ms since 0001-01-01

    req = pb.PlayerProfileRequest(player_id="", ally_code=str(ally_code))

    env = pb.RequestEnvelope(
        service_name="PlayerRpc",
        method_name="GetPlayerArenaProfile",
        payload=req.SerializeToString(),
        client_version=181815,
        client_startup_timestamp=startup - 10,
        platform="Android",
        region="NA",
        client_external_version="99.99.99",
        client_internal_version="99.99.99",
        request_id=str(uuid.uuid4()),
        accept_encoding=pb.RequestEnvelope.GZIPACCEPTENCODING,
        current_client_time=startup + 8,
        network_access="W",
        application="my-swgoh-reader/1.0",
    )

    r = requests.post(URL, data=env.SerializeToString(),
                      headers={"Content-Type": "application/x-protobuf",
                               "Accept-Encoding": "gzip"}, timeout=30)
    r.raise_for_status()

    resp = pb.ResponseEnvelope()
    resp.ParseFromString(r.content)
    if resp.code != 1:   # 1 == OK
        raise RuntimeError(f"server error {resp.code}: {resp.message}")

    profile = pb.SlimPlayerArenaProfileResponse()
    profile.ParseFromString(gzip.decompress(resp.payload))

    fleet = next((p.rank for p in profile.pvp_profile
                  if p.tab == pb.PlayerPvpProfile.PROFILEPVPSHIP), None)
    squad = next((p.rank for p in profile.pvp_profile
                  if p.tab == pb.PlayerPvpProfile.PROFILEPVPCHARACTER), None)
    return dict(name=profile.name, level=profile.level,
                ally_code=str(profile.ally_code), player_id=profile.player_id,
                fleet_rank=fleet, squad_rank=squad,
                tz_offset_min=profile.local_timezone_offset_minutes)

def utc_payout_slot(tz_offset_min: int) -> str:
    m = (19 * 60 - tz_offset_min) % 1440
    return f"{m // 60:02d}:{m % 60:02d}"

if __name__ == "__main__":
    info = get_arena("116563768")
    print(info, "-> payout", utc_payout_slot(info["tz_offset_min"]), "UTC")
```

**Verified live** (2026-08-22): this exact request for ally code `116563768` returns

```
name='Wayfayer' level=85 ally_code='116563768'
player_id='HopG2w3VSPyqNMMQUJceYw' fleet_rank=4 squad_rank=208 tz_offset_min=-300
-> payout 00:00 UTC
```

> **Gotcha that cost us an hour:** `PlayerProfileRequest.ally_code` is a **string** on the wire, while `SlimPlayerArenaProfileResponse.ally_code` is an **int64**. Sending the request ally code as an integer produces a well-formed-looking envelope that the server rejects with `code 9 (INVALIDDATA): "PlayerId/ally code must be specified"` — because a mistyped field is silently treated as unknown and dropped.

Equivalent approaches work from any language with a protobuf runtime (Go, Rust via prost, C# with Google.Protobuf — see `src/Ipd.GameClient` for a working C# implementation).

---

## 6. Etiquette and caveats

- **This is an unofficial, unauthenticated API.** Capital Games can change or lock down it at any time; the spoofed client version (`99.99.99`) may eventually be rejected (error 50).
- **Be gentle:** one poll every 15+ seconds across your whole list is plenty for tracking. Hammering the endpoint risks `RATEEXCEEDED` responses for you and load for everyone.
- Read-only calls like this use no game account and cannot modify anything — but automated access is still technically outside the game's ToS; keep volumes polite.
- Ally codes are public identifiers by design (they're how friends find you); this API exposes only arena placement, level, and timezone offset — nothing sensitive.
