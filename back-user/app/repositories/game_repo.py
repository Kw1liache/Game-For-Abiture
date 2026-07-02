from sqlalchemy.orm import Session

from app.models.game import GameStats


def get_stats_by_user_id(db: Session, user_id: int) -> GameStats | None:
    return db.query(GameStats).filter(GameStats.user_id == user_id).first()


def create_or_update_stats(
    db: Session,
    user_id: int,
    time_played_minutes: int,
    character_gender: str,
) -> GameStats:
    stats = get_stats_by_user_id(db, user_id)
    if stats is None:
        stats = GameStats(
            user_id=user_id,
            time_played_minutes=time_played_minutes,
            character_gender=character_gender,
        )
        db.add(stats)
    else:
        stats.time_played_minutes = time_played_minutes
        stats.character_gender = character_gender

    db.commit()
    db.refresh(stats)
    return stats