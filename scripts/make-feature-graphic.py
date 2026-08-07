#!/usr/bin/env python3
"""Generates docs/features.svg - the feature overview graphic used in the README.

The graphic carries its own dark background so it renders identically in
GitHub's light and dark themes. Run after changing the feature list:

    python3 scripts/make-feature-graphic.py
"""
import os
from xml.sax.saxutils import escape

W, H = 1200, 780

C = {
    "bg_top": "#141d22",
    "bg_bot": "#0b1013",
    "card": "#18232a",
    "card_edge": "#27353e",
    "title": "#f0e8d8",
    "text": "#ded5c4",
    "muted": "#8fa0a8",
    "gold": "#e8a33d",
    "gold_lt": "#f5cd82",
    "moss": "#7fb5a4",
    "ember": "#d8735a",
}

SERIF = "Georgia,'Iowan Old Style','Palatino Linotype','Times New Roman',serif"
SANS = "'Segoe UI',-apple-system,Roboto,'Helvetica Neue',Arial,sans-serif"

# Icons are authored on a 24x24 grid and stroked, so they stay crisp at any size.
ICONS = {
    "village": [
        ("path", "M1.6 10.8l5.9-4.9 5.9 4.9"),
        ("path", "M3 10.4v10.1h9.8V10.4"),
        ("path", "M6.2 20.5v-4.1h3.4v4.1"),
        ("path", "M14.2 14.4l4.1-3.3 4.1 3.3"),
        ("path", "M15.2 14v6.5h6.2V14"),
    ],
    "settler": [
        ("circle", "12,7.4,3.4"),
        ("path", "M4.6 20.5v-1.1a7.4 7.4 0 0 1 14.8 0v1.1"),
    ],
    "coin": [
        ("circle", "9.8,12,6.7"),
        ("path", "M13.5 6a6.7 6.7 0 0 1 0 12"),
        ("path", "M9.8 9.1v5.8"),
        ("path", "M7.8 10.7h4"),
    ],
    "recruit": [
        ("circle", "9.4,7.4,3.2"),
        ("path", "M2.6 20.5v-1a6.9 6.9 0 0 1 10.2-6.1"),
        ("path", "M17.6 13.4v7.1M14 17h7.2"),
    ],
    "banner": [
        ("path", "M6.4 2.6v18.9"),
        ("path", "M6.4 4.1h12.6l-3.2 4.2 3.2 4.2H6.4z"),
    ],
    "work": [
        ("path", "M5.5 4.2h13v5.2h-13z"),
        ("path", "M12 9.4v11.2"),
    ],
    "raid": [
        ("path", "M12 1.8c3.6 4.6 6 7 6 10.6a6 6 0 1 1-12 0C6 8.8 8.4 6.4 12 1.8z"),
        ("path", "M12 12.6c1.6 2 2.4 3 2.4 4.3a2.4 2.4 0 1 1-4.8 0c0-1.3.8-2.3 2.4-4.3z"),
    ],
    "swords": [
        ("path", "M4.4 19.9L17.6 6.7"),
        ("path", "M15.2 5.1l4.4-1.6-1.6 4.4"),
        ("path", "M14.6 8.1l2 2"),
        ("path", "M19.6 19.9L6.4 6.7"),
        ("path", "M8.8 5.1L4.4 3.5l1.6 4.4"),
        ("path", "M9.4 8.1l-2 2"),
    ],
    "sliders": [
        ("path", "M3.4 6.6h17.2M3.4 12h17.2M3.4 17.4h17.2"),
        ("circle", "8.6,6.6,2.4"),
        ("circle", "15.4,12,2.4"),
        ("circle", "7.4,17.4,2.4"),
    ],
    # Small icons for the job strip.
    "job_villager": [
        ("circle", "12,8,3"),
        ("path", "M5.8 20v-.9a6.2 6.2 0 0 1 12.4 0v.9"),
    ],
    "job_tree": [
        ("path", "M12 2.4l4.6 6.2h-2.8l3.7 5h-3l3.1 4.6H6.4l3.1-4.6h-3l3.7-5H7.4z"),
        ("path", "M12 18.2v3.2"),
    ],
    "job_wheat": [
        ("path", "M12 21.5V6"),
        ("path", "M12 10.6c-2.8 0-4-1.8-4-4 2.2 0 4 1.2 4 4z"),
        ("path", "M12 10.6c2.8 0 4-1.8 4-4-2.2 0-4 1.2-4 4z"),
        ("path", "M12 15.4c-2.8 0-4-1.8-4-4 2.2 0 4 1.2 4 4z"),
        ("path", "M12 15.4c2.8 0 4-1.8 4-4-2.2 0-4 1.2-4 4z"),
    ],
    "job_hammer": [
        ("path", "M5.8 4.6h12.4v4.8H5.8z"),
        ("path", "M12 9.4v11"),
    ],
    "job_anvil": [
        ("fillpath", "M2.8 8.8h18.4c0 3.1-2.6 4.9-5.8 4.9h-1.3v2.4h3.8V19H6.1v-2.9h3.8v-2.4H8.6C5.2 13.7 2.8 11.9 2.8 8.8z"),
    ],
    "job_shield": [
        ("path", "M12 2.9l7.1 2.8v5c0 4.9-3 8.1-7.1 9.8-4.1-1.7-7.1-4.9-7.1-9.8v-5z"),
        ("path", "M12 8.2v6.4"),
    ],
}

COLUMNS = [
    {
        "num": "I",
        "label": "EXPLORE THE WILD",
        "accent": C["moss"],
        "cards": [
            ("village", "Villages in the Wild",
             ["Three settlement types placed by", "world generation across three biomes."]),
            ("settler", "Named Settlers",
             ["Villagers with lasting names who", "keep to their home and greet you."]),
            ("coin", "Village Traders",
             ["Meadows villages hold a trader", "stocked with early-game supplies."]),
        ],
    },
    {
        "num": "II",
        "label": "BUILD YOUR OWN",
        "accent": C["gold"],
        "cards": [
            ("recruit", "Recruit Villagers",
             ["Pay coins to hire settlers. They", "join your side and follow you home."]),
            ("banner", "Settlement Banner",
             ["A buildable standard that founds", "your village and counts its people."]),
            ("work", "Put Them to Work",
             ["Assign a job to every settler and", "they fill your chests as you play."]),
        ],
    },
    {
        "num": "III",
        "label": "HOLD THE GATES",
        "accent": C["ember"],
        "cards": [
            ("raid", "Native Raid Events",
             ["Your village counts as a base, so", "Valheim's raid system can target it."]),
            ("swords", "Rival Clans",
             ["Bandit war parties strike by night.", "Your settlers fight them off."]),
            ("sliders", "Raids on Your Terms",
             ["Gated behind Eikthyr by default —", "toggle or retune every raid setting."]),
        ],
    },
]

JOBS = [
    ("job_villager", "Villager"),
    ("job_tree", "Lumberjack"),
    ("job_wheat", "Farmer"),
    ("job_hammer", "Builder"),
    ("job_anvil", "Blacksmith"),
    ("job_shield", "Guard"),
]


def icon(name, cx, cy, scale, color, width=1.7):
    """Renders a 24x24 icon centred on (cx, cy)."""
    out = [f'<g transform="translate({cx - 12 * scale:.2f},{cy - 12 * scale:.2f}) scale({scale})" '
           f'fill="none" stroke="{color}" stroke-width="{width / scale:.2f}" '
           f'stroke-linecap="round" stroke-linejoin="round">']
    for kind, data in ICONS[name]:
        if kind == "circle":
            x, y, r = data.split(",")
            out.append(f'<circle cx="{x}" cy="{y}" r="{r}"/>')
        elif kind == "fillpath":
            out.append(f'<path d="{data}" fill="{color}" stroke="none"/>')
        else:
            out.append(f'<path d="{data}"/>')
    out.append("</g>")
    return "".join(out)


def text(x, y, content, size, color, family=SANS, weight="400", anchor="start", spacing=None):
    ls = f' letter-spacing="{spacing}"' if spacing else ""
    return (f'<text x="{x}" y="{y}" font-family="{family}" font-size="{size}" '
            f'font-weight="{weight}" fill="{color}" text-anchor="{anchor}"{ls}>'
            f'{escape(content)}</text>')


def build():
    s = []
    s.append(f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W} {H}" '
             f'width="{W}" height="{H}" role="img" '
             f'aria-labelledby="vs-title vs-desc">')
    s.append('<title id="vs-title">VikingSettlements feature overview</title>')
    s.append('<desc id="vs-desc">Explore the wild: villages placed by world generation, '
             'named settlers, village traders. Build your own: recruit villagers, '
             'settlement banner, put them to work. Hold the gates: native raid events, '
             'rival clans, tunable config. Six settler jobs: villager, lumberjack, '
             'farmer, builder, blacksmith, guard.</desc>')

    # Background, plus a warm glow from above like firelight.
    s.append('<defs>')
    s.append(f'<linearGradient id="bg" x1="0" y1="0" x2="0" y2="1">'
             f'<stop offset="0" stop-color="{C["bg_top"]}"/>'
             f'<stop offset="1" stop-color="{C["bg_bot"]}"/></linearGradient>')
    s.append('<radialGradient id="glow" cx="0.5" cy="0" r="0.75">'
             f'<stop offset="0" stop-color="{C["gold"]}" stop-opacity="0.13"/>'
             f'<stop offset="1" stop-color="{C["gold"]}" stop-opacity="0"/></radialGradient>')
    s.append(f'<linearGradient id="rule" x1="0" y1="0" x2="1" y2="0">'
             f'<stop offset="0" stop-color="{C["gold"]}" stop-opacity="0"/>'
             f'<stop offset="0.5" stop-color="{C["gold"]}" stop-opacity="0.85"/>'
             f'<stop offset="1" stop-color="{C["gold"]}" stop-opacity="0"/></linearGradient>')
    s.append('</defs>')
    s.append(f'<rect width="{W}" height="{H}" fill="url(#bg)"/>')
    s.append(f'<rect width="{W}" height="{H}" fill="url(#glow)"/>')
    s.append(f'<rect x="0.5" y="0.5" width="{W - 1}" height="{H - 1}" fill="none" '
             f'stroke="{C["card_edge"]}" stroke-width="1"/>')

    # ---- Header ----
    s.append(text(W / 2, 74, "VIKING SETTLEMENTS", 46, C["title"], SERIF, "700", "middle", "5"))
    s.append(text(W / 2, 106, "Living NPC villages for Valheim — explore them, recruit them, rule them",
                  15.5, C["muted"], SANS, "400", "middle"))
    s.append(f'<rect x="{W / 2 - 260}" y="127" width="520" height="1.4" fill="url(#rule)"/>')
    s.append(f'<path d="M{W / 2} 121.5l6 6.2-6 6.2-6-6.2z" fill="{C["gold"]}"/>')

    # ---- Three feature columns ----
    margin, gap = 40, 24
    col_w = (W - margin * 2 - gap * 2) / 3
    card_h, card_gap = 96, 16

    for ci, col in enumerate(COLUMNS):
        x = margin + ci * (col_w + gap)
        accent = col["accent"]

        s.append(text(x + 2, 196, f'{col["num"]}', 15, accent, SERIF, "700", "start", "1"))
        s.append(text(x + 30, 196, col["label"], 13, accent, SANS, "700", "start", "2.4"))
        s.append(f'<rect x="{x}" y="208" width="{col_w}" height="1" fill="{accent}" opacity="0.32"/>')

        for ri, (icon_name, title, lines) in enumerate(col["cards"]):
            y = 224 + ri * (card_h + card_gap)
            s.append(f'<rect x="{x}" y="{y}" width="{col_w}" height="{card_h}" rx="7" '
                     f'fill="{C["card"]}" stroke="{C["card_edge"]}" stroke-width="1"/>')
            # Accent spine marks which pillar the card belongs to.
            s.append(f'<rect x="{x}" y="{y}" width="3" height="{card_h}" rx="1.5" '
                     f'fill="{accent}" opacity="0.75"/>')
            s.append(f'<circle cx="{x + 46}" cy="{y + 48}" r="20" fill="{accent}" opacity="0.11"/>')
            s.append(icon(icon_name, x + 46, y + 48, 1.26, accent, 1.8))
            s.append(text(x + 82, y + 38, title, 16.5, C["title"], SERIF, "700"))
            s.append(text(x + 82, y + 60, lines[0], 13, C["text"]))
            s.append(text(x + 82, y + 78, lines[1], 13, C["muted"]))

    # ---- Job strip ----
    strip_y = 570
    s.append(f'<rect x="{margin}" y="{strip_y}" width="{W - margin * 2}" height="118" rx="8" '
             f'fill="{C["card"]}" stroke="{C["card_edge"]}" stroke-width="1"/>')
    s.append(text(W / 2, strip_y + 27, "SIX SETTLER JOBS", 12.5, C["gold"], SANS, "700", "middle", "3"))

    slot = (W - margin * 2) / len(JOBS)
    for i, (icon_name, label) in enumerate(JOBS):
        cx = margin + slot * (i + 0.5)
        s.append(f'<circle cx="{cx}" cy="{strip_y + 62}" r="17" fill="{C["gold"]}" opacity="0.09"/>')
        s.append(icon(icon_name, cx, strip_y + 62, 1.08, C["gold_lt"], 1.65))
        s.append(text(cx, strip_y + 100, label, 12.5, C["text"], SANS, "400", "middle"))
        if i:
            s.append(f'<rect x="{margin + slot * i}" y="{strip_y + 36}" width="1" height="60" '
                     f'fill="{C["card_edge"]}"/>')

    # ---- Footer ----
    s.append(f'<rect x="{margin}" y="716" width="{W - margin * 2}" height="1" '
             f'fill="{C["card_edge"]}"/>')
    s.append(text(W / 2, 742, "Built with Jötunn  ·  No custom assets — assembled from vanilla pieces  "
                              "·  Server-synced config  ·  Multiplayer ready",
                  12.5, C["muted"], SANS, "400", "middle"))
    s.append("</svg>")
    return "\n".join(s)


if __name__ == "__main__":
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    out = os.path.join(root, "docs", "features.svg")
    os.makedirs(os.path.dirname(out), exist_ok=True)
    with open(out, "w", encoding="utf-8") as f:
        f.write(build())
    print(f"wrote {out}")
