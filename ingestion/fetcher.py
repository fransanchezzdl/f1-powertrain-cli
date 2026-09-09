import logging
from pathlib import Path
import fastf1
import pandas as pd
from ingestion.config import CACHE_DIR

logging.basicConfig(level=logging.INFO, format="[%(levelname)s] %(message)s")


def configure_cache(cache_path: Path = CACHE_DIR) -> None:
    """Enables FastF1 disk caching to avoid redundant API downloads."""
    cache_path.mkdir(parents=True, exist_ok=True)
    fastf1.Cache.enable_cache(str(cache_path))


def fetch_driver_fastest_lap(
    year: int, gp: str, session_type: str, driver: str
) -> pd.DataFrame:
    """Loads session data and extracts telemetry for the driver's fastest lap.

    Returns a normalized DataFrame containing physical channels:
    Distance, Speed, RPM, nGear, Throttle, and DRS.
    """
    configure_cache()

    logging.info(f"Loading session: {year} {gp} [{session_type}]...")
    session = fastf1.get_session(year, gp, session_type)
    session.load(telemetry=True, laps=True, weather=False)

    laps = session.laps.pick_driver(driver)
    if laps.empty:
        raise ValueError(f"Driver '{driver}' not found in {year} {gp} {session_type}.")

    fastest_lap = laps.pick_fastest()
    if fastest_lap is None or fastest_lap.empty:
        raise ValueError(f"No valid lap times recorded for driver '{driver}'.")

    logging.info(
        f"Driver {driver} fastest lap: {fastest_lap['LapTime']} (Lap {int(fastest_lap['LapNumber'])})"
    )

    telemetry = fastest_lap.get_telemetry()
    
    # Select and standardize required powertrain channels
    channels = ["Distance", "Speed", "RPM", "nGear", "Throttle", "DRS"]
    df = telemetry[channels].copy()

    # Convert types explicitly to ensure clean serialisation
    df["Distance"] = df["Distance"].astype(float)
    df["Speed"] = df["Speed"].astype(float)
    df["RPM"] = df["RPM"].astype(int)
    df["nGear"] = df["nGear"].astype(int)
    df["Throttle"] = df["Throttle"].astype(float)
    df["DRS"] = df["DRS"].astype(int)

    return df