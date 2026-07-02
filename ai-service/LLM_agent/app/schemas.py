from typing import Any, Dict, List, Optional
from pydantic import BaseModel


class AskNpcRequest(BaseModel):
    npc_id: str
    question: str


class SourceItem(BaseModel):
    title: Optional[str] = None
    url: Optional[str] = None


class AskNpcResponse(BaseModel):
    answer: str
    sources: List[SourceItem]


class GameAskRequest(BaseModel):
    npc_id: str
    question: str
    player_id: Optional[str] = None
    session_id: Optional[str] = None
    metadata: Optional[Dict[str, Any]] = None


class GameAskResponse(BaseModel):
    npc_id: str
    answer: str
    sources: List[SourceItem]
    