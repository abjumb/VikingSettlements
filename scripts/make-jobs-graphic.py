#!/usr/bin/env python3
"""Generates docs/jobs.svg - the ten-jobs reference card in the design
system's docs-art palette (deep pine background, pine cards, wood initial
badges, moss needs-chips). Layout mirrors the Claude Design handoff's
"Docs Graphic - Jobs" screen; copy states exactly what SettlerWork does."""

import html
import pathlib

# Docs-art palette (Design System III).
BG_TOP, BG_BOTTOM = "#1E3A34", "#122019"
CARD, CARD_STROKE = "#21403A", "#2F4F47"
IVORY, MIST, BODY = "#F4F2E6", "#93ACA4", "#DAE4DC"
GOLD, MOSS = "#F2DC96", "#B5C65B"
MOSS_STROKE = "rgba(157,175,58,.55)"
BADGE_GOLD = "#F5CD82"
BADGE_TOP, BADGE_BOTTOM, BADGE_STROKE = "#5C4227", "#402C15", "#120B05"

JOBS = [
    ("Villager", None, ["The default job — lives in the settlement", "and fights in its defense."]),
    ("Lumberjack", None, ["2–4 wood per work tick into the", "nearest chest with room."]),
    ("Farmer", "beehive for honey", ["1–2 carrots or turnips per tick;", "20% chance of a honey."]),
    ("Builder", "workbench", ["Builds ordered blueprints from the supply", "chest; otherwise repairs up to 3 damaged pieces."]),
    ("Blacksmith", "forge", ["One smelt per tick: copper ore, tin ore,", "iron scraps — or wood into coal."]),
    ("Guard", None, ["No production; 60% wider alert range,", "holds position at the settlement."]),
    ("Cook", "cooking station", ["One cook per tick: raw meats and fish", "into their cooked forms."]),
    ("Miner", None, ["2–4 stone per tick; 15% chance of", "a copper or tin ore."]),
    ("Hunter", None, ["1–2 raw meat per tick; 40% deer hide,", "20% chance of feathers."]),
    ("Brewer", "fermenter", ["2 honey → minor healing mead,", "or 2 barley → barley wine."]),
    ("Courier", "2nd settlement", ["Hauls up to 8 surplus goods to another", "settlement on foot; can be ambushed."]),
    ("Herder", "tamed animals", ["Feeds pen animals from your chests,", "culls the herd above 4, tidies drops."]),
]

W = 1000
PAD_X = 60
CARD_W = (W - 2 * PAD_X - 12) // 2  # 434
CARD_H = 96
GAP = 12
GRID_TOP = 128
FOOTER_GAP = 26

rows = (len(JOBS) + 1) // 2
H = GRID_TOP + rows * (CARD_H + GAP) - GAP + FOOTER_GAP + 52

parts = []
parts.append(
    f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}" '
    f'font-family="Georgia, \'Times New Roman\', serif">')
parts.append(
    f'<defs><linearGradient id="bg" x1="0" y1="0" x2="0" y2="1">'
    f'<stop offset="0" stop-color="{BG_TOP}"/><stop offset="1" stop-color="{BG_BOTTOM}"/></linearGradient>'
    f'<linearGradient id="badge" x1="0" y1="0" x2="0" y2="1">'
    f'<stop offset="0" stop-color="{BADGE_TOP}"/><stop offset="1" stop-color="{BADGE_BOTTOM}"/></linearGradient></defs>')
parts.append(f'<rect width="{W}" height="{H}" fill="url(#bg)" stroke="{CARD_STROKE}"/>')

parts.append(
    f'<text x="{W / 2}" y="62" text-anchor="middle" font-size="28" font-weight="bold" '
    f'fill="{IVORY}" letter-spacing="3">THE TWELVE SETTLER JOBS</text>')
parts.append(
    f'<text x="{W / 2}" y="90" text-anchor="middle" font-size="14" fill="{MIST}" '
    f'font-family="\'Segoe UI\', Arial, sans-serif">'
    'What each job does every work tick, and the workstation it needs first</text>')

for i, (name, needs, body) in enumerate(JOBS):
    col, row = i % 2, i // 2
    x = PAD_X + col * (CARD_W + GAP)
    y = GRID_TOP + row * (CARD_H + GAP)
    parts.append(
        f'<rect x="{x}" y="{y}" width="{CARD_W}" height="{CARD_H}" rx="8" '
        f'fill="{CARD}" stroke="{CARD_STROKE}"/>')
    bx, by = x + 18, y + 18
    parts.append(
        f'<rect x="{bx}" y="{by}" width="40" height="40" rx="6" '
        f'fill="url(#badge)" stroke="{BADGE_STROKE}"/>')
    parts.append(
        f'<text x="{bx + 20}" y="{by + 26}" text-anchor="middle" font-size="16" '
        f'font-weight="bold" fill="{BADGE_GOLD}">{name[:2]}</text>')
    tx = bx + 40 + 14
    parts.append(
        f'<text x="{tx}" y="{y + 34}" font-size="17" font-weight="bold" '
        f'fill="{GOLD}" letter-spacing="1">{html.escape(name)}</text>')
    if needs:
        chip_x = tx + 12 + len(name) * 11
        chip_w = 52 + len(needs) * 6.2
        parts.append(
            f'<rect x="{chip_x}" y="{y + 20}" width="{chip_w:.0f}" height="19" rx="4" '
            f'fill="none" stroke="{MOSS_STROKE}"/>')
        parts.append(
            f'<text x="{chip_x + 8}" y="{y + 33.5}" font-size="11.5" fill="{MOSS}" '
            f'font-family="\'Segoe UI\', Arial, sans-serif">needs {html.escape(needs)}</text>')
    for j, line in enumerate(body):
        parts.append(
            f'<text x="{tx}" y="{y + 56 + j * 19}" font-size="13" fill="{BODY}" '
            f'font-family="\'Segoe UI\', Arial, sans-serif">{html.escape(line)}</text>')

fy = GRID_TOP + rows * (CARD_H + GAP) - GAP + FOOTER_GAP + 6
parts.append(
    f'<text x="{W / 2}" y="{fy}" text-anchor="middle" font-size="12" fill="{MIST}" '
    f'font-family="\'Segoe UI\', Arial, sans-serif">'
    'Producers need a chest with room inside the settlement radius (32 m) — smelting, cooking and brewing need '
    'their input and space for the output in the same chest.</text>')
parts.append(
    f'<text x="{W / 2}" y="{fy + 20}" text-anchor="middle" font-size="12" fill="{MIST}" '
    f'font-family="\'Segoe UI\', Arial, sans-serif">'
    'Settlers eat one food item from your chests about once per game day; hungry settlers stop working, '
    'homeless ones work at half speed. Press T on a settler to see what they need.</text>')
parts.append('</svg>')

out = pathlib.Path(__file__).resolve().parent.parent / "docs" / "jobs.svg"
out.write_text("\n".join(parts), encoding="utf-8")
print(f"wrote {out}")
