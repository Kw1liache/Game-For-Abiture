import os
from pydantic import BaseModel
from dotenv import load_dotenv

load_dotenv()


def normalize_gigachat_chat_url(url: str) -> str:
    """Частая ошибка в .env: указан только .../api/v1 без /chat/completions."""
    u = (url or "").strip().rstrip("/")
    if u.endswith("/api/v1"):
        return f"{u}/chat/completions"
    return (url or "").strip()


class Settings(BaseModel):
    database_url: str = os.getenv("DATABASE_URL", "postgresql://postgres:postgres@localhost:5432/urfu_game")

    gigachat_auth_url: str = os.getenv("GIGACHAT_AUTH_URL", "")
    gigachat_api_url: str = normalize_gigachat_chat_url(os.getenv("GIGACHAT_API_URL", ""))
    # === ДОБАВЛЕНА НОВАЯ СТРОКА ДЛЯ ЭМБЕДДИНГОВ ===
    gigachat_embeddings_url: str = os.getenv("GIGACHAT_EMBEDDINGS_URL", "https://gigachat.devices.sberbank.ru/api/v1/embeddings")
    
    gigachat_scope: str = os.getenv("GIGACHAT_SCOPE", "GIGACHAT_API_PERS")
    gigachat_client_id: str = os.getenv("GIGACHAT_CLIENT_ID", "")
    gigachat_client_secret: str = os.getenv("GIGACHAT_CLIENT_SECRET", "")
    gigachat_verify_ssl: bool = os.getenv("GIGACHAT_VERIFY_SSL", "false").lower() == "true"

    admin_token: str = os.getenv("ADMIN_TOKEN", "")

    embedding_dim: int = int(os.getenv("EMBEDDING_DIM", "1024"))

    top_k: int = int(os.getenv("TOP_K", "5"))
    max_context_chars: int = int(os.getenv("MAX_CONTEXT_CHARS", "6000"))
    min_page_text_chars: int = int(os.getenv("MIN_PAGE_TEXT_CHARS", "120"))

    start_urls: list[str] = [
        "https://urfu.ru/ru/",
        "https://urfu.ru/ru/about/",
        "https://urfu.ru/ru/about/today/figures/",
        "https://campus.urfu.ru/ru/",
        "https://urfu.ru/priemurfu/",
        "https://urfu.ru/ru/applicant/docs-abiturient/exams/",
        "https://urfu.ru/ru/instituty/",
        "https://rtf.urfu.ru/ru/",
        "https://rtf.urfu.ru/about-institute/campus/",
        "https://rtf.urfu.ru/about-institute/departments/",
        "https://rtf.urfu.ru/about-institute/",
        "https://rtf.urfu.ru/about-institute/nauchnye-centry-i-laboratorii/",
        "https://rtf.urfu.ru/about-institute/project-workshop/",
        "https://priem-rtf.urfu.ru/",
        "https://priem-rtf.urfu.ru/level/bakalavriat/",
        "https://priem-rtf.urfu.ru/level/bakalavriat/#contacts",
        "https://rtf.urfu.ru/ru/student/",
        "https://rtf.urfu.ru/ru/science/",
        "https://priem-rtf.urfu.ru/settlement/",
        "https://rtf.urfu.ru/ru/student/profburo/",
        "https://rtf.urfu.ru/resident-learning/",
        "https://rtf.urfu.ru/ru/student/foreign-student/",
        "https://campus.urfu.ru/ru/studencheskii-gorodok/obshchezhitija/obshchezhitija/obshchezhitija/",
        "https://urfu.ru/ru/students/social/campus/",
        "https://cat-rtf.urfu.ru/students_bachelor",
        "https://cat-rtf.urfu.ru/"
    ]
    
    max_pages: int = int(os.getenv("MAX_PAGES", "20"))
    crawl_delay: float = float(os.getenv("CRAWL_DELAY", "3.0"))

settings = Settings()