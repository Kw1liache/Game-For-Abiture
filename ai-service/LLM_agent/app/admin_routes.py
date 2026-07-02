import threading
import time
from dataclasses import dataclass
from typing import Optional

from fastapi import APIRouter, Header, HTTPException, status

from app.config import settings

router = APIRouter(prefix="/admin", tags=["admin"])


@dataclass
class ReindexStatus:
    running: bool = False
    started_at: Optional[float] = None
    finished_at: Optional[float] = None
    last_error: Optional[str] = None


_lock = threading.Lock()
_state = ReindexStatus()
_thread: Optional[threading.Thread] = None


def _require_admin_token(x_admin_token: Optional[str]) -> None:
    if not settings.admin_token:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="ADMIN_TOKEN is not configured",
        )
    if not x_admin_token or x_admin_token != settings.admin_token:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Unauthorized")


def _run_reindex() -> None:
    global _state
    try:
        from scripts.reindex_site import main as reindex_main

        reindex_main()
        with _lock:
            _state.running = False
            _state.finished_at = time.time()
            _state.last_error = None
    except Exception as e:
        with _lock:
            _state.running = False
            _state.finished_at = time.time()
            _state.last_error = f"{type(e).__name__}: {e}"


@router.post("/reindex")
def start_reindex(x_admin_token: Optional[str] = Header(default=None, alias="X-Admin-Token")):
    global _thread, _state
    _require_admin_token(x_admin_token)

    with _lock:
        if _state.running:
            return {
                "status": "already_running",
                "started_at": _state.started_at,
            }

        _state.running = True
        _state.started_at = time.time()
        _state.finished_at = None
        _state.last_error = None

        _thread = threading.Thread(target=_run_reindex, name="reindex_site", daemon=True)
        _thread.start()

    return {"status": "started", "started_at": _state.started_at}


@router.get("/reindex/status")
def reindex_status(x_admin_token: Optional[str] = Header(default=None, alias="X-Admin-Token")):
    _require_admin_token(x_admin_token)

    with _lock:
        return {
            "running": _state.running,
            "started_at": _state.started_at,
            "finished_at": _state.finished_at,
            "last_error": _state.last_error,
        }

