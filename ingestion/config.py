from pathlib import Path

# Project root paths
BASE_DIR = Path(__file__).resolve().parent.parent
CACHE_DIR = BASE_DIR / "cache"
DATA_DIR = BASE_DIR / "data"

# Default session configuration
DEFAULT_YEAR = 2026
DEFAULT_GP = "Monza"
DEFAULT_SESSION = "Q"
DEFAULT_DRIVER_A = "RUS"
DEFAULT_DRIVER_B = "ALO"

# Spatial resampling configuration
DISTANCE_RESAMPLE_STEP_METERS = 5.0