# F1 Powertrain CLI (`f1-powertrain-cli`)

A modular telemetry pipeline and command-line engine for Formula 1 power unit performance analysis. It synchronizes trackside telemetry data and calculates wide-open-throttle (WOT) combustion acceleration deltas, shift RPM envelopes, and MGU-K energy clipping decay across modern hybrid power units.

## Architecture
- **Ingestion Layer:** Python (`fastf1`, `pandas`, `numpy`) for telemetry fetching, filtering, and spatial distance alignment.
- **Analytical Engine:** C# (.NET 8) modular domain analyzer implementing isolated analytical passes via `IPowertrainAnalyzer`.
- **Verification:** Automated unit test suites (`xUnit`) using synthetic test telemetry.
