import argparse
import logging
from pathlib import Path
from ingestion.config import (
    DATA_DIR,
    DEFAULT_DRIVER_A,
    DEFAULT_DRIVER_B,
    DEFAULT_GP,
    DEFAULT_SESSION,
    DEFAULT_YEAR,
)
from ingestion.fetcher import fetch_driver_fastest_lap
from ingestion.synchronizer import synchronize_laps


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Ingest and spatially synchronize F1 powertrain telemetry."
    )
    parser.add_argument("--year", type=int, default=DEFAULT_YEAR, help="Championship year")
    parser.add_argument("--gp", type=str, default=DEFAULT_GP, help="Grand Prix name")
    parser.add_argument("--session", type=str, default=DEFAULT_SESSION, help="Session (FP1, FP2, FP3, Q, SQ, R)")
    parser.add_argument("--driver-a", type=str, default=DEFAULT_DRIVER_A, help="Driver A code (e.g., RUS)")
    parser.add_argument("--driver-b", type=str, default=DEFAULT_DRIVER_B, help="Driver B code (e.g., ALO)")
    parser.add_argument("--output-dir", type=Path, default=DATA_DIR, help="Destination directory for CSV export")
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    args.output_dir.mkdir(parents=True, exist_ok=True)
    output_file = args.output_dir / "telemetry_sync.csv"

    logging.info(f"Extracting telemetry: {args.driver_a} vs {args.driver_b} ({args.gp} {args.year})")

    df_a = fetch_driver_fastest_lap(args.year, args.gp, args.session, args.driver_a)
    df_b = fetch_driver_fastest_lap(args.year, args.gp, args.session, args.driver_b)

    synced_df = synchronize_laps(df_a, df_b, args.driver_a, args.driver_b)

    synced_df.to_csv(output_file, index=False)
    logging.info(f"Successfully exported {len(synced_df)} synchronized spatial data points to {output_file}")


if __name__ == "__main__":
    main()