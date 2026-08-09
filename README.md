# GreenWorld

An end-to-end system that **simulates electricity consumption and generation in a
neighbourhood** and makes it observable over time. Meters emit readings that flow
through **RabbitMQ (via MassTransit)**; a consumer appends each reading to an
**event store** and folds it into per-asset **cumulative projections** in
**PostgreSQL (EF Core)**. Built on Clean Architecture (.NET 10).

You can answer, at any time:

- What is the simulated **date/time** and **weather/season**?
- What is the **current and historical neighbourhood load** (aggregate power/energy)?
- What is the **cumulative energy** (kWh) per asset/meter since simulation start?

## Domain model

```
Neighbourhood (aggregate root)
├── MeteredSite            (abstract, table-per-hierarchy)
│   ├── Household          (base load + optional PV / heat pump / home EV charger)
│   └── PublicFacility     (holds a public EV charger)   × 6
│        └── Asset[]       (each asset is its own meter)
└── NeighbourhoodAggregateSnapshot[]   (aggregate power/energy over time)
```

- A **Household** or **PublicFacility** owns a list of **Assets**. A house may
  hold several (e.g. PV + heat pump).
- Every **Asset** tracks **cumulative energy since simulation start** (kWh),
  split into consumed and generated, updated by folding in meter readings.
- **MeterReadingEvent** is the append-only source of truth (the event stream);
  asset cumulative fields are the fast read projection.

## Event-sourced pipeline

```
MeterSimulatorService ──publish──▶ RabbitMQ ──▶ MeterReadingConsumer
   (weather-driven readings)      (MassTransit)        │
        │                                              ▼
        │ writes                         MeterReadingIngestionService
        ▼                                   ├─ append MeterReadingEvent (event store)
  AggregateSnapshot (per tick)              └─ Asset.ApplyReading(...)  (projection)
        │                                              │
        └──────────────── PostgreSQL (EF Core) ◀───────┘
```

- **`MeterSimulatorService`** (hosted service) is the meter farm: each tick it
  computes every asset's reading from weather + time and **publishes it to
  RabbitMQ**, exactly as physical meters would. It also records a neighbourhood
  aggregate snapshot per tick.
- **`MeterReadingConsumer`** (MassTransit) ingests each reading in its own scope:
  appends the immutable event and updates the asset's cumulative projection in
  one unit of work.
- **Real-time reads** come from the O(1) projections; **historical reads** come
  from the aggregate snapshot series and the raw event store.

## Solution layout

```
GreenWorld.Domain          # Entities, events, MeterReadingCalculator (physics), repo contracts
GreenWorld.Application      # Ingestion + query services, message + response DTOs, ports
GreenWorld.Infrastructure   # EF Core (Postgres), MassTransit/RabbitMQ, seeder, simulator
GreenWorld.Api             # Controllers + composition root
GreenWorld.SharedKernel     # NeighbourhoodConfiguration, base exceptions
GreenWorld.*.Tests          # xUnit
```

Dependency rule: `Api → Application → Domain ← Infrastructure`, all → `SharedKernel`.
(Infrastructure also references Application to implement its ports — the messaging
publisher/consumer and ingestion.)

## Run

```bash
docker compose up -d           # PostgreSQL + RabbitMQ (management UI on :15672)
dotnet run --project GreenWorld.Api
```

On startup the API ensures the schema and **seeds the neighbourhood once**
(30 households + 6 public facilities). The simulator then begins publishing
readings; watch the meters and aggregate move. Swagger UI is at `/swagger`.

### API

| Method & route | Purpose |
|---|---|
| `GET  /api/neighbourhood` | Structure: sites + assets with cumulative energy |
| `GET  /api/neighbourhood/meters` | Cumulative kWh per asset/meter since start |
| `GET  /api/neighbourhood/aggregate` | Latest aggregate power + cumulative energy (real time) |
| `GET  /api/neighbourhood/aggregate/history?from&to&last=N` | Aggregate power/energy over time |
| `GET  /api/neighbourhood/assets/{id}/history` | Raw reading stream for one asset |
| `POST /api/meterreadings` | Publish a reading onto the same pipeline (testing) |

## Design decisions & assumptions

### Step size — 1 hour
The simulated clock advances in whole 1-hour steps (24 ticks/day). It keeps a
year of history compact, aligns with hourly weather/occupancy profiles, and is
enough to see morning/evening load peaks and the midday PV curve. Energy per tick
is `power(kW) × 1h = kWh`. Configurable via `StepMinutes`. Wall-clock pacing is
separate (`Simulator:StepDelayMs`, default 1s per tick) so you can run fast or slow.

### Assets & physics (extensible)
`MeterReadingCalculator` (a pure domain service) turns an asset + weather/time
into a reading, switched on `AssetKind`. Adding a new asset type is a new enum
value + a case — assets themselves stay simple persisted data.

- **BaseLoad** — always present; residential daily curve, per-house scale, mild
  winter uplift.
- **HeatPump** — temperature-driven; load rises as outdoor temp falls below a
  20 °C setpoint, COP degrades in the cold. **No active cooling** (summer ≈ 0).
- **Pv** — generation = installed kWp × irradiance factor.
- **HomeEvCharger** — overnight charging from ~22:00 for the hours needed to meet
  a seeded daily energy need (probabilistic per day).
- **PublicEvCharger** — occupancy-fraction model (below).

### Weather & season (deterministic, no external APIs)
`SeasonalWeatherModel` is a pure function of `(seed, timestamp)`, fully
reproducible. Season is meteorological (by month). It yields **temperature**
(seasonal mean + per-day offset + diurnal sine) driving the heat pump, and
**cloud cover** + a clear-sky **irradiance factor** (daylight window widening in
summer, narrowing in winter) driving PV.

### Public EV charger usage model
Each of the 6 public chargers uses an **occupancy fraction** per hour — the share
of the hour a vehicle is drawing power — representing mixed resident + passer-by
use, peaking around midday and the evening return, with a weekday/weekend factor
and deterministic jitter. Delivered power = `maxPower × occupancy` (default 22 kW).

### Energy accounting & PV netting
Each asset accrues its own cumulative kWh (consumed or generated). Neighbourhood
aggregate snapshots record instantaneous power and running cumulative energy per
tick. Grid interaction is netted at the **neighbourhood aggregate** level: PV
first offsets concurrent load (self-consumption); surplus is export, shortfall is
import. Inter-house trading and storage are not modelled — surplus/deficit is
aggregated across the neighbourhood, enough to show net import/export across a day
and the seasons.

## Configuration (code + file)
The neighbourhood is fully determined by `NeighbourhoodConfiguration`: a fixed
**seed** plus stated **proportions**. `NeighbourhoodConfigurationLoader` starts
from code defaults and overlays `Infrastructure/Configuration/neighbourhood.json`
if present (*code default + optional JSON override*). Connection string, RabbitMQ
and simulator pacing live in `appsettings.json`.

Defaults (guaranteed **exactly 30 households** and **exactly 6 public facilities**):

| Setting | Value |
|---|---|
| Households | 30 |
| Public facilities (chargers) | 6 |
| Households with **PV** | 40% |
| Households with **heat pump** | 30% |
| Households with **home EV charger** | 20% |
| Seed | 42 |
| Start | 2025-01-01T00:00Z |
| Step | 60 min |

Assets are assigned by independent seeded draws against the shares, so a given
seed + config always builds the identical neighbourhood (identities included).

## Persistence notes
Startup uses `EnsureCreated` for zero-friction bring-up. For production, add EF
migrations (`dotnet ef migrations add Initial -p GreenWorld.Infrastructure -s
GreenWorld.Api`) and switch the initializer to `Database.Migrate()`.

## Tests
Domain tests cover the physics (PV ∝ irradiance and zero at night, heat pump rises
as it gets colder, energy = power × step) and the asset projection
(`ApplyReading` accrual + ownership guard). Application tests cover ingestion
(event appended + projection updated) with in-memory fakes — no infra required.
