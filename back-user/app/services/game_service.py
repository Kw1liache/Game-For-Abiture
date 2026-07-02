from sqlalchemy.orm import Session

from app.repositories.game_repo import get_stats_by_user_id, create_or_update_stats
from app.schemas.game import GameStatsCreate, GameStatsOut


def get_user_stats(db: Session, user_id: int) -> GameStatsOut | None:
    stats = get_stats_by_user_id(db, user_id)
    if stats is None:
        return None
    return GameStatsOut.model_validate(stats)


def set_user_stats(db: Session, user_id: int, data: GameStatsCreate) -> GameStatsOut:
    stats = create_or_update_stats(
        db=db,
        user_id=user_id,
        time_played_minutes=data.time_played_minutes,
        character_gender=data.character_gender,
    )
    return GameStatsOut.model_validate(stats)