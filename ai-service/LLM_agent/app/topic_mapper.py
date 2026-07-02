def map_topic_by_url(url: str) -> tuple[str, str]:
    url_lower = url.lower()

    if "applicant" in url_lower or "abitur" in url_lower or "postuplenie" in url_lower:
        return "admission", "administration"

    if "campus" in url_lower or "hostel" in url_lower or "obshchezhit" in url_lower:
        return "dormitory", "dormitory_npc"

    if "students" in url_lower or "/life" in url_lower:
        return "student_life", "stud_council"

    if "science" in url_lower:
        return "science", "professor"

    if "sport" in url_lower:
        return "sports", "stud_council"

    if "education" in url_lower:
        return "study_process", "professor"

    if "institutes" in url_lower or "irit" in url_lower or "rtf" in url_lower:
        return "institute_irit", "professor"

    if "about" in url_lower or "university" in url_lower:
        return "general", "professor"

    return "general", "professor"