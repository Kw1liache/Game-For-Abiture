from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session

from app.core.database import get_db
from app.dependeсies.auth import get_current_user
from app.schemas.game import GameStatsCreate, GameStatsOut
from app.services.game_service import get_user_stats, set_user_stats


router = APIRouter(prefix="/game", tags=["game"])


@router.get("/me", response_model=GameStatsOut | None)
def get_my_stats(
    db: Session = Depends(get_db),
    current_user=Depends(get_current_user),
):
    return get_user_stats(db, current_user.id)


@router.post("/me", response_model=GameStatsOut)
def set_my_stats(
    data: GameStatsCreate,
    db: Session = Depends(get_db),
    current_user=Depends(get_current_user),
):
    return set_user_stats(db, current_user.id, data)