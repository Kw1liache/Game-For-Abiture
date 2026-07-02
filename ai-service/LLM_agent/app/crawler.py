import time
import requests
from bs4 import BeautifulSoup
from urllib.parse import urljoin
from app.url_filters import normalize_url, is_allowed_url

HEADERS = {
    "User-Agent": "UrFU-Pixel-Campus-Bot/1.0 (+educational project)"
}


def extract_links(html: str, base_url: str) -> set[str]:
    soup = BeautifulSoup(html, "html.parser")
    links = set()

    for a in soup.find_all("a", href=True):
        href = a["href"].strip()
        full_url = urljoin(base_url, href)
        full_url = normalize_url(full_url)

        if is_allowed_url(full_url):
            links.add(full_url)

    return links


def crawl_site(start_url: str, max_pages: int = 30, delay: float = 1.0) -> list[str]:
    visited = set()
    queue = [normalize_url(start_url)]
    result = []

    session = requests.Session()
    session.headers.update(HEADERS)

    while queue and len(result) < max_pages:
        url = queue.pop(0)
        if url in visited:
            continue

        visited.add(url)

        try:
            response = session.get(url, timeout=20)
            response.raise_for_status()

            ctype = response.headers.get("Content-Type", "")
            if "text/html" not in ctype:
                continue

            response.encoding = response.apparent_encoding
            html = response.text

            result.append(url)
            print(f"[CRAWL] {url}")

            links = extract_links(html, url)
            for link in links:
                if link not in visited and link not in queue and len(result) + len(queue) < max_pages * 3:
                    queue.append(link)

            time.sleep(delay)

        except Exception as e:
            print(f"[CRAWL ERROR] {url}: {e}")

    return result
