import numpy as np
import pandas as pd
from ingestion.config import DISTANCE_RESAMPLE_STEP_METERS


def synchronize_laps(
    df_a: pd.DataFrame,
    df_b: pd.DataFrame,
    driver_a: str,
    driver_b: str,
    tolerance_meters: float = DISTANCE_RESAMPLE_STEP_METERS,
) -> pd.DataFrame:
    """Synchronizes two telemetry traces onto a unified spatial distance grid.

    Uses merge_asof on the Distance axis with a nearest-match tolerance.
    """
    # Sort strictly by distance
    df_a = df_a.sort_values("Distance").reset_index(drop=True)
    df_b = df_b.sort_values("Distance").reset_index(drop=True)

    # Prefix columns to avoid collisions
    a_cols = {col: f"{col}_{driver_a}" for col in df_a.columns if col != "Distance"}
    b_cols = {col: f"{col}_{driver_b}" for col in df_b.columns if col != "Distance"}

    df_a = df_a.rename(columns=a_cols)
    df_b = df_b.rename(columns=b_cols)

    # Merge on spatial Distance using nearest interpolation
    synced = pd.merge_asof(
        df_a,
        df_b,
        on="Distance",
        direction="nearest",
        tolerance=tolerance_meters,
    )

    # Drop any edge rows where spatial alignment exceeded tolerance
    synced = synced.dropna().reset_index(drop=True)

    # Output schema: Distance, SpeedA, SpeedB, RpmA, RpmB, GearA, GearB, ThrottleA, ThrottleB, DrsA, DrsB
    column_mapping = {
        "Distance": "Distance",
        f"Speed_{driver_a}": "SpeedA",
        f"Speed_{driver_b}": "SpeedB",
        f"RPM_{driver_a}": "RpmA",
        f"RPM_{driver_b}": "RpmB",
        f"nGear_{driver_a}": "GearA",
        f"nGear_{driver_b}": "GearB",
        f"Throttle_{driver_a}": "ThrottleA",
        f"Throttle_{driver_b}": "ThrottleB",
        f"DRS_{driver_a}": "DrsA",
        f"DRS_{driver_b}": "DrsB",
    }

    synced = synced[list(column_mapping.keys())].rename(columns=column_mapping)
    
    synced["GearA"] = synced["GearA"].astype(int)
    synced["GearB"] = synced["GearB"].astype(int)
    synced["RpmA"] = synced["RpmA"].astype(int)
    synced["RpmB"] = synced["RpmB"].astype(int)
    synced["DrsA"] = synced["DrsA"].astype(int)
    synced["DrsB"] = synced["DrsB"].astype(int)
    
    return synced