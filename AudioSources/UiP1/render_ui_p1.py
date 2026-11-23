"""Render the original Card_Game UI P1 library and a single listening reel."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import sys

import numpy as np

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sys.path.insert(0, str(HERE.parent / "GameplayP1"))
from render_gameplay_p1 import Clip, GAME_RATE, RATE, write

CUES = (
    ("UiConfirm", "ui-confirm", .16, .88),
    ("UiCancel", "ui-cancel", .12, .82),
    ("UiDenied", "ui-denied", .145, .80),
    ("UiModalOpen", "ui-modal-open", .20, .83),
    ("UiModalClose", "ui-modal-close", .14, .79),
    ("UiCardSelect", "ui-card-select", .105, .84),
    ("UiDeckSelect", "ui-deck-select", .155, .88),
    ("UiSettingsPreview", "ui-settings-preview", .135, .86),
)


def render(cue: str, seconds: float, seed: int) -> np.ndarray:
    clip = Clip(seconds, seed)
    if cue == "UiConfirm":
        clip.bell(.004, .103, 590, .41)
        clip.bell(.044, .116, 884, .60)
        clip.noise(0, .051, 650, 2600, .20, .004, 5)
    elif cue == "UiCancel":
        clip.noise(0, .10, 410, 2800, .38, .008, 3.5)
        clip.sweep(.009, .106, 537, 263, .60, .011, 3.8, .12)
    elif cue == "UiDenied":
        for when, pitch, level in ((0, 375, .7), (.054, 263, .56)):
            clip.sweep(when, .075, pitch, pitch * .65, level, .005, 4.2, .24)
            clip.noise(when, .05, 450, 2180, level * .38, .004, 6)
    elif cue == "UiModalOpen":
        clip.noise(0, .182, 450, 3470, .44, .032, 2.9)
        clip.sweep(.012, .175, 203, 665, .50, .021, 3.7, .12)
        clip.bell(.062, .130, 708, .20)
    elif cue == "UiModalClose":
        clip.noise(0, .12, 430, 2960, .43, .011, 3.2)
        clip.sweep(.009, .115, 420, 187, .44, .012, 3.6, .17)
        clip.sweep(.054, .083, 195, 111, .36, .006, 4)
    elif cue == "UiCardSelect":
        clip.noise(0, .091, 830, 5800, .70, .009, 3.0, 35)
        clip.sweep(.029, .061, 1150, 525, .32, .004, 4.5)
    elif cue == "UiDeckSelect":
        clip.noise(0, .129, 430, 3450, .55, .012, 2.5, 29)
        clip.sweep(.026, .10, 294, 148, .61, .006, 4.3, .26)
        clip.sweep(.058, .092, 205, 117, .50, .005, 4.8, .21)
    else:
        clip.sweep(.004, .125, 533, 533, .67, .013, 2.9, .06)
        clip.bell(.041, .091, 667, .22)
    return clip.finish(-16 if cue == "UiConfirm" else -18)


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--preview-dir", required=True, type=Path)
    args = parser.parse_args()
    pieces = []
    manifest = []
    gap = np.zeros(round(.36 * GAME_RATE))
    for index, (cue, name, seconds, volume) in enumerate(CUES):
        master = render(cue, seconds, 8919 + index * 307)
        runtime = np.interp(np.arange(round(seconds * GAME_RATE)) * RATE / GAME_RATE, np.arange(len(master)), master)
        master_path = HERE / "Masters" / (name + ".wav")
        runtime_path = ROOT / "Assets/Audio/SFX/UI/P1" / (name + ".wav")
        write(master_path, master, RATE, "PCM_24")
        write(runtime_path, runtime, GAME_RATE, "PCM_16")
        if pieces:
            pieces.append(gap)
        manifest.append({"cue": cue, "file": name, "seconds": seconds, "volume": volume,
                         "preview_seconds": round(sum(map(len, pieces)) / GAME_RATE, 3),
                         "peak_dbfs": round(20*np.log10(np.max(np.abs(runtime))), 2),
                         "master_sha256": sha(master_path), "runtime_sha256": sha(runtime_path)})
        pieces.append(runtime * volume)
    write(args.preview_dir / "Card_Game-UI-Audio-P1.wav", np.concatenate(pieces), GAME_RATE, "PCM_16")
    (HERE / "render-manifest.json").write_text(json.dumps(manifest, indent=2)+"\n", encoding="utf-8")
    print(f"Rendered {len(manifest)} UI clips.")


if __name__ == "__main__":
    main()
