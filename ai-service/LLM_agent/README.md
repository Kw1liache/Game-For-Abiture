# UrFU Pixel Campus — Canonical Runtime

Канонический режим проекта: только `docker compose`.

- `db`: PostgreSQL + pgvector
- `backend`: FastAPI + parser + RAG
- `n8n`: webhook-шлюз для Unity

Unity всегда отправляет вопросы в n8n:
`POST http://<server>:5678/webhook/urfu-npc-ask`

n8n внутри docker-сети вызывает backend:
`POST http://backend:8000/v1/game/ask`

backend вызывает внешние API:
- GigaChat API (эмбеддинги и генерация ответа)

---

## Запуск (один сценарий)

### 1) Перейти в папку
```bash
cd LLM_agent
```

### 2) Подготовить `.env`
Скопируй `.env.example` в `.env` и заполни:
- `GIGACHAT_AUTH_URL`
- `GIGACHAT_API_URL`
- `GIGACHAT_CLIENT_ID`
- `GIGACHAT_CLIENT_SECRET`
- `ADMIN_TOKEN`
- `N8N_UNITY_API_KEY`

### 3) Поднять весь стек
```bash
docker compose up -d --build
```

### 4) Выполнить индексацию
```bash
docker compose exec backend python -m scripts.reindex_site
```

---

## Что и где работает

- n8n UI: `http://127.0.0.1:5678`
- Unity webhook endpoint: `http://127.0.0.1:5678/webhook/urfu-npc-ask`
- Backend не опубликован наружу, доступен только внутри docker-сети как `http://backend:8000`

---

## Настройка n8n

1. Открой `http://127.0.0.1:5678`
2. Импортируй workflow-файлы:
   - `integrations/n8n/urfu_npc_webhook_workflow.json`
   - `integrations/n8n/urfu_reindex_scheduler_workflow.json`
   - `integrations/n8n/urfu_reindex_watchdog_workflow.json`
   - `integrations/n8n/urfu_healthcheck_workflow.json`
3. Активируй workflow:
   - `URFU NPC Gateway (Secure)` — обязательно
   - остальные — по необходимости (рекомендуется включить все)
4. Проверь в ноде `Call RAG API`, что URL равен `http://backend:8000/v1/game/ask`

---

## Тестирование

### Быстрый тест backend
- health изнутри контейнера:
  - `docker compose exec backend python -c "import requests; print(requests.get('http://127.0.0.1:8000/health', timeout=10).text)"`
- темы в векторной БД:
  - `docker compose exec backend python -c "import requests; print(requests.get('http://127.0.0.1:8000/debug/topics', timeout=10).text)"`

### Тест через n8n webhook (канонический путь)
`POST http://127.0.0.1:5678/webhook/urfu-npc-ask`

Header (если задан `N8N_UNITY_API_KEY`):
- `X-API-Key: <your-key>`

```json
{
  "npc_id": "dean_irit",
  "question": "Какие документы нужны для поступления?",
  "player_id": "player_42",
  "session_id": "session_a1"
}
```

Ожидаемый ответ:
```json
{
  "npc_id": "dean_irit",
  "answer": "...",
  "sources": [
    {"title": "Источник", "url": "https://..."}
  ]
}
```

### End-to-end smoke test (одной командой)
После импорта и активации workflow в n8n:

```bash
docker compose exec backend python -m scripts.smoke_test
```

Скрипт проверяет:
- доступность backend health внутри docker-сети;
- вызов n8n webhook;
- структуру JSON-ответа (`npc_id`, `answer`, `sources`).

---

## Unity интеграция

- Скопируй `integrations/unity/NpcApiClient.cs` и `integrations/unity/NpcDialogController.cs` в проект Unity.
- В `NpcApiClient.endpointUrl` укажи только:
  `http://127.0.0.1:5678/webhook/urfu-npc-ask`
- В `NpcApiClient.apiKey` укажи тот же ключ, что в `N8N_UNITY_API_KEY`.
- На Canvas добавь `InputField`, `Text`, `Button` и свяжи их в `NpcDialogController`.

---

## Production-практика по workflow

- `URFU NPC Gateway (Secure)`:
  - принимает Unity-запросы;
  - проверяет `X-API-Key`;
  - валидирует поля `npc_id`, `question`;
  - вызывает backend (`/v1/game/ask`);
  - проверяет структуру ответа и возвращает JSON в Unity.

- `URFU Reindex Scheduler`:
  - каждые 6 часов вызывает `POST /admin/reindex` с `X-Admin-Token`.

- `URFU Reindex Watchdog`:
  - каждые 10 минут проверяет `GET /admin/reindex/status`;
  - формирует alert payload при `last_error`.

- `URFU Runtime Healthcheck`:
  - каждые 5 минут проверяет `GET /health`;
  - формирует alert payload, если статус backend не `ok`.
