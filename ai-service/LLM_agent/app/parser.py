import hashlib
import re
import requests
import trafilatura
from bs4 import BeautifulSoup
from playwright.sync_api import sync_playwright

NOISE_HINTS = re.compile(
    r"(menu|nav|navbar|header|footer|sidebar|breadcrumb|cookie|popup|modal|banner|share|widget|toolbar|pagination)",
    re.IGNORECASE,
)

BLOCK_TAGS = ["p", "h1", "h2", "h3", "h4", "li", "td"]


def _is_noise_node(tag) -> bool:
    attrs = " ".join(
        [
            tag.get("id", ""),
            " ".join(tag.get("class", [])),
            tag.get("role", ""),
            tag.get("aria-label", ""),
        ]
    )
    return bool(NOISE_HINTS.search(attrs))


def _extract_text_blocks(scope: BeautifulSoup) -> list[str]:
    extracted_lines: list[str] = []
    seen = set()

    for block in scope.find_all(BLOCK_TAGS):
        if block.find_parent(["nav", "header", "footer", "aside", "menu"]):
            continue
        if block.find_parent(lambda t: t and _is_noise_node(t)):
            continue

        text_line = block.get_text(separator=" ", strip=True)
        text_line = re.sub(r"\s+", " ", text_line).strip()
        if len(text_line) < 18:
            continue
        if text_line in seen:
            continue
        seen.add(text_line)
        extracted_lines.append(text_line)

    return extracted_lines


def _extract_with_trafilatura(html: str) -> list[str]:
    try:
        extracted = trafilatura.extract(
            html,
            include_comments=False,
            include_tables=True,
            output_format="txt",
            favor_precision=False,
        )
        if not extracted:
            return []
        lines = [re.sub(r"\s+", " ", line).strip() for line in extracted.splitlines()]
        return [line for line in lines if len(line) >= 18]
    except Exception:
        return []


def _pick_main_scope(soup: BeautifulSoup) -> BeautifulSoup:
    selectors = [
        "main",
        "article",
        '[role="main"]',
        "#content",
        ".content",
        ".main-content",
    ]
    for selector in selectors:
        node = soup.select_one(selector)
        if node:
            return node

    # Fallback: выбираем самый "текстовый" контейнер
    candidates = soup.find_all(["div", "section"], limit=2000)
    if not candidates:
        return soup
    return max(candidates, key=lambda t: len(t.get_text(" ", strip=True)), default=soup)


def _load_html_with_playwright(url: str) -> str:
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context()
        page = context.new_page()
        try:
            # Убираем тяжелые ресурсы для снижения таймаутов.
            page.route(
                "**/*",
                lambda route: route.abort()
                if route.request.resource_type in {"image", "font", "media"}
                else route.continue_(),
            )
            page.goto(url, timeout=45000, wait_until="domcontentloaded")
            try:
                page.wait_for_load_state("networkidle", timeout=5000)
            except Exception:
                pass
            page.wait_for_timeout(1200)
            return page.content()
        finally:
            browser.close()


def _load_html_with_requests(url: str) -> str:
    response = requests.get(
        url,
        timeout=20,
        headers={"User-Agent": "UrFU-Pixel-Campus-Bot/1.0 (+educational project)"},
    )
    response.raise_for_status()
    response.encoding = response.apparent_encoding
    return response.text


def parse_url(url: str) -> dict:
    html = ""
    try:
        html = _load_html_with_playwright(url)
    except Exception as e:
        print(f"[PLAYWRIGHT ОШИБКА] {url}: {e}")
        try:
            html = _load_html_with_requests(url)
        except Exception as req_e:
            print(f"[REQUESTS ОШИБКА] {url}: {req_e}")

    if not html:
        return {"url": url, "title": "Ошибка", "cleaned_text": "", "content_hash": ""}

    soup = BeautifulSoup(html, "html.parser")
    title_tag = soup.find("title")
    title = title_tag.text.strip() if title_tag else "Страница УрФУ"

    for bad_tag in soup(["script", "style", "nav", "header", "footer", "aside", "menu", "button"]):
        bad_tag.decompose()

    for node in soup.find_all(lambda t: t and _is_noise_node(t)):
        node.decompose()

    scope = _pick_main_scope(soup)
    extracted_lines = _extract_text_blocks(scope)

    # Если внутри main/article ничего не нашли — пробуем по всей странице.
    if not extracted_lines:
        extracted_lines = _extract_text_blocks(soup)

    # Если после HTML-cleaning текста мало, пробуем trafilatura fallback.
    if len("\n".join(extracted_lines).strip()) < 120:
        tf_lines = _extract_with_trafilatura(html)
        for line in tf_lines:
            if line not in extracted_lines:
                extracted_lines.append(line)

    text = "\n".join(extracted_lines).strip()
    content_hash = hashlib.md5(text.encode("utf-8")).hexdigest() if text else ""

    return {
        "url": url,
        "title": title,
        "cleaned_text": text,
        "content_hash": content_hash,
    }