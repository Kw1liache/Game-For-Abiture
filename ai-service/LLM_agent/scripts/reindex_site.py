from sqlalchemy.sql import func
from sqlalchemy import text as sql_text
from app.db import SessionLocal, engine, Base
from app.models import Page, Chunk
from app.parser import parse_url
from app.chunker import chunk_text
from app.embeddings import embed_text
from app.crawler import crawl_site
from app.topic_mapper import map_topic_by_url
from app.config import settings
import time


def upsert_page_and_chunks(db, parsed, section, topic, npc_tag):
    page = db.query(Page).filter(Page.url == parsed["url"]).first()

    if page and page.content_hash == parsed["content_hash"]:
        page.last_crawled_at = func.now()
        print(f"[SKIP] Без изменений: {parsed['url']}")
        return

    if not page:
        page = Page(
            url=parsed["url"],
            title=parsed["title"],
            section=section,
            cleaned_text=parsed["cleaned_text"],
            content_hash=parsed["content_hash"],
        )
        db.add(page)
        db.flush()
    else:
        page.title = parsed["title"]
        page.section = section
        page.cleaned_text = parsed["cleaned_text"]
        page.content_hash = parsed["content_hash"]
        page.last_crawled_at = func.now()

        db.query(Chunk).filter(Chunk.page_id == page.id).delete()

    chunks = chunk_text(parsed["cleaned_text"], max_chars=1200, overlap=200)
    unique_chunks = []
    seen_hashes = set()
    for chunk in chunks:
        normalized = " ".join(chunk.split())
        ch_hash = hash(normalized)
        if ch_hash in seen_hashes:
            continue
        seen_hashes.add(ch_hash)
        unique_chunks.append(chunk)

    print(
        f"[DEBUG] {parsed['url']} -> topic={topic}, "
        f"chunks_after_filter={len(chunks)}, unique_chunks={len(unique_chunks)}"
    )

    for idx, ch_text in enumerate(unique_chunks):
        emb = embed_text(ch_text)
        if len(emb) != settings.embedding_dim:
            raise ValueError(
                f"Неверная размерность embedding для {parsed['url']}: "
                f"{len(emb)} != {settings.embedding_dim}"
            )
        chunk = Chunk(
            page_id=page.id,
            chunk_index=idx,
            chunk_text=ch_text,
            topic=topic,
            npc_tag=npc_tag,
            source_title=parsed["title"],
            source_url=parsed["url"],
            embedding=emb,
        )
        db.add(chunk)
        
        time.sleep(0.3)

    print(f"[INDEXED] {parsed['url']} | topic={topic} | chunks={len(unique_chunks)}")


def main():
    from sqlalchemy import text as sql_text
    with engine.connect() as conn:
        conn.execute(sql_text("CREATE EXTENSION IF NOT EXISTS vector"))
        conn.commit()

    Base.metadata.create_all(bind=engine)
    db = SessionLocal()

    try:
        all_found_urls = set()

        print("🕸️ Начинаем сбор ссылок со всех сайтов из конфига...")
        
        # === БЕРЕМ СПИСОК ИЗ СЕТТИНГОВ ===
        for base_url in settings.start_urls:
            print(f"-> Сканирую: {base_url}")
            urls = crawl_site(
                start_url=base_url,
                max_pages=settings.max_pages,
                delay=settings.crawl_delay,
            )
            all_found_urls.update(urls)

        print(f"\n[INFO] Итого собрано уникальных страниц: {len(all_found_urls)}")

        for url in all_found_urls:
            try:
                parsed = parse_url(url)
                text = parsed["cleaned_text"]

                text_len = len(text.strip()) if text else 0
                if text_len < settings.min_page_text_chars:
                    print(
                        f"[SKIP] Слишком мало текста ({text_len} < {settings.min_page_text_chars}): {url}"
                    )
                    continue

                topic, npc_tag = map_topic_by_url(url)

                upsert_page_and_chunks(
                    db=db,
                    parsed=parsed,
                    section=topic,
                    topic=topic,
                    npc_tag=npc_tag,
                )
                db.commit()

            except Exception as e:
                db.rollback()
                print(f"[ERROR] {url}: {e}")

    finally:
        db.close()


if __name__ == "__main__":
    main()
    