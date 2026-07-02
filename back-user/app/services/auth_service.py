from sqlalchemy.orm import Session
from fastapi import HTTPException, status

from app.core.security import hash_password, verify_password, create_access_token
from app.repositories.user_repo import get_user_by_username, create_user
from app.schemas.auth import UserCreate


def register_user(db: Session, data: UserCreate):
    existing = get_user_by_username(db, data.username)
    if existing:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Username already exists",
        )
    hashed = hash_password(data.password)
    user = create_user(db, username=data.username, hashed_password=hashed)
    return user


def authenticate_user(db: Session, username: str, password: str):
    user = get_user_by_username(db, username)
    if not user:
        return None
    if not verify_password(password, user.hashed_password):
        return None
    return user


def login_user(user):
    token = create_access_token(subject=user.username)
    return token