"""Render the original Card_Game Music P0 score. Requires Python 3.11+ and NumPy."""
from __future__ import annotations

import argparse
import hashlib
import json
import math
from pathlib import Path
import wave

import numpy as np

RATE = 48_000
ROOT = Path(__file__).resolve().parents[2]
SCORE_DIR = Path(__file__).resolve().parent
TRACKS = {
    "music.menu.loop": (True, -10.0),
    "music.match.loop": (True, -11.0),
    "music.result.victory": (False, -5.0),
    "music.result.defeat": (False, -7.0),
}

# MIDI note numbers. Each cell contains a bass and an open upper voicing.
HARMONY = {
    "Dm": (38, (57, 62, 65, 69, 76)),
    "DmC": (36, (57, 62, 65, 69, 74)),
    "Bb": (34, (53, 57, 62, 65, 76)),
    "Gm": (43, (58, 62, 65, 69, 74)),
    "F": (41, (57, 60, 64, 67, 72)),
    "Csus": (36, (55, 60, 62, 67, 74)),
    "Asus": (33, (55, 57, 62, 64, 74)),
    "BbD": (38, (53, 58, 62, 65, 72)),
    "Fadd": (41, (53, 60, 65, 67, 69)),
}
# The core phrase uses D, F, E, A. Both stingers answer that phrase.
MOTIF = ((0.0, 62, 1.15), (1.5, 65, 0.65), (2.35, 64, 0.85), (3.5, 69, 1.55))
ANSWER = ((0.5, 67, 1.4), (2.05, 65, 0.72), (3.0, 64, 1.0), (4.3, 62, 1.45))

# Relative partial strengths of synthesized instruments. No sampled instruments.
SPECTRA = {
    "cello": (1.0, .62, .38, .25, .125, .082, .062, .041, .030, .016, .010),
    "strings": (1.0, .55, .32, .21, .12, .064, .050, .032, .019, .014),
    "viola": (1.0, .48, .30, .18, .14, .076, .053, .034, .025, .016),
    "reed": (1.0, .20, .30, .059, .077, .033, .026),
    "horn": (1.0, .74, .30, .19, .08, .05, .028, .011),
}

ROOM = (
    (.021, .14, False), (.033, -.08, True), (.052, .107, True), (.078, .090, False),
    (.107, .080, True), (.139, .063, False), (.183, -.055, True), (.226, .054, False),
    (.289, .046, True), (.352, .040, True), (.441, -.033, False), (.536, .026, True),
    (.628, .021, False), (.747, .015, True), (.859, .011, False), (1.014, .008, True),
)


class Score:
    def __init__(self, seconds: float, looping: bool, seed: int):
        self.mix = np.zeros((round(seconds * RATE), 2), dtype=np.float32)
        self.looping = looping
        self.random = np.random.default_rng(seed)

    def place(self, when: float, signal: np.ndarray, level: float, pan: float) -> None:
        offset = round(when * RATE)
        angle = (pan + 1) * math.pi / 4
        stereo = signal[:, None] * np.array([math.cos(angle), math.sin(angle)]) * level
        if self.looping:
            offset %= len(self.mix)
        count = min(len(signal), len(self.mix) - offset)
        self.mix[offset:offset + count] += stereo[:count]
        if self.looping and count < len(signal):
            self.mix[:len(signal) - count] += stereo[count:]

    def bowed(self, when: float, duration: float, pitch: int, level: float, pan: float,
              voice: str, attack: float, release: float) -> None:
        freq = 440 * 2 ** ((pitch - 69) / 12)
        t = np.arange(round((duration + release) * RATE), dtype=np.float64) / RATE
        phase = self.random.uniform(0, math.tau)
        vibrato_rate = self.random.uniform(3.7, 5.3)
        vibrato_gain = .024 if voice == "viola" else .012
        drift = np.sin(math.tau * .43 * t + phase) * .063
        vibrato = np.sin(math.tau * vibrato_rate * t + phase) * vibrato_gain * freq / vibrato_rate
        wave_phase = math.tau * freq * t + vibrato + drift
        signal = np.zeros(len(t), dtype=np.float64)
        for harmonic, weight in enumerate(SPECTRA[voice], 1):
            if freq * harmonic >= 10_000:
                break
            breath = 1 + .085 * np.sin(math.tau * (.55 + .058 * harmonic) * t + phase * harmonic)
            signal += weight * breath * np.sin(wave_phase * harmonic + phase * harmonic ** .84)
        envelope = np.sin(np.clip(t / attack, 0, 1) * math.pi / 2) ** 1.35
        envelope *= .9 + .1 * np.sin(math.tau * .37 * t + phase)
        envelope *= (.5 + .5 * np.cos(np.clip((t - duration) / release, 0, 1) * math.pi))
        self.place(when, signal * envelope / 1.8, level, pan)

    def pluck(self, when: float, pitch: int, level: float, pan: float, decay: float = .66) -> None:
        freq = 440 * 2 ** ((pitch - 69) / 12)
        t = np.arange(round(3.3 * decay * RATE), dtype=np.float64) / RATE
        signal = np.zeros(len(t), dtype=np.float64)
        for ratio, gain, loss in ((1, 1, 1), (2.001, .32, 1.7), (3.006, .15, 2.45),
                                  (4.012, .105, 4), (5.04, .082, 6), (8.17, .05, 10)):
            phase = self.random.uniform(0, math.tau)
            signal += gain * np.exp(-loss * t / decay) * np.sin(math.tau * freq * ratio * t + phase)
        edges = (1 - np.exp(-t / .008)) * np.clip((t[-1] - t) / .14, 0, 1) ** 2
        self.place(when, signal * edges / 1.5, level, pan)

    def pulse(self, when: float, level: float, pan: float = 0) -> None:
        t = np.arange(round(.78 * RATE), dtype=np.float64) / RATE
        phase = math.tau * (44 * t + 2.8 * (1 - np.exp(-t * 14)))
        signal = np.sin(phase) * np.exp(-t * 6.7)
        for freq in (79, 113, 147, 211):
            signal += .13 * np.sin(math.tau * freq * t + self.random.uniform(0, math.tau)) * np.exp(-t * 12)
        edges = (1 - np.exp(-t / .028)) * np.clip((t[-1] - t) / .20, 0, 1) ** 2
        self.place(when, signal * edges, level, pan)

    def frame_drum(self, when: float, level: float, pan: float, pitch: float) -> None:
        t = np.arange(round(.56 * RATE), dtype=np.float64) / RATE
        sweep = .035 * pitch * (1 - np.exp(-t * 28))
        body = np.sin(math.tau * (pitch * t + sweep)) * np.exp(-t * 9)
        body += .27 * np.sin(math.tau * (1.58 * pitch * t + sweep)) * np.exp(-t * 17)
        skin = self.random.standard_normal(len(t) + 96)
        rim = np.convolve(skin, np.hanning(33) / 16, mode="valid")[32:-32]
        air = np.convolve(skin, np.hanning(97) / 48, mode="valid")
        body += .58 * (rim - air) * np.exp(-t * 33)
        edges = np.minimum(t / .012, 1) ** 1.5
        edges *= np.clip((t[-1] - t) / .12, 0, 1) ** 2
        self.place(when, body * edges, level, pan)

    def master(self, peak_db: float) -> np.ndarray:
        # Reflections use periodic convolution for loops, including the tail before sample zero.
        dry = self.mix.copy()
        soft = (dry + np.roll(dry, 2, axis=0) + np.roll(dry, 4, axis=0) + np.roll(dry, 6, axis=0)) * .25
        if self.looping:
            dry -= np.mean(dry, axis=0)
        else:
            soft[:6] = 0
        wet = dry.copy()
        for seconds, gain, swap in ROOM:
            delay = round(seconds * RATE)
            reflection = soft[:, ::-1] if swap else soft
            wet[delay:] += gain * reflection[:-delay]
            if self.looping:
                wet[:delay] += gain * reflection[-delay:]
        if not self.looping:
            edge = np.sin(np.linspace(0, math.pi / 2, round(.025 * RATE))) ** 2
            wet[:len(edge)] *= edge[:, None]
            tail = np.cos(np.linspace(0, math.pi / 2, round(.35 * RATE))) ** 2
            wet[-len(tail):] *= tail[:, None]
        return wet * (10 ** (peak_db / 20) / np.max(np.abs(wet)))


def chord(score: Score, start: float, beat: float, name: str, strength: float) -> None:
    bass, voices = HARMONY[name]
    score.bowed(start, 3.76 * beat, bass, .067 * strength, -.054, "cello", .66, 1.15)
    for index, pitch in enumerate(voices):
        pan = (-.41, .41, -.26, .28, .08)[index]
        level = (.024, .023, .021, .018, .012)[index] * strength
        score.bowed(start + index * .025, 3.62 * beat, pitch, level, pan, "strings", .54 + index * .10, 1.22)


def phrase(score: Score, when: float, beat: float, notes: tuple, level: float, voice: str,
           transpose: int = 0) -> None:
    for offset, pitch, sustain in notes:
        score.bowed(when + offset * beat, sustain * beat, pitch + transpose, level,
                    -.17, voice, .18, .47)
        if voice == "reed":
            score.pluck(when + offset * beat + .029, pitch + transpose + 12, level * .32, .28, .83)


def menu() -> np.ndarray:
    beat = 60 / 72
    harmony = (
        "Dm", "DmC", "Bb", "Gm", "Dm", "F", "Gm", "Asus",
        "Bb", "F", "Csus", "Dm", "Gm", "BbD", "F", "Asus",
        "Dm", "DmC", "Bb", "Gm", "Bb", "F", "Csus", "Asus",
        "Gm", "BbD", "F", "Dm", "Bb", "Gm", "Asus", "Dm",
    )
    score = Score(len(harmony) * 4 * beat, True, 71955)
    for bar, name in enumerate(harmony):
        start = bar * 4 * beat
        chord(score, start, beat, name, .87 + .12 * math.sin(bar * math.tau / 16 + .55))
        _, notes = HARMONY[name]
        if bar % 4 != 3:
            score.pluck(start + 2.75 * beat, notes[bar % 3] + 12, .019, .43 if bar % 2 else -.43)
        if bar % 8 in (1, 5):
            score.pluck(start + .55 * beat, notes[2] + 12, .032, .36, .89)
    for bar in (0, 16):
        phrase(score, bar * 4 * beat + .25 * beat, beat, MOTIF, .055, "viola")
        phrase(score, (bar + 4) * 4 * beat, beat, ANSWER, .041, "reed")
    alternate = ((0, 70, 1.35), (1.65, 69, 1), (3, 67, 1.8), (5.15, 64, 1.0))
    phrase(score, 8 * 4 * beat + .5 * beat, beat, alternate, .049, "viola")
    phrase(score, 24 * 4 * beat + .25 * beat, beat, alternate, .04, "reed", -12)
    phrase(score, 28 * 4 * beat + .35 * beat, beat, ANSWER, .037, "viola")
    return score.master(TRACKS["music.menu.loop"][1])


def match() -> np.ndarray:
    # Four dotted-quarter beats per bar. The short bows and drums establish
    # the triple subdivision immediately, before the opening horn phrase.
    eighth = 60 / 100 / 3
    chords = {
        "Gm": (43, 46, 50), "Ab": (44, 48, 51), "Eb": (39, 43, 46),
        "Bb": (46, 50, 53), "F": (41, 45, 48), "Cm": (36, 39, 43), "D": (38, 42, 45),
    }
    sections = (
        ("Gm", "Gm", "Eb", "D"), ("Gm", "Ab", "Eb", "D"),
        ("Gm", "Bb", "F", "D"), ("Cm", "Eb", "Gm", "D"),
        ("Gm", "Eb", "Bb", "D"), ("Cm", "Ab", "Gm", "D"),
        ("Eb", "Cm", "Gm", "D"), ("Gm", "Gm", "Ab", "D"),
        ("Gm", "Bb", "Eb", "F"), ("Cm", "Gm", "Ab", "D"),
        ("Gm", "Eb", "Cm", "D"), ("Gm", "Ab", "Eb", "D"),
        ("Gm", "Eb", "D", "Gm"),
    )
    score = Score(len(sections) * 48 * eighth, True, 42687)
    for section, progression in enumerate(sections):
        reduced = section in (3, 6, 9)
        for measure, name in enumerate(progression):
            start = (section * 48 + measure * 12) * eighth
            root, third, fifth = chords[name]
            strength = .74 if reduced else .93 + .07 * math.sin(start * .24)
            notes = ((0, root, .112), (2, fifth, .051), (3, root + 12, .085),
                     (5, third, .046), (6, root, .105), (8, fifth, .052),
                     (9, third + 12, .067), (11, fifth, .044))
            for slot, pitch, level in notes:
                if reduced and slot in (5, 8):
                    continue
                score.bowed(start + slot * eighth, .125, pitch, level * strength,
                            -.22 if slot % 3 == 0 else .24, "cello", .019, .112)
                if slot % 3 == 0:
                    score.pluck(start + slot * eighth + .012, pitch, .032 * strength, .23, .23)
            score.frame_drum(start, .098 * strength, -.10, 59)
            score.frame_drum(start + 6 * eighth, .076 * strength, .12, 64)
            for slot in (3, 9):
                score.frame_drum(start + slot * eighth, .039 * strength, .28, 108)
            if measure == 3 and not reduced:
                for slot in (10, 11):
                    score.frame_drum(start + slot * eighth, .032 * strength, -.24, 118)
            if measure in (1, 2):
                score.bowed(start + .17, 1.17, fifth + 12, .025 * strength, .31, "strings", .37, .54)
                for slot, pitch in ((4, third + 24), (7, fifth + 24), (10, root + 24)):
                    score.pluck(start + slot * eighth, pitch, .026 * strength, .48, .39)

        when = section * 48 * eighth
        if not reduced:
            challenge = ((2, 62, 2.6), (6, 55, 1.5), (9, 58, 2.3),
                         (13, 62, 2.4), (17, 60, 1.6), (19, 55, 2.3))
            answer = ((26, 63, 2.2), (30, 58, 2.5), (34, 55, 1.5),
                      (37, 57, 2.1), (41, 54, 2.3), (44, 57, 1.5))
            if section in (0, 4, 7, 11):
                phrase(score, when, eighth, challenge, .061, "horn")
            if section in (0, 2, 7, 10):
                phrase(score, when, eighth, answer, .048, "horn")
        else:
            ward = ((6, 74, 4.2), (16, 75, 2.2), (22, 72, 4.0), (34, 70, 3.0))
            phrase(score, when, eighth, ward, .029, "reed")
    return score.master(TRACKS["music.match.loop"][1])


def result(victory: bool) -> np.ndarray:
    score = Score(3.8 if victory else 3.65, False, 99041 if victory else 99043)
    if victory:
        melody = ((.032, 62, .32), (.35, 65, .30), (.64, 64, .26), (.94, 69, .44), (1.43, 72, .86))
        cadences = ((.018, "Dm", .59), (.63, "Csus", .56), (1.39, "Fadd", 1.38))
        voice = "horn"
    else:
        melody = ((.04, 69, .32), (.38, 65, .31), (.72, 64, .31), (1.06, 62, .84))
        cadences = ((.022, "BbD", .81), (.85, "Gm", .45), (1.25, "Dm", 1.35))
        voice = "viola"
    for when, pitch, length in melody:
        score.bowed(when, length, pitch, .126 if victory else .098, -.14, voice, .073, .47)
        score.pluck(when + .018, pitch + 12, .033 if victory else .012, .32, .61)
    for when, name, duration in cadences:
        bass, notes = HARMONY[name]
        score.bowed(when, duration, bass, .105, -.045, "cello", .15, .64)
        for i, note in enumerate(notes[:4]):
            score.bowed(when + i * .014, duration, note, .036, (-.34, .35, -.20, .24)[i],
                        "strings", .17 + i * .023, .68)
    if victory:
        score.pulse(1.4, .069)
    return score.master(TRACKS["music.result.victory" if victory else "music.result.defeat"][1])


def write_wave(path: Path, audio: np.ndarray, depth: int) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    scale = 2 ** (depth - 1)
    pcm = np.clip(np.rint(audio * scale), -scale, scale - 1)
    if depth == 24:
        payload = np.asarray(pcm, dtype="<i4").view(np.uint8).reshape(-1, 4)[:, :3].tobytes()
    elif depth == 16:
        payload = pcm.astype("<i2").tobytes()
    else:
        raise ValueError(depth)
    with wave.open(str(path), "wb") as file:
        file.setparams((2, depth // 8, RATE, 0, "NONE", "not compressed"))
        file.writeframes(payload)


def clip_preview(audio: np.ndarray, start: float, duration: float) -> np.ndarray:
    clip = audio[round(start * RATE):round((start + duration) * RATE)].copy()
    edge = np.sin(np.linspace(0, math.pi / 2, round(.18 * RATE))) ** 2
    clip[:len(edge)] *= edge[:, None]
    clip[-len(edge):] *= edge[::-1, None]
    return clip


def describe(audio: np.ndarray, looping: bool) -> dict:
    stats = {
        "seconds": round(len(audio) / RATE, 5),
        "frames": len(audio),
        "sample_peak_dbfs": round(20 * math.log10(np.max(np.abs(audio))), 2),
        "rms_dbfs": round(20 * math.log10(np.sqrt(np.mean(np.square(audio)))), 2),
    }
    if looping:
        jump = np.max(np.abs(audio[0] - audio[-1]))
        sides = (audio[-4800:], audio[:4800])
        difference = max(np.max(np.abs(np.diff(side, axis=0))) for side in sides)
        stats["seam_delta_dbfs"] = round(20 * math.log10(max(jump, 1e-12)), 2)
        stats["local_delta_max_dbfs"] = round(20 * math.log10(max(difference, 1e-12)), 2)
    return stats


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--preview-dir", type=Path, required=True)
    parser.add_argument("--preview-only", action="store_true")
    args = parser.parse_args()
    previews = []
    metrics = {}
    for (track, (looping, _)), render in zip(TRACKS.items(), (menu, match, lambda: result(True), lambda: result(False))):
        audio = render()
        metrics[track] = describe(audio, looping)
        if not args.preview_only:
            master = SCORE_DIR / "Masters" / (track + ".wav")
            runtime = ROOT / "Assets" / "Audio" / "Music" / (track + ".wav")
            write_wave(master, audio, 24)
            write_wave(runtime, audio, 16)
            metrics[track]["master_sha256"] = hashlib.sha256(master.read_bytes()).hexdigest()
            metrics[track]["runtime_sha256"] = hashlib.sha256(runtime.read_bytes()).hexdigest()
        previews.append(clip_preview(audio, 0.0, 12.4) if looping else audio.copy())
        previews.append(np.zeros((round(.48 * RATE), 2), dtype=np.float32))
        print(track, metrics[track], flush=True)
    write_wave(args.preview_dir / "Card_Game-Music-P0-Preview.wav", np.concatenate(previews[:-1]), 16)
    (args.preview_dir / "music-metrics.json").write_text(json.dumps(metrics, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
