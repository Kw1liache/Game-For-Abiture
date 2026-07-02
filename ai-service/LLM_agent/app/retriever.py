from sqlalchemy.orm import Session
from sqlalchemy import select
from app.models import Chunk
from app.embeddings import embed_text, embed_query
from app.config import settings


def retrieve_chunks(db: Session, question: str, allowed_topics: list[str], top_k: int | None = None):
    top_k = top_k or settings.top_k
    q_emb = embed_query(question)

    stmt = (
        select(Chunk)
        .where(Chunk.topic.in_(allowed_topics))
        .order_by(Chunk.embedding.cosine_distance(q_emb))
        .limit(top_k)
    )

    return db.execute(stmt).scalars().all()


def build_context(chunks: list, max_chars: int | None = None) -> str:
    max_chars = max_chars or settings.max_context_chars
    parts = []
    total = 0

    for ch in chunks:
        block = (
            f"[Источник: {ch.source_title or 'Без названия'} | {ch.source_url or ''}]\n"
            f"{ch.chunk_text}\n"
        )

        if total + len(block) > max_chars:
            break

        parts.append(block)
        total += len(block)

    return "\n\n".join(parts)
