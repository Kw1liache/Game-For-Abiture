import logging
import httpx
import uuid
from app.config import settings

logger = logging.getLogger(__name__)


class GigaChatClient:
    def __init__(self):
        self.access_token = None

    def authenticate(self):
        headers = {
            "Authorization": f"Basic {settings.gigachat_client_secret}",
            "RqUID": str(uuid.uuid4()),
            "Content-Type": "application/x-www-form-urlencoded",
        }

        data = {
            "scope": settings.gigachat_scope
        }

        timeout = httpx.Timeout(connect=30.0, read=120.0, write=30.0, pool=30.0)

        with httpx.Client(verify=settings.gigachat_verify_ssl, timeout=timeout) as client:
            response = client.post(settings.gigachat_auth_url, headers=headers, data=data)
            response.raise_for_status()
            payload = response.json()

        self.access_token = payload["access_token"]
        return self.access_token

    def ensure_token(self):
        if not self.access_token:
            self.authenticate()

    def chat(self, system_prompt: str, user_prompt: str) -> str:
        self.ensure_token()

        headers = {
            "Authorization": f"Bearer {self.access_token}",
            "Content-Type": "application/json",
        }

        payload = {
            "model": "GigaChat",
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt},
            ],
            "temperature": 0.3,
            "max_tokens": 500,
        }

        timeout = httpx.Timeout(connect=30.0, read=180.0, write=30.0, pool=30.0)

        with httpx.Client(verify=settings.gigachat_verify_ssl, timeout=timeout) as client:
            response = client.post(settings.gigachat_api_url, headers=headers, json=payload)

            if response.status_code == 401:
                self.authenticate()
                headers["Authorization"] = f"Bearer {self.access_token}"
                response = client.post(settings.gigachat_api_url, headers=headers, json=payload)

            response.raise_for_status()
            result = response.json()

        return result["choices"][0]["message"]["content"]
    
    def get_embedding(self, text: str) -> list[float]:
        self.ensure_token()

        headers = {
            "Authorization": f"Bearer {self.access_token}",
            "Content-Type": "application/json",
        }

        # Payload для API эмбеддингов GigaChat
        payload = {
            "model": "Embeddings", # Это стандартное имя модели для векторов у Сбера
            "input": [text]        # GigaChat принимает массив строк
        }

        timeout = httpx.Timeout(connect=30.0, read=120.0, write=30.0, pool=30.0)

        with httpx.Client(verify=settings.gigachat_verify_ssl, timeout=timeout) as client:
            # Обратите внимание: используется URL для эмбеддингов
            response = client.post(settings.gigachat_embeddings_url, headers=headers, json=payload)

            # Если токен протух, обновляем и пробуем еще раз (как в вашем методе chat)
            if response.status_code == 401:
                self.authenticate()
                headers["Authorization"] = f"Bearer {self.access_token}"
                response = client.post(settings.gigachat_embeddings_url, headers=headers, json=payload)

            response.raise_for_status()
            result = response.json()

        # Возвращаем сам вектор (список float)
        return result["data"][0]["embedding"]
    