from pathlib import Path
import subprocess, sys

ROOT = Path(__file__).resolve().parents[2]
raise SystemExit(subprocess.call([str(ROOT / "run_regression.bat")], shell=True))
