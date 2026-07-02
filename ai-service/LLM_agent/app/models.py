from sqlalchemy import Column, Integer, String, Text, DateTime, ForeignKey
from sqlalchemy.sql import func
from sqlalchemy.orm import relationship
from pgvector.sqlalchemy import Vector
from app.db import Base
from app.config import settings


class Page(Base):
    __tablename__ = "pages"

    id = Column(Integer, primary_key=True, index=True)
    url = Column(String, unique=True, nullable=False, index=True)
    title = Column(String, nullable=True)
    section = Column(String, nullable=True)
    content_hash = Column(String, nullable=False, index=True)
    cleaned_text = Column(Text, nullable=False)
    last_crawled_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), server_default=func.now(), onupdate=func.now())

    chunks = relationship("Chunk", back_populates="page", cascade="all, delete-orphan")


class Chunk(Base):
    __tablename__ = "chunks"

    id = Column(Integer, primary_key=True, index=True)
    page_id = Column(Integer, ForeignKey("pages.id", ondelete="CASCADE"), nullable=False)
    chunk_index = Column(Integer, nullable=False)
    chunk_text = Column(Text, nullable=False)
    topic = Column(String, nullable=True)
    npc_tag = Column(String, nullable=True)
    source_title = Column(String, nullable=True)
    source_url = Column(String, nullable=True)
    embedding = Column(Vector(settings.embedding_dim))

    page = relationship("Page", back_populates="chunks")
    