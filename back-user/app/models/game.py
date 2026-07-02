from sqlalchemy import Integer, String, ForeignKey
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.core.database import Base
from app.models.user import User


class GameStats(Base):
    __tablename__ = "game_stats"

    id: Mapped[int] = mapped_column(primary_key=True, index=True)
    user_id: Mapped[int] = mapped_column(ForeignKey("users.id"), nullable=False, index=True)

    time_played_minutes: Mapped[int] = mapped_column(Integer, default=0)
    character_gender: Mapped[str] = mapped_column(String(10), default="unknown")

    user: Mapped[User] = relationship("User", backref="game_stats")