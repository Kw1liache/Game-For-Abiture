from app.db import SessionLocal
from app.retriever import retrieve_chunks, build_context
from app.gigachat_client import GigaChatClient

def main():
    print("🤖 Загрузка RAG-системы (Гид по УрФУ)...")
    
    # 1. Инициализируем клиента GigaChat
    client = GigaChatClient()
    
    # 2. Открываем сессию базы данных
    db = SessionLocal()
    
    # Темы, по которым мы будем искать ответы (основано на ваших логах парсера)
    allowed_topics = ["general", "student_life", "admission", "study_process", "science", "dormitory"]
    
    print("✅ Система готова! Введите 'выход' для завершения.\n")
    print("-" * 50)

    try:
        # Делаем бесконечный цикл, чтобы можно было задавать вопросы один за другим
        while True:
            user_question = input("\n👤 Ваш вопрос: ")
            
            if user_question.lower() in ["выход", "exit", "quit"]:
                print("Завершение работы...")
                break
                
            if not user_question.strip():
                continue

            print("🔍 Ищу информацию в базе УрФУ (GigaChat + Postgres)...")
            
            # 3. Ищем куски текста в базе через эмбеддинги
            chunks = retrieve_chunks(db=db, question=user_question, allowed_topics=allowed_topics)
            
            # 4. Собираем найденные куски в один текст
            context = build_context(chunks)
            
            # (Для отладки: раскомментируйте строку ниже, чтобы видеть, что нашла база)
            print(f"\n[НАЙДЕННЫЙ КОНТЕКСТ]:\n{context}\n")

            # 5. Формируем System Prompt (Характер NPC + Знания)
            system_prompt = f"""Ты дружелюбный и знающий гид-ассистент по УрФУ (Уральскому федеральному университету).
Отвечай на вопросы пользователя подробно, вежливо и понятно.
ОПИРАЙСЯ ТОЛЬКО НА ЭТУ ИНФОРМАЦИЮ С САЙТА УРФУ:
{context}

Если в предоставленном тексте нет ответа на вопрос пользователя, честно скажи: "К сожалению, я не нашел точной информации об этом на сайте университета." Не выдумывай факты."""

            print("🧠 GigaChat думает...")
            
            # 6. Отправляем запрос в Сбер
            answer = client.chat(
                system_prompt=system_prompt,
                user_prompt=user_question
            )
            
            print(f"\n🎓 Гид УрФУ: {answer}")
            print("-" * 50)

    except Exception as e:
        print(f"❌ Произошла ошибка: {e}")
    finally:
        db.close() # Обязательно закрываем базу данных

if __name__ == "__main__":
    main()