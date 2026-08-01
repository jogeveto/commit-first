"""Microservicio ATS — Asistente de Empleabilidad IA (scaffold).

Auditor determinista del % de match CV↔vacante. Aquí solo el esqueleto:
health check + un stub determinista de /match. El algoritmo NLP real
(spaCy/scikit-learn, TF-IDF) se implementa en el slice de EP-006 (HU-016)
con TDD; el determinismo (misma entrada -> mismo %) es un AC de esa historia.
"""
from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI(title="ATS Service", version="0.1.0-scaffold")


class MatchRequest(BaseModel):
    cv_text: str
    vacancy_text: str


@app.get("/health")
def health():
    return {"status": "ok", "service": "ats-service"}


@app.post("/match")
def match(req: MatchRequest):
    # STUB determinista: proporción de tokens de la vacante presentes en el CV.
    # No es el algoritmo final; existe para probar el wiring end-to-end del scaffold.
    vocab = {t.lower() for t in req.vacancy_text.split() if t.strip()}
    if not vocab:
        return {"score": 0.0, "confidence": "baja", "note": "vacante sin texto"}
    cv_tokens = {t.lower() for t in req.cv_text.split() if t.strip()}
    hits = len(vocab & cv_tokens)
    score = round(100.0 * hits / len(vocab), 1)
    return {"score": score, "confidence": "alta", "stub": True}
