"""Synthesize Card_Game Gameplay P1 from an original, seeded score."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
import soundfile as sf

RATE = 48000
GAME_RATE = 44100
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
CLIPS = (
    ("Abilities/ability-epic", "AbilityEpic", .49, -8, .86),
    ("Abilities/ability-epic-02", "AbilityEpic", .55, -8, .86),
    ("Abilities/ability-legendary", "AbilityLegendary", .81, -7, .82),
    ("Abilities/ability-legendary-02", "AbilityLegendary", .91, -7, .82),
    ("Abilities/ability-cancel", "AbilityCancel", .31, -10, .76),
    ("Elements/element-fire", "ElementFire", .29, -10, .50),
    ("Elements/element-water", "ElementWater", .32, -10, .50),
    ("Elements/element-earth", "ElementEarth", .36, -10, .50),
    ("Elements/element-wind", "ElementWind", .26, -10, .50),
    ("Elements/element-light", "ElementLight", .32, -10, .50),
    ("Elements/element-dark", "ElementDark", .36, -10, .50),
    ("Cards/creature-shift", "CreatureShift", .24, -10, .82),
    ("Cards/card-return", "CardReturn", .19, -11, .76),
    ("Battle/battle-retreat", "BattleRetreat", .34, -10, .78),
    ("Battle/turn-remote", "TurnRemote", .19, -12, .68),
    ("Battle/battle-draw", "BattleDraw", .45, -9, .82),
)


class Clip:
    def __init__(self, seconds: float, seed: int):
        self.samples = np.zeros(round(RATE * seconds), dtype=np.float64)
        self.random = np.random.default_rng(seed)

    @staticmethod
    def time(seconds: float) -> np.ndarray:
        return np.arange(round(seconds * RATE)) / RATE

    def add(self, onset: float, tone: np.ndarray, gain: float) -> None:
        start = round(onset * RATE)
        end = min(start + len(tone), len(self.samples))
        release = np.clip((len(tone) - np.arange(end-start) - 1) / (.009 * RATE), 0, 1)
        self.samples[start:end] += tone[:end-start] * gain * release

    def sweep(self, onset: float, seconds: float, start: float, end: float, gain: float,
              attack: float = .012, decay: float = 3.0, harmonics: float = .18) -> None:
        t = self.time(seconds)
        phase = 2 * np.pi * t * (start + (end - start) * t / (2 * seconds))
        wave = np.sin(phase) + harmonics * np.sin(2 * phase + .3) + harmonics * .43 * np.sin(3.02 * phase + .7)
        env = np.minimum(t / attack, 1) ** 1.5 * np.exp(-decay * t / seconds)
        self.add(onset, wave * env, gain)

    def noise(self, onset: float, seconds: float, low: float, high: float, gain: float,
              attack: float, decay: float = 2.5, flutter: float = 0) -> None:
        t = self.time(seconds)
        freqs = np.fft.rfftfreq(len(t), 1 / RATE)
        white = self.random.standard_normal(len(t))
        window = (freqs / np.maximum(freqs + low, 1)) ** 2 / (1 + (freqs / high) ** 5)
        layer = np.fft.irfft(np.fft.rfft(white) * window, n=len(t))
        layer /= max(np.std(layer) * 2.8, .001)
        env = np.minimum(t / attack, 1) ** 1.5 * np.exp(-decay * t / seconds)
        if flutter:
            env *= .71 + .29 * np.cos(2 * np.pi * t * flutter)
        self.add(onset, layer * env, gain)

    def bell(self, onset: float, seconds: float, frequency: float, gain: float) -> None:
        for ratio, level, decay in ((1, 1, 3.2), (2.02, .32, 4.7), (2.77, .19, 6.1), (4.08, .13, 8.4)):
            self.sweep(onset, seconds, frequency * ratio, frequency * ratio, gain * level, .008, decay, 0)

    def finish(self, peak: float) -> np.ndarray:
        samples = self.samples
        shape = np.sin(np.linspace(0, np.pi / 2, round(.003 * RATE))) ** 2
        samples[:len(shape)] *= shape
        tail = np.sin(np.linspace(0, np.pi / 2, round(.034 * RATE))) ** 2
        samples[-len(tail):] *= tail[::-1]
        return samples * (10 ** (peak / 20) / np.max(np.abs(samples)))


def spell(clip: Clip, name: str) -> None:
    alternative = name.endswith("-02")
    if "legendary" in name:
        hit = .30 if not alternative else .39
        clip.noise(0, hit + .05, 370, 3600, .31, hit * .78, .45, 21 if alternative else 16)
        clip.sweep(0, hit + .04, 83 if alternative else 67, 280, .37, hit * .72, .45)
        clip.sweep(hit, .39, 148, 54, .78, .013, 3.1, .28)
        clip.noise(hit+.012, .36, 250, 1950, .49, .012, 4.2)
        for delay, pitch in ((.015, 390), (.043, 523 if alternative else 587), (.067, 782)):
            clip.bell(hit + delay, .36, pitch, .26)
        return
    if "epic" in name:
        hit = .133 if alternative else .098
        clip.noise(.003, .28, 650, 4200, .42, .055, 2.7, 32)
        clip.sweep(.006, .27, 224 if alternative else 193, 621, .58, .02, 3.5)
        clip.sweep(hit, .31, 488 if alternative else 423, 161, .57, .012, 3.8)
        clip.bell(hit + .054, .28, 901 if alternative else 779, .23)
        return
    clip.noise(0, .145, 850, 4800, .46, .058, .7)
    clip.sweep(.016, .23, 1280, 116, .74, .044, 1.8, .32)
    clip.noise(.136, .144, 430, 3300, .74, .005, 5)
    clip.sweep(.139, .135, 190, 58, .82, .008, 4.4)


def element(clip: Clip, name: str) -> None:
    if name == "fire":
        clip.noise(0, .29, 520, 3450, .65, .023, 2.9, 18)
        for onset, pitch in ((.015, 1400), (.047, 910), (.090, 1610), (.138, 1270), (.191, 1100), (.233, 1800)):
            clip.noise(onset, .043, pitch, 5750, .85, .0015, 6)
        clip.sweep(0, .16, 151, 99, .27)
    elif name == "water":
        clip.noise(0, .32, 650, 3100, .36, .038, 2.1, 24)
        for onset, pitch in ((.019, 480), (.082, 342), (.121, 590), (.162, 470), (.209, 680)):
            clip.sweep(onset, .118, pitch, pitch * 1.78, .50, .011, 4.4, .075)
    elif name == "earth":
        clip.sweep(.017, .34, 125, 45, .81, .018, 3.1, .38)
        clip.noise(.007, .25, 170, 1300, .77, .010, 3.6, 19)
        clip.noise(.081, .18, 280, 1580, .42, .006, 5.2)
        clip.sweep(.103, .24, 78, 51, .24, .03, 3)
    elif name == "wind":
        clip.noise(0, .26, 410, 3700, .89, .065, 2.5, 9)
        clip.noise(.037, .20, 1280, 6600, .46, .049, 4.8)
        clip.sweep(.025, .23, 420, 655, .105, .09, 2.2, .05)
    elif name == "light":
        for onset, pitch, gain in ((0, 622, .42), (.032, 831, .37), (.058, 1047, .33), (.106, 1245, .16)):
            clip.bell(onset, .26, pitch, gain)
        clip.noise(.042, .18, 1900, 7100, .16, .041, 3.3)
    else:
        clip.sweep(0, .36, 158, 67, .85, .087, 2.2, .36)
        clip.sweep(.043, .31, 143, 101, .51, .071, 1.9, .24)
        clip.noise(.01, .32, 200, 1780, .35, .131, 1.8, 36)


def action(clip: Clip, name: str) -> None:
    if name == "creature-shift":
        clip.noise(.003, .20, 260, 2550, .79, .025, 2.4, 35)
        clip.sweep(.135, .105, 203, 130, .52, .009, 3.1)
        clip.noise(.147, .08, 560, 2500, .37, .005, 4)
    elif name == "card-return":
        clip.noise(0, .168, 620, 6200, .67, .023, 2.9, 32)
        clip.sweep(.056, .13, 281, 502, .24, .021, 3.7)
        clip.noise(.106, .071, 770, 4000, .32, .004, 3.9)
    elif name == "battle-retreat":
        clip.noise(0, .32, 340, 3300, .60, .03, 2.4, 21)
        clip.sweep(.045, .29, 284, 89, .64, .025, 3.4)
        clip.noise(.171, .157, 300, 1460, .38, .013, 3.5)
    elif name == "turn-remote":
        clip.bell(.009, .178, 367, .74)
        clip.sweep(.011, .153, 180, 164, .21, .013, 3.4)
    else:
        clip.bell(.014, .38, 426, .53)
        clip.bell(.077, .34, 569, .43)
        clip.sweep(.093, .34, 213, 210, .34, .06, 2.8)
        clip.noise(.034, .28, 400, 2900, .15, .072, 2.4)


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write(path: Path, samples: np.ndarray, rate: int, subtype: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    sf.write(path, samples, rate, subtype=subtype)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--preview-dir", type=Path, required=True)
    args = parser.parse_args()
    preview, entries = [], []
    previous_group = None
    for index, (name, cue, seconds, peak, volume) in enumerate(CLIPS):
        group, basename = name.split("/")
        generator = Clip(seconds, 1761 + index * 173)
        if group == "Abilities":
            spell(generator, basename)
        elif group == "Elements":
            element(generator, basename.removeprefix("element-"))
        else:
            action(generator, basename)
        master = generator.finish(peak)
        # The highest synthesis band is below 8 kHz, safely below both Nyquist limits.
        frames = round(seconds * GAME_RATE)
        runtime = np.interp(np.arange(frames) * RATE / GAME_RATE, np.arange(len(master)), master)
        master_file = HERE / "Masters" / (name + ".wav")
        game_file = ROOT / "Assets" / "Audio" / "SFX" / "P1" / (name + ".wav")
        write(master_file, master, RATE, "PCM_24")
        write(game_file, runtime, GAME_RATE, "PCM_16")
        if preview:
            preview.append(np.zeros(round(GAME_RATE * (.62 if group != previous_group else .24))))
        start = sum(map(len, preview)) / GAME_RATE
        preview.append(runtime * volume)
        previous_group = group
        entries.append({"file": name, "cue": cue, "preview_seconds": round(start, 3), "seconds": seconds,
                        "peak_dbfs": round(20 * np.log10(np.max(np.abs(runtime))), 2), "volume": volume,
                        "master_sha256": digest(master_file), "runtime_sha256": digest(game_file)})
    write(args.preview_dir / "Card_Game-Gameplay-Audio-P1.wav", np.concatenate(preview), GAME_RATE, "PCM_16")
    (HERE / "render-manifest.json").write_text(json.dumps(entries, indent=2) + "\n", encoding="utf-8")
    print("Rendered", len(entries), "original clips and", round(sum(map(len, preview))/GAME_RATE, 2), "seconds of preview")


if __name__ == "__main__":
    main()
