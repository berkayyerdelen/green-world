# GreenWorld

A small end-to-end system that **simulates electricity consumption and generation
in a neighbourhood** and visualizes what happens over time. Built on Clean
Architecture (.NET 10) with dependencies pointing inward only.

## Solution layout

- **GreenWorld.Domain** — entities (`Neighbourhood`, `Household`), value objects
  (`EnergyAmount`, `SimulationTick`), policy contracts, repository interfaces, domain exceptions.
- **GreenWorld.Application** — use cases (`SimulationService`), request/response DTOs, mapping.
- **GreenWorld.Infrastructure** — persistence, repository + policy implementations.
- **GreenWorld.Api** — HTTP endpoints, DI composition root (`AddGreenWorld`).
- **GreenWorld.SharedKernel** — base exceptions, shared config.
- **GreenWorld.Domain.Tests / GreenWorld.Application.Tests** — xUnit.

## Dependency rule

```
Api ──> Application ──> Domain <── Infrastructure
              └──────────> SharedKernel <──────────┘
```

## Run

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project GreenWorld.Api      # Swagger at /swagger
```

## Where the domain logic goes next

The simulation core lives behind two policy contracts in `GreenWorld.Domain`:
`IConsumptionPolicy` and `IGenerationPolicy`. `SimulationService` steps through
time and aggregates both across every household. Placeholder implementations
(`ConstantConsumptionPolicy`, `DaylightGenerationPolicy`) live in Infrastructure
and are swapped in via DI — replace them as the model grows.
