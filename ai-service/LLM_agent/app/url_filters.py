from urllib.parse import urlparse, urlunparse

EXCLUDED_EXTENSIONS = (
    ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
    ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp",
    ".zip", ".rar", ".7z", ".mp4", ".mp3"
)

EXCLUDED_PATH_PARTS = (
    "/search",
    "/bitrix/",
    "/upload/",
    "/auth",
    "/login",
    "/logout",
)

ALLOWED_DOMAIN = "urfu.ru"

ALLOWED_PREFIXES = (
    "/ru/",
)

ALLOWED_KEYWORDS = (
    "/ru/applicant",
    "/ru/students",
    "/ru/about",
    "/ru/education",
    "/ru/science",
    "/ru/life",
    "/ru/university",
    "/ru/institutes",
)


def normalize_url(url: str) -> str:
    parsed = urlparse(url)
    cleaned = parsed._replace(query="", fragment="")
    normalized = urlunparse(cleaned)

    if normalized.endswith("/") and normalized != f"{parsed.scheme}://{parsed.netloc}/":
        normalized = normalized[:-1]

    return normalized


def is_internal_url(url: str) -> bool:
    parsed = urlparse(url)
    return parsed.netloc.endswith(ALLOWED_DOMAIN)


def is_allowed_url(url: str) -> bool:
    parsed = urlparse(url)
    path = parsed.path.lower()

    if not parsed.scheme.startswith("http"):
        return False

    if not is_internal_url(url):
        return False

    if not any(path.startswith(prefix) for prefix in ALLOWED_PREFIXES):
        return False

    if any(path.endswith(ext) for ext in EXCLUDED_EXTENSIONS):
        return False

    if any(part in path for part in EXCLUDED_PATH_PARTS):
        return False

    if not any(keyword in path for keyword in ALLOWED_KEYWORDS):
        return False

    return True
