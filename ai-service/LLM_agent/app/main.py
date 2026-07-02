from sqlalchemy import text

from fastapi import FastAPI
from app.routes import router
from app.admin_routes import router as admin_router
from app.db import Base, engine

app = FastAPI(title="UrFU RAG API")

# pgvector must exist before tables that use VECTOR(...)
with engine.begin() as conn:
    conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector"))

Base.metadata.create_all(bind=engine)

app.include_router(router)
app.include_router(admin_router)


@app.get("/health")
def health():
    return {"status": "ok"}
