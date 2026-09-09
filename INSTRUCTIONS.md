# F1 Powertrain CLI

## Prerequisites

- Docker Desktop installed and running.
- A terminal opened at the repository root.

## Build the container

```bash
docker build -t f1-powertrain-cli .
```

The Docker build restores the .NET and Python dependencies, runs all xUnit tests, and publishes the self-contained CLI binary.

## Interactive REPL

```bash
docker run --rm -it \
  -v "$(pwd)/cache:/app/cache" \
  -v "$(pwd)/data:/app/data" \
  f1-powertrain-cli
```

At the `f1-pu>` prompt, use:

- `compare` to enter a year, Grand Prix, session, and two driver codes, then fetch and analyze telemetry.
- `analyze` to analyze an existing synchronized CSV.
- `exit` to close the REPL.

Press Enter at a prompt to use the displayed default. The standard comparison is 2026 Monza qualifying, with RUS as Driver A and PIA as Driver B.

## One-shot or scriptable execution

```bash
docker run --rm \
  -v "$(pwd)/cache:/app/cache" \
  -v "$(pwd)/data:/app/data" \
  f1-powertrain-cli --fetch --year 2026 --gp Monza --session Q --driver-a RUS --driver-b PIA
```

Use `--csv` to analyze a different synchronized CSV path. Without `--fetch`, the CLI analyzes the CSV already present at `/app/data/telemetry_sync.csv`.

## Persistent volumes

FastF1 stores downloaded session data under `cache/`. Mounting it at `/app/cache` keeps that cache on the host, so later comparisons can reuse telemetry instead of downloading it again. The `data/` mount persists the synchronized `telemetry_sync.csv` output and makes it available for analysis in later container runs.

## Output

The report includes the WOT gear operating envelope, the full-throttle acceleration delta summary, and a concise table of high-speed MGU-K deployment decay events. Low-speed throttle drops and repeated micro-deltas on one straight are filtered out.
