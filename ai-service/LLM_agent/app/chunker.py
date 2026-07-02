from typing import List


BAD_LINE_PATTERNS = [
    "студентам",
    "школьникам",
    "сотрудникам",
    "выпускникам",
    "партнерам",
    "работодателям",
    "сми",
]


def looks_like_menu_chunk(text: str) -> bool:
    low = text.lower()
    hits = sum(1 for pattern in BAD_LINE_PATTERNS if pattern in low)

    # фильтр только для явного меню
    if hits >= 5 and len(text) < 1000:
        return True

    lines = [line.strip() for line in text.split("\n") if line.strip()]
    short_lines = sum(1 for line in lines if len(line) < 30)

    if len(lines) > 12 and short_lines / len(lines) > 0.9:
        return True

    return False


def chunk_text(text: str, max_chars: int = 1200, overlap: int = 200) -> List[str]:
    text = text.strip()
    if not text:
        return []

    paragraphs = [p.strip() for p in text.split("\n") if p.strip()]
    chunks: List[str] = []
    current = ""

    for para in paragraphs:
        if len(current) + len(para) + 1 <= max_chars:
            current += ("\n" if current else "") + para
        else:
            if current and not looks_like_menu_chunk(current):
                chunks.append(current)

            if len(para) > max_chars:
                start = 0
                while start < len(para):
                    end = start + max_chars
                    piece = para[start:end]
                    if not looks_like_menu_chunk(piece):
                        chunks.append(piece)
                    start += max_chars - overlap
                current = ""
            else:
                current = para

    if current and not looks_like_menu_chunk(current):
        chunks.append(current)

    if not chunks:
        return []

    # Добавляем overlap между соседними чанками, чтобы не терять стык контекста.
    final_chunks: List[str] = []
    min_chunk_chars = 180
    for idx, chunk in enumerate(chunks):
        chunk = chunk.strip()
        if not chunk:
            continue

        if idx > 0 and overlap > 0:
            prev_tail = chunks[idx - 1].strip()[-overlap:]
            if prev_tail:
                chunk = f"{prev_tail}\n{chunk}"

        if final_chunks and len(chunk) < min_chunk_chars:
            merged = f"{final_chunks[-1]}\n{chunk}"
            if len(merged) <= max_chars + overlap:
                final_chunks[-1] = merged
                continue

        final_chunks.append(chunk)

    return final_chunks