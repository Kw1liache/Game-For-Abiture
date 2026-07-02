from fastapi import FastAPI

from app.api.v1.routers import auth as auth_router
from app.api.v1.routers import users as users_router
from app.api.v1.routers import game as game_router

from app.core.database import Base, engine
from app.models.user import User

app = FastAPI(title="user-service")

Base.metadata.create_all(bind=engine)

app.include_router(auth_router.router, prefix="/api/v1")
app.include_router(users_router.router, prefix="/api/v1")
app.include_router(game_router.router, prefix="/api/v1")