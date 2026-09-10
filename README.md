# F1 Powertrain CLI

```text
███████╗ ██╗
██╔════╝███║
█████╗  ╚██║
██╔══╝   ██║
██║      ██║
╚═╝      ╚═╝ POWERTRAIN PERFORMANCE ANALYZER
```

A telemetry comparison CLI for investigating Formula 1 power-unit performance. 

*Developed by Francisco Sánchez de León Acevedo*

>See [INSTRUCTIONS.md](INSTRUCTIONS.md) for Docker build, interactive REPL, and scriptable execution commands.

## Architecture

- **Ingestion:** Python, FastF1, pandas, and NumPy fetch the selected session, extract each driver's fastest-lap channels, and spatially synchronize them into `data/telemetry_sync.csv`.
- **Analytical engine:** C#/.NET 8 analyzers calculate the operating envelope, WOT speed deltas, and high-speed MGU-K clipping candidates.
- **Presentation:** The CLI renders aligned terminal tables and supports both one-shot flags and an interactive `f1-pu>` REPL.
- **Verification:** xUnit tests use synthetic telemetry to validate the domain algorithms during the Docker build.

## Powertrain analysis

- **WOT comparison:** Samples at throttle of at least 98% are compared spatially. Mean and maximum speed deltas show acceleration performance under sustained driver demand.
- **Gear envelopes:** For each gear, the engine reports maximum speed and average RPM while the car is at WOT, exposing shift and deployment differences.
- **MGU-K clipping:** A candidate event is a sustained full-throttle speed decay of at least 3.5 km/h after the car has reached 285 km/h. One event is recorded per straight-line throttle stretch, filtering low-speed noise and repeated micro-deltas.

## Project layout

```text
ingestion/                  FastF1 fetch and spatial synchronization
src/PowertrainCli/          .NET domain analyzers and terminal UI
tests/PowertrainCli.Tests/  xUnit behavioral tests
cache/                      Persistent FastF1 session cache
data/                       Synchronized telemetry output
```

## License and data

This tool is intended for analysis of telemetry available through FastF1. Follow the data provider's terms and rate limits when collecting session data.
