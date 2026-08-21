# ParadiseRP Architecture

## Objective

ParadiseRP is built as a game system around Nitro, not inside Nitro.

Nitro remains responsible for Habbo rendering, room lifecycle, native packets, chat and the room engine. ParadiseRP owns RP state, gameplay services and UI synchronization.

## Protected Nitro boundary

Do not modify Nitro WebSocket framing, packet parsing, renderer, canvas lifecycle or room lifecycle unless a regression proves it is unavoidable.

```text
Nitro
├── Room Engine
├── Renderer
├── Network
└── Game Events
      │
      ▼
ParadiseBridge
      │
      ▼
ParadiseStore
      │
      ▼
Paradise UI
```

## Current authoritative sources

- `users`: account identity, Habbo username/look/rank, credits and base account state.
- `play_stats`: RP health, armor, progression, bank, status flags and RP statistics.
- `groups` / `group_memberships` / `play_jobs_ranks`: jobs, businesses, gangs and grades.
- `play_products` / `play_products_owned`: legacy RP item definitions and owned inventory.
- `play_phones*`: phone models, owned phones, apps and phone numbers.
- `play_phone_chats`: persistent phone messages.
- `play_vehicles*`: vehicle definitions and owned vehicle state.
- `play_apartments*` / `play_houses`: property state.

Do not duplicate these values in new ParadiseRP tables.

## ParadiseBridge

The existing roleplay WebEvent channel is the transport foundation for ParadiseBridge. The new protocol must progressively move away from comma-delimited strings and server-rendered HTML toward versioned JSON messages.

Envelope:

```json
{
  "v": 1,
  "event": "player:snapshot",
  "data": {}
}
```

Initial event families:

```text
player:snapshot
player:vitals
player:identity
player:employment
economy:update
room:update
inventory:update
phone:update
vehicle:update
property:update
notification:new
```

The server is authoritative. The client may request an action but never decides the result of money, inventory, vehicles, health, jobs, police or property mutations.

## ParadiseStore

One client-side source of truth:

```text
ParadiseStore
├── player
├── identity
├── vitals
├── progression
├── economy
├── employment
├── inventory
├── phone
├── vehicles
├── properties
├── notifications
├── room
└── ui
```

Legacy `rp-hud-data.php` polling is retained only as a migration fallback until server-pushed snapshots are proven stable.

## Module shape

Each new ParadiseRP domain should converge toward:

```text
Module/
├── Model
├── DTO
├── Repository
├── Service
└── Events
```

Avoid adding new responsibilities to `RoleplayUser`, `GameClient` or `WebEventManager` when a domain service is appropriate.

## Character System V2

Character V2 does not replace the Habbo account. It adds RP identity fields that do not already exist.

Existing values remain authoritative:

```text
users.id               -> account/user id
users.username         -> Habbo username
users.look             -> avatar look
users.rank             -> staff role
users.credits          -> cash
play_stats.curhealth   -> health
play_stats.maxhealth   -> max health
play_stats.armor       -> armor
play_stats.level       -> RP level
play_stats.curxp       -> RP XP
play_stats.needxp      -> XP target
play_stats.bank        -> bank balance
```

The new character table stores only missing RP identity information such as citizen number, RP first/last name, birth date, gender and nationality.

## Non-regression gate

Every ParadiseRP feature must re-test:

```text
login
room load
room change
native chat
Paradise chat adapter
phone window
inventory window
Paradise UI/window manager
Nitro WebSocket
Paradise WebEvent socket
```

No feature is merged if one of these regresses.
