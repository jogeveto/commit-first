#!/usr/bin/env python3
"""front-plan.py — selecciona el conjunto disjunto máximo de épicas no fundacionales.

stdin: JSON [{"epica","layer","files_scope":[glob]}]
stdout: {"selected":[...],"serialized":[{"epica","reason"}],"excluded_foundational":[...]}
Determinista: orden por 'epica'. Solape de globs por prefijo de directorio o glob idéntico.
"""
import json, sys, fnmatch

def overlap(a, b):
    for ga in a:
        for gb in b:
            if ga == gb or fnmatch.fnmatch(ga, gb) or fnmatch.fnmatch(gb, ga):
                return True
            pa, pb = ga.split("*", 1)[0], gb.split("*", 1)[0]
            # Fail closed: a leading/empty-prefix wildcard could match anything → assume overlap.
            if not pa or not pb:
                return True
            if pa.startswith(pb) or pb.startswith(pa):
                return True
    return False

def _empty_result():
    return {"selected": [], "serialized": [], "excluded_foundational": []}

def main():
    try:
        cands = json.load(sys.stdin)
        if not isinstance(cands, list) or not all(isinstance(c, dict) for c in cands):
            raise ValueError("entrada inválida: se esperaba una lista de objetos")
        cands = sorted(cands, key=lambda c: c.get("epica",""))
        excluded = [c["epica"] for c in cands if c.get("layer") == "foundational"]
        pool = [c for c in cands if c.get("layer") != "foundational"]
        selected, sel_scopes, serialized = [], [], []
        for c in pool:
            sc = c.get("files_scope") or []
            if any(overlap(sc, s) for s in sel_scopes):
                serialized.append({"epica": c["epica"], "reason": "solape de files_scope con épica seleccionada"})
            else:
                selected.append(c["epica"]); sel_scopes.append(sc)
        print(json.dumps({"selected":selected,"serialized":serialized,"excluded_foundational":excluded}))
    except Exception:
        print(json.dumps(_empty_result()))
    return 0

if __name__ == "__main__":
    sys.exit(main())
