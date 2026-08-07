#!/usr/bin/env python3
"""Generates the Thunderstore package icon.

Writes docs/icon.svg (the editable source) and, when a Chromium binary is
available, renders it to VikingSettlements/Package/icon.png at the 256x256
Thunderstore requires.

    python3 scripts/make-icon.py

Shares the palette of docs/features.svg so the mod's artwork reads as one set.
"""
import os
import shutil
import subprocess
import sys

SVG = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256" width="256" height="256" role="img">
<title>VikingSettlements</title>
<defs>
  <linearGradient id="bg" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0" stop-color="#182228"/>
    <stop offset="1" stop-color="#0a0f12"/>
  </linearGradient>
  <radialGradient id="hearth" cx="0.5" cy="0.72" r="0.55">
    <stop offset="0" stop-color="#e8a33d" stop-opacity="0.38"/>
    <stop offset="1" stop-color="#e8a33d" stop-opacity="0"/>
  </radialGradient>
  <linearGradient id="roof" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0" stop-color="#f5cd82"/>
    <stop offset="1" stop-color="#dc9a37"/>
  </linearGradient>
  <linearGradient id="door" x1="0" y1="0" x2="0" y2="1">
    <stop offset="0" stop-color="#ffd489"/>
    <stop offset="1" stop-color="#e8952b"/>
  </linearGradient>
</defs>

<rect width="256" height="256" fill="url(#bg)"/>
<rect width="256" height="256" fill="url(#hearth)"/>

<!-- Flanking huts, dimmed so the longhouse stays the focal point. -->
<g opacity="0.5" fill="#c98a33">
  <path d="M18 150 L52 116 L86 150 Z"/>
  <rect x="30" y="148" width="44" height="42"/>
  <path d="M170 150 L204 116 L238 150 Z"/>
  <rect x="182" y="148" width="44" height="42"/>
</g>

<!-- Central longhouse. -->
<path d="M40 136 L128 52 L216 136 Z" fill="url(#roof)"/>
<rect x="62" y="132" width="132" height="60" fill="#dc9a37"/>
<!-- Eaves shadow separates roof from wall at small sizes. -->
<rect x="62" y="132" width="132" height="5" fill="#0a0f12" opacity="0.35"/>

<!-- Lit doorway. -->
<path d="M110 192 v-28 a18 18 0 0 1 36 0 v28 Z" fill="url(#door)"/>
<path d="M116 192 v-26 a12 12 0 0 1 24 0 v26 Z" fill="#0a0f12" opacity="0.55"/>

<!-- Ridge beam ends, a Norse roofline flourish. -->
<path d="M128 52 l-13 -14 M128 52 l13 -14" stroke="#f5cd82" stroke-width="7" stroke-linecap="round" fill="none"/>

<!-- Ground. -->
<rect x="14" y="190" width="228" height="7" rx="3.5" fill="#e8a33d" opacity="0.85"/>

<!-- Palisade stakes. -->
<g fill="#c98a33" opacity="0.75">
  <path d="M26 204 l6 -10 6 10 Z"/><path d="M50 204 l6 -10 6 10 Z"/>
  <path d="M74 204 l6 -10 6 10 Z"/><path d="M98 204 l6 -10 6 10 Z"/>
  <path d="M122 204 l6 -10 6 10 Z"/><path d="M146 204 l6 -10 6 10 Z"/>
  <path d="M170 204 l6 -10 6 10 Z"/><path d="M194 204 l6 -10 6 10 Z"/>
  <path d="M218 204 l6 -10 6 10 Z"/>
</g>
</svg>
"""

CHROMIUM_CANDIDATES = [
    "/opt/pw-browsers/chromium_headless_shell-1194/chrome-linux/headless_shell",
    "chromium", "chromium-browser", "google-chrome",
]


def find_chromium():
    for c in CHROMIUM_CANDIDATES:
        if os.path.isfile(c):
            return c
        found = shutil.which(c)
        if found:
            return found
    return None


def main():
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    svg_path = os.path.join(root, "docs", "icon.svg")
    png_path = os.path.join(root, "VikingSettlements", "Package", "icon.png")
    os.makedirs(os.path.dirname(svg_path), exist_ok=True)

    with open(svg_path, "w", encoding="utf-8") as f:
        f.write(SVG)
    print(f"wrote {svg_path}")

    chromium = find_chromium()
    if not chromium:
        print("No Chromium found - render docs/icon.svg to a 256x256 PNG yourself "
              f"and save it as {png_path}", file=sys.stderr)
        return 1

    subprocess.run([
        chromium, "--headless", "--disable-gpu", "--no-sandbox", "--hide-scrollbars",
        "--force-device-scale-factor=1", "--window-size=256,256",
        "--default-background-color=00000000",
        f"--screenshot={png_path}", svg_path,
    ], check=True, capture_output=True)
    print(f"wrote {png_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
