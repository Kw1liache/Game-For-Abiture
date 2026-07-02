import time

from app.config import settings
from app.gigachat_client import GigaChatClient

gigachat = GigaChatClient()


def embed_text(text: str) -> list[float]:
    """Эмбеддинг для документов — prefix 'passage:'"""
    for attempt in range(5):
        try:
            vec = gigachat.get_embedding(f"passage: {text}")
            if len(vec) != settings.embedding_dim:
                raise ValueError(f"Unexpected embedding dim: {len(vec)} != {settings.embedding_dim}")
            return vec

        except Exception as e:
            err = str(e)
            if "503" in err or "Model too busy" in err:
                print(f"😴 [EMB] Модель/сервис загружается... ({attempt+1}/5)")
                time.sleep(15)
            elif "429" in err:
                print(f"🚦 [EMB] Rate-limit ({attempt+1}/5)")
                time.sleep(5)
            else:
                print(f"[ОШИБКА EMBEDDINGS] {e}")
                if attempt == 4:
                    raise
                time.sleep(5)

    raise Exception("Не удалось получить вектор после 5 попыток.")


def embed_query(text: str) -> list[float]:
    """Эмбеддинг для поисковых запросов — prefix 'query:'"""
    vec = gigachat.get_embedding(f"query: {text}")
    if len(vec) != settings.embedding_dim:
        raise ValueError(f"Unexpected embedding dim: {len(vec)} != {settings.embedding_dim}")
    return vec