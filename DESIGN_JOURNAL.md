# GreenWorld — Design Journal

A record of the decisions behind the build, framed as the questions that came up
and the answers we landed on, mapped to the commits that implemented them. It
reads as the "why" companion to the README's "what".

---

## 1. How should the project be structured?

**Q — What layout and framework?**
Clean Architecture on .NET 10, one project per layer (`Domain`, `Application`,
`Infrastructure`, `Api`, `SharedKernel`) plus a test project per layer that holds
logic. Dependencies point inward only: `Api → Application → Domain ← Infrastructure`,
everything → `SharedKernel`.

**Q — Why Clean Architecture for a small simulation?**
The brief weighs "clear domain modelling, correctness, extensibility." Keeping the
domain (entities, physics, invariants) free of framework/DB/HTTP concerns makes the
simulation logic testable in isolation and lets the moving parts (persistence,
messaging, transport) be swapped without touching the core.

> `72bb299` chore: initialize GreenWorld Clean Architecture solution (.NET 10)

---

## 2. How do we model the neighbourhood and its energy?

**Q — What is the aggregate shape?**
`Neighbourhood` (aggregate root) → `MeteredSite` (abstract) → `Asset`. A site is
either a `Household` or a `PublicFacility` (table-per-hierarchy). Each owns a list
of assets, and a house may hold several (PV + heat pump + EV).

**Q — What is a "meter"?**
Every `Asset` *is* a meter: it carries its own cumulative energy (kWh) since the
simulation start, split into consumed and generated.

**Q — How do readings update state?**
Event-sourced. A `MeterReadingEvent` is the append-only source of truth; the asset's
cumulative fields are a fast O(1) projection updated by `Asset.ApplyReading(...)`.
A `NeighbourhoodAggregateSnapshot` is written per tick for aggregate power/energy
over time.

**Q — Step size?**
1 hour (24 ticks/day). Compact history, aligns with hourly weather/occupancy
profiles, still shows morning/evening peaks and the midday PV curve. Energy per
tick = `power(kW) × 1h`. Configurable via `StepMinutes`; wall-clock pacing is
separate so it can run faster than real time.

> `c29110a` feat: event-sourced neighbourhood energy simulation

---

## 3. Where does the reading data come from? (messaging)

**Q — How do meters deliver readings?**
"Imagine meters publish to RabbitMQ and a consumer ingests them." So a background
`MeterSimulatorService` computes each asset's weather-driven reading per tick and
**publishes it to RabbitMQ**; a consumer appends the event and updates the
projection. Real meters could replace the simulator with no other change.

**Q — Raw client or a library?**
Started with `RabbitMQ.Client`, then switched to **MassTransit** for cleaner
topology, per-message DI scopes, and consumer ergonomics.

**Q — Store events or also project?**
Both: append the raw event (truth) *and* maintain the cumulative projection (speed).

> `c29110a` (initial pipeline) → MassTransit adopted mid-build

---

## 4. How is it persisted?

**Q — Which database?**
EF Core + PostgreSQL. TPH for sites, an `assets` table with the projection columns,
an append-only `meter_readings` event store, and `neighbourhood_aggregate_snapshots`
for the time series.

**Q — Real-time vs historical reads?**
Real-time from the projections; historical from the snapshot series and the raw
event stream.

> `5845d67` feat: EF migrations + live energy dashboard

---

## 5. How is the neighbourhood configured?

**Q — Code, file, or seed?**
All three: a fixed **seed** + stated **proportions** in `NeighbourhoodConfiguration`
(code defaults) overlaid by an optional `neighbourhood.json`. Guarantees exactly
**30 households** and **6 public facilities**; assets assigned by seeded draws
(40% PV, 30% heat pump, 20% home EV). Same seed ⇒ identical neighbourhood.

> `72bb299` / `c29110a`

---

## 6. Weather, season, and the physics

**Q — Real weather API?**
No — deterministic and privacy-friendly. `SeasonalWeatherModel` is a pure function
of `(seed, timestamp)`: seasonal mean temperature + diurnal sine, per-day cloud
cover, and a clear-sky irradiance curve (daylight window widening in summer).

**Q — What must weather influence?**
PV generation (irradiance × capacity) and heat-pump consumption (temperature-driven,
COP degrading in the cold; heating only, no cooling).

**Q — Public EV charger usage model?**
An hourly **occupancy fraction** (share of the hour a vehicle draws power), peaking
midday and evening, with weekday/weekend and deterministic jitter — documented in
the README.

**Q — PV netting assumption?**
Netted at the neighbourhood aggregate: PV first offsets concurrent load
(self-consumption); surplus exports, shortfall imports. No inter-house trading or
storage.

> `c29110a`

---

## 7. How do we run and observe it?

**Q — One-command startup?**
`docker compose up --build` runs the whole stack: API + PostgreSQL + RabbitMQ, wired
by service name, waiting on healthchecks.

**Q — Animated UI?**
A self-contained dashboard (`wwwroot/index.html`, Chart.js) polling every 2s: current
simulated time, season + weather, live consumption/generation/net power, the
aggregate chart, and per-meter cumulative kWh.

> `5845d67` feat: EF migrations + live energy dashboard
> `666ea39` feat: containerise the API (full-stack docker-compose)

---

## 8. Runtime issues we hit (and fixed)

**Q — Swagger 404 in the container?**
Swagger was Development-only; enabled it in all environments and moved the host port
to 8088.
> `6d852b5`

**Q — `TypeLoadException: GetSwagger ... does not have an implementation`?**
Swashbuckle 6.6.2 predates .NET 10 → bumped to 7.2.0. The deeper cause was a
`Microsoft.OpenApi` 2.x vs 1.6.x clash pulled in by `Microsoft.AspNetCore.OpenApi`;
removed that package since we use Swashbuckle.
> `03e4609`, `5ab3bb8`

**Q — `PendingModelChangesWarning` on `Migrate()`?**
EF 10 hard-fails when the hand-authored snapshot doesn't byte-match the model.
Suppressed that specific warning so the migration applies; documented regenerating
with `dotnet ef` as the clean fix.
> `7de5d79`

**Q — `column "CloudCover" does not exist`?**
Editing the existing migration doesn't re-run it on an existing DB (already in
`__EFMigrationsHistory`). Fix: `docker compose down -v` to recreate the volume.
> (operational, after `c82e13d`)

---

## 9. Closing the gaps against the brief

**Q — Did we meet every requirement 3 (UI) item?**
Weather/season were missing from the UI. Added them to the aggregate snapshot,
`/aggregate` response, and dashboard (season card + temp/cloud/irradiance).

**Q — Is the clock "controllable"?**
Added `ISimulationControl` read by the simulator loop and
`/api/simulation/pause|resume|speed|status`, with buttons in the dashboard.

**Q — Does the chart show "last 24 simulated hours"?**
Default is now a rolling last-24-simulated-hours window with a 24h / 7d / All toggle
(x-axis uses simulated timestamps, so it holds even when the sim runs fast).

**Q — Limitations documented?**
Added a "Known limitations & next steps" section (aggregate-only PV netting, no
cooling, no destructive reset, hand-authored migration, at-least-once delivery,
not verified end-to-end in a build environment).

> `c82e13d` feat: weather/season in UI + state, runtime clock controls, limitations docs
> `864e301` feat(ui): rolling chart window toggle (Last 24h / 7d / All)

---

## Commit history at a glance

| Commit | Summary |
|---|---|
| `72bb299` | Initialize Clean Architecture solution (.NET 10) |
| `c29110a` | Event-sourced neighbourhood energy simulation |
| `5845d67` | EF migrations + live energy dashboard |
| `666ea39` | Containerise the API (full-stack docker-compose) |
| `6d852b5` | Enable Swagger everywhere; API on host port 8088 |
| `03e4609` | Bump Swashbuckle to 7.2.0 for .NET 10 |
| `5ab3bb8` | Drop Microsoft.AspNetCore.OpenApi (OpenApi 2.x clash) |
| `7de5d79` | Suppress EF 10 PendingModelChangesWarning |
| `c82e13d` | Weather/season in UI + state, runtime clock controls, limitations docs |
| `864e301` | Rolling chart window toggle (Last 24h / 7d / All) |

*(Reverse-chronological in `git log`; oldest → newest above.)*
