"""Render P0 WAVs from Kenney CC0 recordings and original synthesis.

Run from the project root with numpy and soundfile. The credited recordings are in AudioSources/P0/Foley.
"""
from pathlib import Path
from fractions import Fraction

import numpy as np
import soundfile as sf

SR = 48000
OUT = 44100
SOURCE = Path("AudioSources/P0/Foley")
MASTER = Path("AudioSources/P0/Masters")
RUNTIME = Path("Assets/Audio/SFX")

# ID, cue, relative path, seconds, peak dBFS, character / processing.
CLIPS = [
    ("sfx.card.deal", "CardDeal", "Cards/card-deal", .122, -16, "Скольжение, короткий взмах, лёгкая посадка."),
    ("sfx.card.deal", "CardDeal", "Cards/card-deal-02", .112, -16.7, "Более сухое скольжение и высокий край карты."),
    ("sfx.card.deal", "CardDeal", "Cards/card-deal-03", .138, -16.3, "Мягкий взмах и более поздний контакт."),
    ("sfx.card.pick", "CardPick", "Cards/card-pick", .095, -17, "Отрыв и короткий бумажный щелчок."),
    ("sfx.card.place.creature", "CardPlace", "Cards/card-place-creature", .153, -11.5, "Карточный контакт и тёплое дерево."),
    ("sfx.card.place.creature", "CardPlace", "Cards/card-place-creature-02", .171, -12, "Другой удар дерева с более низким резонансом."),
    ("sfx.card.place.field", "FieldPlace", "Cards/card-place-field", .254, -9, "Плотная посадка с каменным отзвуком."),
    ("sfx.card.field.reveal", "FieldFlip", "Cards/card-field-reveal", .335, -11, "Шелест переворота, бронзовые обертоны и вспышка."),
    ("sfx.card.discard", "AbilityDiscard", "Cards/card-discard", .145, -15, "Сухой шелест, приглушённая посадка на стопку."),
    ("sfx.battle.start", "BattleStart", "Battle/battle-start", .726, -6.5, "Удар и медный зов из двух голосов."),
    ("sfx.battle.power.up", "PowerUp", "Battle/battle-power-up", .218, -12, "Яркие восходящие обертоны и серебряный акцент."),
    ("sfx.battle.power.down", "PowerDown", "Battle/battle-power-down", .237, -14, "Приглушённые нисходящие обертоны и рассеивание."),
    ("sfx.battle.win", "BattleWin", "Battle/battle-win", .562, -8, "Два восходящих удара, верхний гармонический отклик."),
    ("sfx.battle.lose", "BattleLose", "Battle/battle-lose", .579, -11, "Мягкий удар, нисходящие фразы и тёмный отклик."),
    ("sfx.battle.creature.out", "CreatureOut", "Battle/battle-creature-out", .309, -14, "Рассеивание и мягкий глухой контакт."),
    ("sfx.battle.field.remove", "FieldRemove", "Battle/battle-field-remove", .409, -12, "Каменное скольжение, удар и короткий резонанс."),
    ("sfx.turn.local", "TurnStart", "Battle/turn-local", .384, -13, "Два ясных резонансных тона."),
    ("sfx.ability.common", "AbilityPlay", "Abilities/ability-common", .267, -15.5, "Мягкий магический щелчок с яркой искрой."),
    ("sfx.ability.common", "AbilityPlay", "Abilities/ability-common-02", .291, -15.8, "Мягкий магический щелчок со сдвоенной искрой."),
    ("ui.click", "Button", "UI/ui-click", .074, -19, "Короткий нейтральный деревянный щелчок."),
]

def gain(db):
    return 10 ** (db / 20)

def db(level):
    return 20 * np.log10(max(float(level), 1e-10))

def peak(wave):
    return float(np.max(np.abs(wave)))

def rms(wave):
    return float(np.sqrt(np.mean(wave * wave)))

def biquad(wave, cutoff, rate, high_pass):
    # Butterworth section with a Q of 1/sqrt(2).
    omega = 2 * np.pi * cutoff / rate
    cosine = np.cos(omega)
    alpha = np.sin(omega) / np.sqrt(2)
    gain_sign = 1 if high_pass else -1
    b0 = float((1 + gain_sign * cosine) / (2 * (1 + alpha)))
    b1 = -2 * gain_sign * b0
    a1 = float(-2 * cosine / (1 + alpha))
    a2 = float((1 - alpha) / (1 + alpha))
    x1 = x2 = y1 = y2 = 0.
    filtered = []
    for x in wave.tolist():
        y = b0 * (x + x2) + b1 * x1 - a1 * y1 - a2 * y2
        filtered.append(y)
        x2, x1, y2, y1 = x1, x, y1, y
    return np.array(filtered)

def band(wave, low, high, rate=SR):
    return biquad(biquad(wave, low, rate, high_pass=True), high, rate, high_pass=False)

def resample_poly(wave, numerator, denominator):
    # Band-limited interpolation with a Kaiser-windowed sinc kernel.
    ratio = numerator / denominator
    cutoff = min(1., ratio) * .965
    half = int(np.ceil(24 / cutoff))
    padded = np.pad(wave, half + 1)
    offsets = np.arange(-half + 1, half + 1)
    positions = np.arange(int(np.ceil(len(wave) * ratio))) / ratio
    output = np.empty(len(positions))
    for start in range(0, len(output), 4096):
        region = positions[start:start + 4096]
        centers = region.astype(int)
        delta = region[:, None] - centers[:, None] - offsets
        window = np.i0(8.6 * np.sqrt(np.maximum(0, 1 - (delta / half) ** 2))) / np.i0(8.6)
        kernel = cutoff * np.sinc(cutoff * delta) * window
        samples = padded[centers[:, None] + offsets + half + 1]
        output[start:start + len(region)] = np.sum(samples * kernel, axis=1)
    return output

def limit_tail(wave, seconds):
    n = min(len(wave), round(seconds * SR))
    wave[-n:] *= np.cos(np.linspace(0, np.pi / 2, n)) ** 2
    return wave

class Effect:
    def __init__(self, duration, seed):
        self.wave = np.zeros(round(duration * SR))
        self.random = np.random.default_rng(seed)
        self.sources = []

    def add(self, wave, start, level):
        start = round(start * SR)
        count = min(len(wave), len(self.wave) - start)
        if count > 0 and peak(wave) > 0:
            self.wave[start:start + count] += wave[:count] * (level / peak(wave))

    def recording(self, name, start, seconds, level, speed=1., low=120, high=8500):
        relative = ("Casino/Audio/" if name.startswith(("card-", "cards-"))
                    else "Impact/media/audio/") + name + ".ogg"
        path = SOURCE / relative
        wave, rate = sf.read(path, always_2d=True)
        wave = wave.mean(axis=1)
        onset = np.flatnonzero(np.abs(wave) > peak(wave) * .008)[0]
        wave = wave[max(0, onset - round(.001 * rate)):]
        ratio = (Fraction(SR, rate) / Fraction(str(speed))).limit_denominator(1000)
        wave = resample_poly(wave, ratio.numerator, ratio.denominator)
        wave = band(wave, low, high)[:round(seconds * SR)]
        self.add(limit_tail(wave, .012), start, level)
        self.sources.append(relative)

    def resonance(self, start, seconds, frequency, level, color, bend=0., attack=.003):
        t = np.arange(round(seconds * SR)) / SR
        wave = np.zeros_like(t)
        for ratio, amplitude, decay in color:
            phase = 2 * np.pi * ratio * frequency * (t + bend * t * t / (2 * seconds))
            wave += amplitude * np.sin(phase) * np.exp(-t / (seconds * decay))
        wave *= -np.expm1(-t / attack)
        self.add(limit_tail(wave, .025), start, level)

    def air(self, start, seconds, low, high, level, taper):
        t = np.linspace(0., 1., round(seconds * SR))
        wave = band(self.random.normal(size=t.size), low, high)
        wave *= np.sin(np.pi * t) ** 2 * np.exp(taper * (t - .5))
        self.add(wave, start, level)

    def render(self, target_peak):
        wave = band(self.wave, 65, 15200)
        wave = limit_tail(wave, .018)
        wave = np.tanh(1.12 * wave / peak(wave))
        return wave * gain(target_peak) / peak(wave)

WOOD = [(1, 1., .18), (1.63, .62, .13), (2.48, .36, .09), (4.1, .18, .055)]
STONE = [(1, .85, .24), (1.91, 1., .16), (2.72, .52, .16), (5.24, .29, .11)]
BRONZE = [(1, 1., .36), (2.09, .64, .24), (3.91, .43, .16), (5.62, .18, .11)]
HORN = [(1, .48, .3), (2, 1., .28), (3, .71, .24), (4, .32, .17), (6, .18, .1)]
GLASS = [(1, .9, .28), (2.01, .49, .22), (3, .26, .18), (4.23, .36, .14), (7.11, .17, .09)]
LOW = [(1, .87, .31), (2, .62, .22), (3.02, .37, .18), (4.99, .19, .12)]

def design(effect, cue, variant):
    rec = effect.recording
    ring = effect.resonance
    air = effect.air
    if cue == "CardDeal":
        rec(f"card-slide-{[1, 3, 4][variant]}", 0, .11, .56, [1.17, 1.34, 1.03][variant], 470)
        air(.006, .085, 1100, 5800, .18, -1.)
        rec(f"card-place-{[1, 2, 3][variant]}", [.047, .041, .056][variant], .078, .56, 1.33, 550, 7600)
    elif cue == "CardPick":
        rec("card-slide-2", 0, .075, .72, 1.57, 450, 7200)
        rec("card-fan-1", .017, .068, .43, 1.26, 650, 7700)
        air(.003, .07, 1900, 7100, .13, -.4)
    elif cue == "CardPlace":
        rec(f"card-place-{[1, 4][variant]}", 0, .13, .57, 1.06, 190, 7300)
        rec(f"impactWood_medium_00{[1, 3][variant]}", .004, .15, .67, 1.16, 115, 4500)
        ring(.004, .10, [206, 193][variant], .34, WOOD, -.045)
    elif cue == "FieldPlace":
        rec("card-shove-3", 0, .12, .29, .92, 380)
        rec("impactGeneric_light_002", .012, .19, .78, .82, 100, 5300)
        ring(.012, .23, 153, .53, STONE, -.035)
    elif cue == "FieldFlip":
        rec("card-slide-3", 0, .13, .5, 1.29, 425)
        air(.005, .14, 940, 5800, .26, 2.)
        rec("impactMetal_light_003", .076, .15, .38, .93, 700, 5900)
        ring(.071, .22, 340, .41, BRONZE, -.014)
        ring(.117, .19, 587, .22, GLASS, .037)
    elif cue == "AbilityDiscard":
        rec("card-slide-1", 0, .071, .45, 1.18, 490, 6500)
        rec("card-shove-1", .013, .09, .31, 1.2, 380, 5500)
        rec("card-place-4", .039, .091, .68, .93, 190, 4400)
        rec("impactSoft_medium_001", .041, .095, .34, 1.2, 100, 2600)
    elif cue == "BattleStart":
        rec("impactPunch_heavy_002", 0, .28, .73, .86, 96, 4100)
        air(.004, .24, 780, 3150, .24, -1.)
        ring(.01, .46, 147, .69, HORN, .022, .031)
        ring(.117, .53, 196, .57, HORN, -.026, .033)
        ring(.133, .55, 222, .24, BRONZE, -.018, .026)
        ring(.237, .48, 294, .11, HORN, -.012, .041)
    elif cue == "PowerUp":
        ring(0, .117, 440, .73, GLASS, .083, .005)
        air(.014, .12, 1800, 7500, .23, 1.)
        ring(.048, .168, 659, .77, GLASS, .04, .009)
        ring(.074, .13, 880, .2, BRONZE, .019)
    elif cue == "PowerDown":
        ring(0, .145, 441, .55, LOW, -.082, .012)
        air(.006, .15, 450, 2000, .3, -2.)
        ring(.052, .18, 294, .69, LOW, -.071, .012)
        ring(.09, .14, 174, .22, WOOD, -.03)
    elif cue == "BattleWin":
        rec("impactWood_medium_004", 0, .19, .61, .97, 140, 4100)
        ring(.009, .30, 196, .62, BRONZE, .019)
        ring(.093, .42, 294, .8, HORN, .022, .021)
        ring(.139, .41, 392, .47, GLASS, .018, .018)
        ring(.178, .37, 587, .24, BRONZE, -.01, .013)
    elif cue == "BattleLose":
        rec("impactSoft_medium_003", 0, .25, .58, .73, 98, 2300)
        ring(.006, .34, 330, .65, LOW, -.07, .021)
        ring(.106, .44, 220, .66, LOW, -.052, .023)
        ring(.154, .4, 155, .31, STONE, -.038, .019)
        air(.024, .28, 260, 1310, .18, -1.)
    elif cue == "CreatureOut":
        air(0, .26, 980, 3950, .56, -2.2)
        ring(.009, .29, 292, .48, GLASS, -.21, .018)
        rec("impactSoft_medium_000", .032, .22, .48, .8, 125, 2400)
        ring(.055, .23, 139, .28, LOW, -.18, .01)
    elif cue == "FieldRemove":
        rec("card-shove-2", 0, .18, .46, .77, 200, 5200)
        rec("impactGeneric_light_003", .063, .26, .73, .78, 90, 4100)
        ring(.065, .33, 171, .46, STONE, -.061, .012)
        air(.036, .31, 350, 2090, .19, -1.)
    elif cue == "TurnStart":
        ring(0, .213, 392, .68, BRONZE, .006, .009)
        ring(.103, .274, 523, .82, GLASS, -.008, .012)
        ring(.115, .22, 262, .19, LOW, 0, .015)
    elif cue == "AbilityPlay":
        rec(f"impactMetal_light_00{[2, 4][variant]}", 0, .095, .43, 1.21, 830, 7200)
        air(.005, .16, 1300, 4900, .43, .7)
        ring(.021, .218, [517, 478][variant], .63, GLASS, [.115, .081][variant], .011)
        ring([.084, .067][variant], .18, [773, 723][variant], .26, BRONZE, -.065, .012)
    elif cue == "Button":
        rec("impactWood_medium_000", 0, .069, .88, 1.45, 280, 4300)
        ring(.004, .067, 344, .31, WOOD, -.022)

def main():
    MASTER.mkdir(parents=True, exist_ok=True)
    Path("Logs/P0").mkdir(parents=True, exist_ok=True)
    heard = []
    stats = []
    origins = []
    counts = {}
    for index, (asset_id, cue, path, seconds, target, note) in enumerate(CLIPS):
        variant = counts.get(cue, 0)
        counts[cue] = variant + 1
        effect = Effect(seconds, 310 + index)
        design(effect, cue, variant)
        wave = effect.render(target)
        src = MASTER / (Path(path).name + ".wav")
        sf.write(src, wave, SR, "PCM_24")
        dest = RUNTIME / (path + ".wav")
        dest.parent.mkdir(parents=True, exist_ok=True)
        sf.write(dest, resample_poly(wave, 147, 160), OUT, "PCM_16")
        runtime, rate = sf.read(dest)
        true_peak = db(peak(resample_poly(runtime, 4, 1)))
        start = 1000 * np.flatnonzero(np.abs(runtime) > gain(-52))[0] / rate
        tail = db(rms(runtime[-round(rate * .005):]))
        phone = db(rms(band(runtime, 350, 6500, rate)))
        assert rate == OUT and runtime.ndim == 1 and true_peak < -1
        assert start < 15 and tail < -50 and phone > -44, (path, start, tail, phone)
        stats.append(f"| {cue} | `{path}.wav` | {len(runtime)/rate:.3f} | {db(peak(runtime)):.1f} | {true_peak:.1f} | {db(rms(runtime)):.1f} | {start:.1f} | {tail:.1f} | {phone:.1f} |")
        sources = " / ".join(f"`{Path(s).name}` ({s.split('/')[0]})" for s in effect.sources)
        origins.append(f"| `{asset_id}` | `{path}.wav` | {sources or 'Оригинальные синтезированные слои'} | {note} |")
        heard.extend([runtime, np.zeros(round(.38 * rate))])
    sf.write("Logs/P0/p0-preview.wav", np.concatenate(heard), OUT, "PCM_16")
    Path("Logs/P0/stats.md").write_text("\n".join(stats) + "\n", encoding="utf-8")
    Path("Logs/P0/origins.md").write_text("\n".join(origins) + "\n", encoding="utf-8")
    print(f"{len(stats)} WAVs passed timing, format, peak, attack, tail and band checks.")

if __name__ == "__main__":
    main()
