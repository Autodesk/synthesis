from pathlib import Path

from src import ADDIN_PATH

# APS configuration. These default to production; internal developers can override any of
# them with a local `.env` file next to Synthesis.py (see .env.template).
_DEFAULTS = {
    "SYNTHESIS_APS_CLIENT_ID": "GCxaewcLjsYlK8ud7Ka9AKf9dPwMR3e4GlybyfhAK2zvl3tU",
    "SYNTHESIS_APS_CALLBACK_URL": "https://synthesis.autodesk.com/api/aps/exporter/",
    "SYNTHESIS_APS_AUTHENTICATION_BASE_URL": "https://developer.api.autodesk.com",
    "SYNTHESIS_APS_DATA_MANAGEMENT_BASE_URL": "https://developer.api.autodesk.com",
    "SYNTHESIS_APS_USERINFO_BASE_URL": "https://api.aps.autodesk.com",
}


def _loadDotEnv(path: Path) -> dict[str, str]:
    values: dict[str, str] = {}
    if not path.is_file():
        return values
    for line in path.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, _, value = line.partition("=")
        values[key.strip()] = value.strip().strip("\"'")
    return values


_DOT_ENV = _loadDotEnv(Path(ADDIN_PATH).parent / ".env")


def _get(name: str) -> str:
    return _DOT_ENV.get(name) or _DEFAULTS[name]


CLIENT_ID = _get("SYNTHESIS_APS_CLIENT_ID")
CALLBACK_URL = _get("SYNTHESIS_APS_CALLBACK_URL")
AUTHENTICATION_BASE_URL = _get("SYNTHESIS_APS_AUTHENTICATION_BASE_URL").rstrip("/")
DATA_MANAGEMENT_BASE_URL = _get("SYNTHESIS_APS_DATA_MANAGEMENT_BASE_URL").rstrip("/")
USERINFO_BASE_URL = _get("SYNTHESIS_APS_USERINFO_BASE_URL").rstrip("/")
APS_SCOPES = "data:create data:write data:search data:read openid profapi:core-std-profile:read profapi:img-profile:read"
