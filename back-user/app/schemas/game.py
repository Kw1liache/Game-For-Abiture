from pydantic import BaseModel


class GameStatsBase(BaseModel):
    time_played_minutes: int
    character_gender: str


class GameStatsCreate(GameStatsBase):
    pass


class GameStatsUpdate(GameStatsBase):
    pass


class GameStatsOut(GameStatsBase):
    id: int
    user_id: int

    class Config:
        from_attributes = True