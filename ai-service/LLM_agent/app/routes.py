from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from app.db import get_db
from app.schemas import AskNpcRequest, AskNpcResponse, GameAskRequest, GameAskResponse, SourceItem
from app.npc_config import NPCS
from app.retriever import retrieve_chunks, build_context
from app.gigachat_client import GigaChatClient

router = APIRouter()
gigachat = GigaChatClient()


def _generate_answer_for_npc(npc_id: str, question: str, db: Session) -> tuple[str, list[SourceItem]]:
    npc = NPCS.get(npc_id)
    if not npc:
        raise HTTPException(status_code=404, detail="NPC not found")

    chunks = retrieve_chunks(
        db=db,
        question=question,
        allowed_topics=npc["allowed_topics"],
    )

    if not chunks:
        return (
            "Я не нашёл подтвержденной информации по этому вопросу на официальных страницах УрФУ.",
            [],
        )

    context = build_context(chunks)

    system_prompt = (
        f"Ты NPC в игре UrFU: Pixel Campus.\n"
        f"Твоя роль: {npc['name']}.\n"
        f"{npc['style']}\n"
        f"Отвечай только на основе предоставленного контекста из официальных источников УрФУ.\n"
        f"Если в контексте нет точного ответа, прямо скажи: "
        f"'Я не могу точно ответить по официальным данным из текущего контекста'.\n"
        f"Не выдумывай факты, не обобщай и не добавляй информацию от себя.\n"
        f"Если возможно, цитируй формулировки из контекста.\n"
        f"Если вопрос зависит от конкретного направления подготовки, обязательно укажи это.\n"
        f"В конце ответа кратко скажи: 'Информация основана на официальных источниках УрФУ.'"
    )

    user_prompt = (
        f"Контекст:\n{context}\n\n"
        f"Вопрос игрока:\n{question}"
    )
    answer = gigachat.chat(system_prompt=system_prompt, user_prompt=user_prompt)

    sources = []
    seen = set()
    for ch in chunks:
        key = (ch.source_title, ch.source_url)
        if key not in seen:
            seen.add(key)
            sources.append(SourceItem(title=ch.source_title, url=ch.source_url))

    return answer, sources


@router.post("/debug/search")
def debug_search(payload: AskNpcRequest, db: Session = Depends(get_db)):
    npc = NPCS.get(payload.npc_id)
    if not npc:
        raise HTTPException(status_code=404, detail="NPC not found")

    chunks = retrieve_chunks(
        db=db,
        question=payload.question,
        allowed_topics=npc["allowed_topics"],
    )

    return {
        "npc": npc["name"],
        "question": payload.question,
        "results": [
            {
                "topic": ch.topic,
                "title": ch.source_title,
                "url": ch.source_url,
                "text": ch.chunk_text[:500]
            }
            for ch in chunks
        ]
    }


@router.post("/ask-npc", response_model=AskNpcResponse)
def ask_npc(payload: AskNpcRequest, db: Session = Depends(get_db)):
    answer, sources = _generate_answer_for_npc(payload.npc_id, payload.question, db)
    return AskNpcResponse(answer=answer, sources=sources)


@router.get("/v1/game/npcs")
def list_npcs():
    return [
        {
            "npc_id": npc_id,
            "name": npc["name"],
            "style": npc["style"],
            "allowed_topics": npc["allowed_topics"],
        }
        for npc_id, npc in NPCS.items()
    ]


@router.post("/v1/game/ask", response_model=GameAskResponse)
def game_ask(payload: GameAskRequest, db: Session = Depends(get_db)):
    answer, sources = _generate_answer_for_npc(payload.npc_id, payload.question, db)
    return GameAskResponse(npc_id=payload.npc_id, answer=answer, sources=sources)

@router.get("/debug/chunks/{topic}")
def debug_chunks(topic: str, db: Session = Depends(get_db)):
    from app.models import Chunk

    chunks = (
        db.query(Chunk)
        .filter(Chunk.topic == topic)
        .limit(10)
        .all()
    )

    return {
        "topic": topic,
        "count": len(chunks),
        "items": [
            {
                "title": ch.source_title,
                "url": ch.source_url,
                "text": ch.chunk_text[:700]
            }
            for ch in chunks
        ]
    }
    
@router.get("/debug/topics")
def debug_topics(db: Session = Depends(get_db)):
    from sqlalchemy import func
    from app.models import Chunk

    rows = (
        db.query(Chunk.topic, func.count(Chunk.id))
        .group_by(Chunk.topic)
        .all()
    )

    return [{"topic": topic, "count": count} for topic, count in rows]   