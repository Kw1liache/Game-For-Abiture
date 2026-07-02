from fastapi import APIRouter, Depends

from app.dependeсies.auth import get_current_user


router = APIRouter(prefix="/users", tags=["users"])


@router.get("/me")
def read_me(current_user = Depends(get_current_user)):
    return {
        "id": current_user.id,
        "username": current_user.username,
        "is_active": current_user.is_active,
    }