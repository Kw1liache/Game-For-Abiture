from app.config import settings
from app.gigachat_client import GigaChatClient

print("AUTH URL:", settings.gigachat_auth_url)
print("SCOPE:", settings.gigachat_scope)
print("SECRET LEN:", len(settings.gigachat_client_secret))

client = GigaChatClient()
token = client.authenticate()
print("TOKEN OK")