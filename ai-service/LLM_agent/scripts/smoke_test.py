import json
import os
import sys
from urllib import error, request


def _http_get(url: str, timeout: int = 20) -> tuple[int, str]:
    req = request.Request(url, method="GET")
    with request.urlopen(req, timeout=timeout) as resp:
        body = resp.read().decode("utf-8", errors="ignore")
        return resp.status, body


def _http_post_json(url: str, payload: dict, timeout: int = 30, headers: dict | None = None) -> tuple[int, str]:
    data = json.dumps(payload).encode("utf-8")
    req_headers = {"Content-Type": "application/json"}
    if headers:
        req_headers.update(headers)
    req = request.Request(
        url,
        data=data,
        method="POST",
        headers=req_headers,
    )
    with request.urlopen(req, timeout=timeout) as resp:
        body = resp.read().decode("utf-8", errors="ignore")
        return resp.status, body


def main() -> int:
    print("[1/3] Checking backend health from docker network...")
    try:
        status, body = _http_get("http://backend:8000/health")
        if status != 200 or '"status"' not in body:
            print(f"[FAIL] backend health unexpected: status={status}, body={body}")
            return 1
        print(f"[OK] backend health: {body}")
    except error.URLError as e:
        print(f"[FAIL] backend health unreachable: {e}")
        return 1

    print("[2/3] Checking n8n webhook endpoint...")
    payload = {
        "npc_id": "dean_irit",
        "question": "Какие документы нужны для поступления?",
        "player_id": "smoke_tester",
        "session_id": "smoke_session",
    }
    req_headers = {}
    api_key = os.getenv("N8N_UNITY_API_KEY", "").strip()
    if api_key:
        req_headers["X-API-Key"] = api_key

    try:
        status, body = _http_post_json(
            "http://n8n:5678/webhook/urfu-npc-ask",
            payload,
            timeout=60,
            headers=req_headers,
        )
    except error.HTTPError as e:
        raw = e.read().decode("utf-8", errors="ignore")
        print(f"[FAIL] n8n webhook HTTP error: status={e.code}, body={raw}")
        print(
            "Tip: import and activate workflow "
            "'integrations/n8n/urfu_npc_webhook_workflow.json' in n8n UI."
        )
        return 1
    except error.URLError as e:
        print(f"[FAIL] n8n webhook unreachable: {e}")
        return 1

    if status != 200:
        print(f"[FAIL] n8n webhook unexpected status={status}, body={body}")
        return 1
    print("[OK] n8n webhook responded with 200")

    print("[3/3] Validating response JSON fields...")
    try:
        data = json.loads(body)
    except json.JSONDecodeError:
        print(f"[FAIL] response is not JSON: {body}")
        return 1

    missing = [k for k in ("npc_id", "answer", "sources") if k not in data]
    if missing:
        print(f"[FAIL] missing fields in response: {missing}; body={body}")
        return 1

    if not str(data.get("answer", "")).strip():
        print(f"[FAIL] empty answer in response: {body}")
        return 1

    print("[OK] smoke test passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
