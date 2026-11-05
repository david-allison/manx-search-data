#!/usr/bin/env python3
"""Seed the sparse 'ManxColumnLanguage' column in document.csv files (HANDOFF-manx-search-data.md).

Flags rows whose Manx cell is English (an English function-word ratio heuristic,
guarded by a Manx function-word counter-signal), then appends a trailing
'ManxColumnLanguage' column to the affected files: 'en' on flagged rows, empty elsewhere.

The rewrite is byte-conservative: existing bytes are never re-encoded or
re-quoted; the only change is text appended to the end of each record of the
files which gain the column. Untouched files are never opened for writing.

Usage:
  python3 tools/seed_language.py report   # TSV of rows which would be flagged
  python3 tools/seed_language.py apply    # rewrite the affected files
"""
import csv
import io
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent / "OpenData"

# English function words, excluding forms which are also common Manx words
# (Manx: as=and, she=it is, my=my/if, son=for, er=on, shee=peace ...)
EN_FUNC = {
    "the", "of", "and", "to", "in", "a", "is", "that", "it", "was", "for",
    "on", "are", "with", "his", "they", "at", "be", "this", "have", "from",
    "or", "had", "by", "not", "but", "what", "all", "were", "when", "we",
    "there", "can", "an", "which", "their", "if", "will", "up", "other",
    "about", "out", "then", "them", "these", "so", "some", "would", "him",
    "into", "has", "more", "no", "could", "than", "been", "who", "its",
    "did", "over", "only", "very", "after", "our", "just", "because",
    "any", "those", "shall", "such", "where", "here", "you", "your", "he",
    "should", "must", "being", "do", "does", "how", "why", "also",
    "thy", "thou", "thee", "unto", "hath", "doth", "upon", "ever", "never",
    "every", "against", "through", "without", "within", "while", "until",
}

# Manx function words: a counter-signal against borrowed English words in Manx rows.
# Words common in BOTH languages (as, son, row, bee, hie, each, er ...) are in
# neither list: they must not count for either side.
GV_FUNC = {
    "yn", "ny", "va", "ta", "ayns", "dy", "cha", "eh", "ee",
    "ad", "shen", "shoh", "lesh", "mysh", "agh", "myr", "tra",
    "dys", "gys", "ren", "jeh", "da", "oo", "shiu", "shin", "mee",
    "mish", "uss", "eshyn", "ish", "nyn", "e", "y", "ec", "ass", "fo",
    "roish", "harrish", "veih", "huggey", "echey", "eck", "oc", "ain",
    "eu", "dou", "dhyt", "vel", "nagh", "nish", "reesht", "foast",
    "t'eh", "v'eh", "ta'n", "va'n", "da'n", "jeh'n", "cre", "quoi",
    "kys", "dooyrt", "vees", "jean", "jannoo", "goll", "cheet",
    "va'd", "t'ad", "v'ad", "orrym", "ort", "urree", "aym", "ayd",
}

VALID_VALUES = {"", "en", "la", "gv", "mixed"}


def tokens(text):
    # hyphens stay inside tokens: syllabified Manx ("an-magh") must not shed English-looking parts
    out = []
    cur = []
    for ch in text.lower():
        if ch.isalpha() or ch in "'’-":
            cur.append("'" if ch == "’" else ch)
        elif cur:
            out.append("".join(cur).strip("'-"))
            cur = []
    if cur:
        out.append("".join(cur).strip("'-"))
    return [t for t in out if t]


def classify(manx_cell):
    """Returns 'en' if the cell is English-dominated, else None."""
    toks = tokens(manx_cell)
    if not toks:
        return None
    if max(len(t) for t in toks) <= 2:
        return None  # reading-primer syllable drills ("an en in on un")
    en = [t for t in toks if t in EN_FUNC]
    gv = sum(1 for t in toks if t in GV_FUNC)
    ratio = len(en) / len(toks)
    if len(set(en)) < 2:
        return None  # one repeated function word is not evidence of English
    if len(toks) >= 4:
        # gv <= 1: a cell with real Manx function words is Manx (or mixed) - leave for a human
        if ratio >= 0.3 and len(en) > gv and gv <= 1:
            return "en"
    else:
        # short rows are noisy (borrowed words score English) - require a stronger signal
        if ratio >= 0.5 and gv == 0:
            return "en"
    return None


def load_overrides():
    """Human-skim decisions: 'path<TAB>row<TAB>value' lines; value '' suppresses a false positive."""
    path = Path(__file__).resolve().parent / "seed_language.overrides.tsv"
    overrides = {}
    if not path.exists():
        return overrides
    for line in path.read_text(encoding="utf-8").splitlines():
        if not line.strip() or line.startswith("#"):
            continue
        rel, row, value = line.split("\t")[:3]
        assert value in VALID_VALUES, f"invalid override value {value!r}"
        overrides[(rel, int(row))] = value or None
    return overrides


def split_records(text):
    """Split CSV text into (content, terminator) pairs, quote-aware, byte-preserving."""
    records = []
    cur = []
    in_quotes = False
    i = 0
    n = len(text)
    while i < n:
        ch = text[i]
        if ch == '"':
            in_quotes = not in_quotes
            cur.append(ch)
            i += 1
        elif not in_quotes and ch == "\r" and i + 1 < n and text[i + 1] == "\n":
            records.append(("".join(cur), "\r\n"))
            cur = []
            i += 2
        elif not in_quotes and ch in "\r\n":
            records.append(("".join(cur), ch))
            cur = []
            i += 1
        else:
            cur.append(ch)
            i += 1
    if cur:
        records.append(("".join(cur), ""))
    return records


def parse_record(content):
    """Parse a single raw CSV record into fields ([] for a blank record)."""
    rows = list(csv.reader(io.StringIO(content.lstrip("﻿"))))
    if not rows:
        return []
    assert len(rows) == 1, f"record parsed to {len(rows)} rows: {content[:80]!r}"
    return rows[0]


def process(apply_changes):
    flagged_total = 0
    files_changed = 0
    overrides = load_overrides()
    writer = csv.writer(sys.stdout, delimiter="\t")
    for path in sorted(ROOT.rglob("document.csv")):
        rel = str(path.relative_to(ROOT))
        raw = path.read_text(encoding="utf-8", errors="surrogateescape", newline="")
        records = split_records(raw)
        assert "".join(c + t for c, t in records) == raw

        header_index = next((i for i, (c, _) in enumerate(records) if parse_record(c)), None)
        if header_index is None:
            continue
        header = parse_record(records[header_index][0])
        if "Manx" not in header or "ManxColumnLanguage" in header:
            continue
        manx_index = header.index("Manx")

        flags = {}
        for i in range(header_index + 1, len(records)):
            fields = parse_record(records[i][0])
            if not fields:
                continue
            cell = fields[manx_index] if manx_index < len(fields) else ""
            if (rel, i + 1) in overrides:
                language = overrides[(rel, i + 1)]
                source = "override"
            else:
                language = classify(cell)
                source = "auto"
            flags[i] = language
            if language:
                flagged_total += 1
                if not apply_changes:
                    writer.writerow([rel, i + 1, language, source,
                                     cell[:120].replace("\n", " ").replace("\t", " ")])

        if not any(flags.values()):
            continue
        files_changed += 1
        if not apply_changes:
            continue

        new_records = []
        for i, (content, terminator) in enumerate(records):
            if i == header_index:
                content += ",ManxColumnLanguage"
            elif i in flags and parse_record(content):
                content += "," + (flags[i] or "")
            new_records.append((content, terminator))
        new_raw = "".join(c + t for c, t in new_records)
        verify(raw, new_raw, path)
        path.write_text(new_raw, encoding="utf-8", errors="surrogateescape", newline="")

    print(f"flagged rows: {flagged_total}, files affected: {files_changed}", file=sys.stderr)


def verify(old_raw, new_raw, path):
    """Every original field must survive unchanged, with exactly one field appended per row."""
    old_rows = [r for r in csv.reader(io.StringIO(old_raw)) if r]
    new_rows = [r for r in csv.reader(io.StringIO(new_raw)) if r]
    assert len(old_rows) == len(new_rows), path
    assert new_rows[0] == old_rows[0] + ["ManxColumnLanguage"], path
    for old, new in zip(old_rows[1:], new_rows[1:]):
        assert new[:-1] == old, (path, old, new)
        assert new[-1] in VALID_VALUES, (path, new)


if __name__ == "__main__":
    mode = sys.argv[1] if len(sys.argv) > 1 else "report"
    if mode not in ("report", "apply"):
        sys.exit(f"unknown mode {mode!r}: use 'report' or 'apply'")
    process(apply_changes=(mode == "apply"))
