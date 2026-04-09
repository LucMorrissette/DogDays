"""
Splash / Intro Screen Theme Generator
======================================
Upbeat 80s Canadian synth-rock — driving, catchy, and bright.
Think Kim Mitchell "Patio Lanterns" / "Go For a Soda" energy:
prominent synth lead hooks, punchy guitar, bouncy bass, big drums.

Key: A major (bright, anthemic)
Tempo: 138 BPM

Song Structure (24 measures @ 138 BPM ≈ 42s, designed to loop):
  Intro riff (4 bars) → Verse hook (8 bars) → Chorus (8 bars) →
  Outro turnaround (4 bars → loops to Intro)
"""

import os
import sys

sys.path.insert(0, os.path.dirname(__file__))
from midi_utils import MidiBuilder, GMInstrument, DrumNote

# ── Constants ──────────────────────────────────────────────────────────────────
TEMPO = 138
BEATS_PER_MEASURE = 4

# Track indices
SYNTH_LEAD = 0
GUITAR = 1
BASS = 2
DRUMS = 3
SYNTH_PAD = 4

# MIDI channels
LEAD_CH = 0
GUITAR_CH = 1
BASS_CH = 2
PAD_CH = 3

# ── MIDI note constants (A major scale: A B C# D E F# G#) ─────────────────────
# Octave 2
A2  = 45
B2  = 47
Cs3 = 49
D3  = 50
E3  = 52
Fs3 = 54
Gs3 = 56

# Octave 3
A3  = 57
B3  = 59
Cs4 = 61
D4  = 62
E4  = 64
Fs4 = 66
Gs4 = 68

# Octave 4
A4  = 69
B4  = 71
Cs5 = 73
D5  = 74
E5  = 76

# Low bass notes
A1  = 33
D2  = 38
E2  = 40
Fs2 = 42

REST = None

# ── Chord definitions ─────────────────────────────────────────────────────────
# Full open voicings for guitar (5-6 notes, wide spread for thick 80s sound)
GUITAR_CHORDS = {
    "A":   [A2, E3, A3, Cs4, E4, A4],    # A2 E3 A3 C#4 E4 A4
    "D":   [D3, A3, D4, Fs4, A4],         # D3 A3 D4 F#4 A4
    "E":   [E3, B3, E4, Gs4, B4],         # E3 B3 E4 G#4 B4
    "F#m": [Fs3, Cs4, Fs4, A4, Cs5],     # F#3 C#4 F#4 A4 C#5
}

# Full voicings for synth pad (warm background)
PAD_CHORDS = {
    "A":   [A3, Cs4, E4],
    "D":   [D4, Fs4, A4],
    "E":   [E4, Gs4, B4],
    "F#m": [Fs3, A3, Cs4],
}

# Bass roots
BASS_ROOTS = {"A": A1, "D": D2, "E": E2, "F#m": Fs2}

# Section chord progressions
MAIN_CHORDS   = ["A", "D", "E", "D"]       # Bright, driving — 2 bars each
CHORUS_CHORDS = ["F#m", "D", "A", "E"]     # Lift into chorus — 2 bars each

D = DrumNote  # shorthand


# ═══════════════════════════════════════════════════════════════════════════════
#  SYNTH LEAD — The catchy Kim Mitchell hook
# ═══════════════════════════════════════════════════════════════════════════════

def lead_intro_riff(builder, start):
    """Punchy synth riff — the memorable splash hook. 4 bars."""
    # Bar 1-2: A major riff (think "Patio Lanterns" synth hook)
    riff_phrase_1 = [
        (0.0,   A4,  0.5,  110),   # driving A
        (0.5,   Cs5, 0.25, 100),   # quick C#
        (0.75,  E5,  0.75, 105),   # hold E
        (1.5,   D5,  0.5,  100),   # bounce to D
        (2.0,   Cs5, 0.5,  105),   # C#
        (2.5,   A4,  0.25, 95),    # quick A
        (2.75,  B4,  0.75, 100),   # hold B
        (3.5,   A4,  0.5,  90),    # land on A
        # Bar 2: resolution phrase
        (4.0,   Cs5, 0.5,  110),
        (4.5,   E5,  0.25, 100),
        (4.75,  D5,  0.5,  105),
        (5.25,  Cs5, 0.25, 95),
        (5.5,   B4,  0.5,  100),
        (6.0,   A4,  1.5,  105),   # long held A
        (7.5,   Gs4, 0.5,  85),    # leading tone pickup
    ]
    for offset, pitch, dur, vel in riff_phrase_1:
        builder.add_note(SYNTH_LEAD, LEAD_CH, pitch, start + offset, dur, vel)

    # Bar 3-4: higher energy repeat with variation
    riff_phrase_2 = [
        (8.0,   E5,  0.5,  115),   # start higher
        (8.5,   D5,  0.25, 105),
        (8.75,  Cs5, 0.5,  110),
        (9.25,  B4,  0.25, 100),
        (9.5,   A4,  0.5,  105),
        (10.0,  B4,  0.5,  100),
        (10.5,  Cs5, 0.5,  110),
        (11.0,  E5,  0.75, 115),   # hold high E
        (11.75, D5,  0.25, 100),
        # Bar 4: build to section entry
        (12.0,  Cs5, 0.5,  110),
        (12.5,  B4,  0.25, 100),
        (12.75, A4,  0.25, 95),
        (13.0,  B4,  0.5,  100),
        (13.5,  Cs5, 0.5,  105),
        (14.0,  E5,  1.0,  115),   # sustained E landing
        (15.0,  D5,  0.5,  100),   # quick turnaround
        (15.5,  Cs5, 0.5,  95),
    ]
    for offset, pitch, dur, vel in riff_phrase_2:
        builder.add_note(SYNTH_LEAD, LEAD_CH, pitch, start + offset, dur, vel)


def lead_verse_hook(builder, start, chord, bar_idx):
    """Melodic hook that follows the chord changes. 2 bars."""
    # Different melodic phrase per chord position for variety
    phrases = {
        "A": [
            (0.0,  A4,  0.5,  105),
            (0.5,  Cs5, 0.25, 95),
            (0.75, E5,  0.75, 110),
            (1.5,  D5,  0.5,  100),
            (2.0,  Cs5, 1.0,  105),
            (3.0,  B4,  0.5,  90),
            (3.5,  A4,  0.5,  85),
            # Bar 2
            (4.0,  Cs5, 0.5,  100),
            (4.5,  B4,  0.5,  95),
            (5.0,  A4,  1.5,  105),
            (6.5,  Gs4, 0.25, 80),
            (6.75, A4,  0.5,  85),
            (7.25, B4,  0.75, 90),
        ],
        "D": [
            (0.0,  D5,  0.5,  110),
            (0.5,  Cs5, 0.25, 100),
            (0.75, D5,  0.5,  105),
            (1.25, E5,  0.75, 110),
            (2.0,  D5,  0.5,  100),
            (2.5,  Cs5, 0.5,  95),
            (3.0,  B4,  0.5,  90),
            (3.5,  A4,  0.5,  85),
            # Bar 2
            (4.0,  D5,  0.75, 105),
            (4.75, Cs5, 0.25, 95),
            (5.0,  B4,  1.0,  100),
            (6.0,  A4,  0.5,  90),
            (6.5,  B4,  0.75, 95),
            (7.25, Cs5, 0.75, 100),
        ],
        "E": [
            (0.0,  E5,  0.75, 115),
            (0.75, D5,  0.25, 100),
            (1.0,  Cs5, 0.5,  110),
            (1.5,  B4,  0.5,  100),
            (2.0,  Cs5, 0.75, 105),
            (2.75, E5,  0.5,  110),
            (3.25, D5,  0.75, 100),
            # Bar 2
            (4.0,  E5,  0.5,  110),
            (4.5,  Cs5, 0.5,  100),
            (5.0,  B4,  0.75, 105),
            (5.75, A4,  0.25, 90),
            (6.0,  B4,  0.5,  95),
            (6.5,  Cs5, 0.5,  100),
            (7.0,  D5,  0.5,  105),
            (7.5,  E5,  0.5,  95),
        ],
    }
    # D chord appears twice, use the same phrase; fall back to A for unknown
    phrase = phrases.get(chord, phrases["A"])
    for offset, pitch, dur, vel in phrase:
        builder.add_note(SYNTH_LEAD, LEAD_CH, pitch, start + offset, dur, vel)


def lead_chorus(builder, start, chord, bar_idx):
    """Soaring chorus melody — higher energy, longer held notes. 2 bars."""
    phrases = {
        "F#m": [
            (0.0,  Cs5, 0.75, 110),
            (0.75, A4,  0.25, 95),
            (1.0,  Fs4, 0.5,  100),
            (1.5,  A4,  0.5,  105),
            (2.0,  Cs5, 1.0,  115),
            (3.0,  B4,  0.5,  100),
            (3.5,  A4,  0.5,  95),
            (4.0,  Fs4, 0.75, 100),
            (4.75, A4,  0.25, 90),
            (5.0,  Cs5, 1.5,  110),
            (6.5,  B4,  0.5,  95),
            (7.0,  A4,  0.5,  90),
            (7.5,  Gs4, 0.5,  85),
        ],
        "D": [
            (0.0,  D5,  1.0,  115),
            (1.0,  Cs5, 0.5,  105),
            (1.5,  D5,  0.5,  110),
            (2.0,  E5,  1.5,  120),
            (3.5,  D5,  0.5,  100),
            (4.0,  Cs5, 0.75, 110),
            (4.75, B4,  0.25, 95),
            (5.0,  A4,  0.75, 105),
            (5.75, B4,  0.25, 90),
            (6.0,  Cs5, 1.0,  110),
            (7.0,  D5,  0.5,  105),
            (7.5,  E5,  0.5,  100),
        ],
        "A": [
            (0.0,  E5,  0.75, 120),
            (0.75, Cs5, 0.25, 105),
            (1.0,  A4,  0.5,  110),
            (1.5,  Cs5, 0.5,  105),
            (2.0,  E5,  1.5,  120),
            (3.5,  D5,  0.5,  105),
            (4.0,  Cs5, 0.5,  110),
            (4.5,  B4,  0.5,  100),
            (5.0,  A4,  1.5,  115),
            (6.5,  B4,  0.5,  95),
            (7.0,  Cs5, 0.5,  100),
            (7.5,  D5,  0.5,  95),
        ],
        "E": [
            (0.0,  E5,  1.0,  120),
            (1.0,  D5,  0.5,  110),
            (1.5,  Cs5, 0.5,  105),
            (2.0,  B4,  0.75, 110),
            (2.75, Cs5, 0.25, 100),
            (3.0,  D5,  0.5,  110),
            (3.5,  E5,  0.5,  115),
            (4.0,  Cs5, 0.5,  110),
            (4.5,  D5,  0.5,  105),
            (5.0,  E5,  1.5,  120),
            (6.5,  D5,  0.5,  105),
            (7.0,  Cs5, 0.5,  100),
            (7.5,  B4,  0.5,  95),
        ],
    }
    phrase = phrases.get(chord, phrases["A"])
    for offset, pitch, dur, vel in phrase:
        builder.add_note(SYNTH_LEAD, LEAD_CH, pitch, start + offset, dur, vel)


# ═══════════════════════════════════════════════════════════════════════════════
#  GUITAR — Bright rhythm chords
# ═══════════════════════════════════════════════════════════════════════════════

def guitar_muted_chug(builder, start, chord, measures=2, volume=90):
    """Palm-muted 8th-note chugging — driving 80s rock rhythm. 2 bars."""
    notes = GUITAR_CHORDS[chord]
    for m in range(measures):
        t = start + m * 4
        for beat in range(8):
            note_time = t + beat * 0.5
            # Accent beats 1 and 3; lighter on upbeats
            vel = volume if beat % 4 == 0 else (volume - 10 if beat % 2 == 0 else volume - 20)
            for j, pitch in enumerate(notes):
                builder.add_note(GUITAR, GUITAR_CH, pitch,
                                 note_time + j * 0.015, 0.65, vel - j)


def guitar_open_strum(builder, start, chord, measures=2, volume=95):
    """Open strummed chords — bigger, more chorus energy. 2 bars."""
    notes = GUITAR_CHORDS[chord]
    for m in range(measures):
        t = start + m * 4
        # Strum on 1, and-of-2, 3, and-of-4 (syncopated 80s feel)
        strum_times = [0.0, 1.5, 2.0, 3.5]
        strum_durs  = [1.8, 0.6, 1.8, 0.6]
        strum_vels  = [volume, volume - 15, volume - 5, volume - 15]
        for st, sd, sv in zip(strum_times, strum_durs, strum_vels):
            for j, pitch in enumerate(notes):
                builder.add_note(GUITAR, GUITAR_CH, pitch,
                                 t + st + j * 0.035, sd, sv - j * 2)


def guitar_intro(builder, start):
    """Intro guitar: sparse power chord stabs on beats 1 and 3. 4 bars."""
    intro_chords = [("A", 0), ("A", 4), ("D", 8), ("E", 12)]
    for chord, offset in intro_chords:
        notes = GUITAR_CHORDS[chord]
        t = start + offset
        # Hit on beat 1 — big stab, let it ring
        for j, pitch in enumerate(notes):
            builder.add_note(GUITAR, GUITAR_CH, pitch, t + j * 0.035, 1.8, 100 - j * 2)
        # Hit on beat 3 — slightly softer
        for j, pitch in enumerate(notes):
            builder.add_note(GUITAR, GUITAR_CH, pitch, t + 2.0 + j * 0.035, 1.5, 90 - j * 2)


def guitar_outro(builder, start):
    """Outro: sustained ring-out chords. 4 bars."""
    chords = ["A", "D", "E", "A"]
    for i, chord in enumerate(chords):
        t = start + i * 4
        notes = GUITAR_CHORDS[chord]
        vol = 90 - i * 5
        for j, pitch in enumerate(notes):
            builder.add_note(GUITAR, GUITAR_CH, pitch, t + j * 0.02, 3.5, vol - j * 2)


# ═══════════════════════════════════════════════════════════════════════════════
#  BASS — Bouncy, melodic 80s bass
# ═══════════════════════════════════════════════════════════════════════════════

# Scale tones relative to each chord root (in A major)
BASS_SCALE = {
    "A":   {"root": A1, "3rd": Cs3 - 12, "5th": E2, "oct": A2},
    "D":   {"root": D2, "3rd": Fs2, "5th": A2, "oct": D3},
    "E":   {"root": E2, "3rd": Gs3 - 12, "5th": B2, "oct": E3},
    "F#m": {"root": Fs2, "3rd": A2, "5th": Cs3, "oct": Fs3},
}


def bass_driving(builder, start, chord, measures=2):
    """Driving 80s bass — octave bounces and chromatic approaches. 2 bars."""
    n = BASS_SCALE[chord]
    root, fifth, octave = n["root"], n["5th"], n["oct"]

    for m in range(measures):
        t = start + m * 4
        # Root on 1, octave pop on and-of-2, root on 3, walk-up on 4
        builder.add_note(BASS, BASS_CH, root,   t + 0.0, 1.0,  105)
        builder.add_note(BASS, BASS_CH, root,   t + 1.0, 0.5,  85)
        builder.add_note(BASS, BASS_CH, octave, t + 1.5, 0.5,  100)
        builder.add_note(BASS, BASS_CH, root,   t + 2.0, 0.75, 100)
        builder.add_note(BASS, BASS_CH, fifth,  t + 2.75, 0.25, 85)
        builder.add_note(BASS, BASS_CH, root,   t + 3.0, 0.5,  90)
        builder.add_note(BASS, BASS_CH, n["3rd"], t + 3.5, 0.5, 80)


def bass_chorus(builder, start, chord, measures=2):
    """Chorus bass — busier, 8th-note pumping with fills. 2 bars."""
    n = BASS_SCALE[chord]
    root, third, fifth, octave = n["root"], n["3rd"], n["5th"], n["oct"]

    for m in range(measures):
        t = start + m * 4
        # Pumping 8th-note root with octave variation
        builder.add_note(BASS, BASS_CH, root,   t + 0.0, 0.5, 110)
        builder.add_note(BASS, BASS_CH, root,   t + 0.5, 0.5, 85)
        builder.add_note(BASS, BASS_CH, fifth,  t + 1.0, 0.5, 95)
        builder.add_note(BASS, BASS_CH, root,   t + 1.5, 0.5, 85)
        builder.add_note(BASS, BASS_CH, octave, t + 2.0, 0.5, 110)
        builder.add_note(BASS, BASS_CH, root,   t + 2.5, 0.5, 90)
        builder.add_note(BASS, BASS_CH, fifth,  t + 3.0, 0.5, 95)
        builder.add_note(BASS, BASS_CH, third,  t + 3.5, 0.5, 80)


def bass_intro(builder, start):
    """Intro bass: roots on the beat, enters bar 3. 2 bars of bass."""
    builder.add_note(BASS, BASS_CH, A1, start + 8,  3.5, 95)
    builder.add_note(BASS, BASS_CH, D2, start + 12, 1.5, 95)
    builder.add_note(BASS, BASS_CH, E2, start + 14, 1.5, 90)


def bass_outro(builder, start):
    """Outro bass: long sustained roots trailing off. 4 bars."""
    chords = ["A", "D", "E", "A"]
    for i, chord in enumerate(chords):
        root = BASS_ROOTS[chord]
        builder.add_note(BASS, BASS_CH, root, start + i * 4, 3.5, 90 - i * 8)


# ═══════════════════════════════════════════════════════════════════════════════
#  SYNTH PAD — Warm 80s background wash
# ═══════════════════════════════════════════════════════════════════════════════

def pad_sustain(builder, start, chord, measures=2, volume=65):
    """Warm sustained synth pad. 2 bars."""
    notes = PAD_CHORDS[chord]
    for pitch in notes:
        builder.add_note(SYNTH_PAD, PAD_CH, pitch, start, measures * 4 - 0.5, volume)


def pad_intro(builder, start):
    """Intro pad: swells in on bar 2. 4 bars."""
    # Bar 2-4: gradual entrance
    a_notes = PAD_CHORDS["A"]
    d_notes = PAD_CHORDS["D"]
    e_notes = PAD_CHORDS["E"]

    for pitch in a_notes:
        builder.add_note(SYNTH_PAD, PAD_CH, pitch, start + 4, 3.5, 45)
    for pitch in d_notes:
        builder.add_note(SYNTH_PAD, PAD_CH, pitch, start + 8, 3.5, 55)
    for pitch in e_notes:
        builder.add_note(SYNTH_PAD, PAD_CH, pitch, start + 12, 3.5, 60)


def pad_outro(builder, start):
    """Outro pad: fading sustained chords. 4 bars."""
    chords = ["A", "D", "E", "A"]
    for i, chord in enumerate(chords):
        notes = PAD_CHORDS[chord]
        vol = 60 - i * 8
        for pitch in notes:
            builder.add_note(SYNTH_PAD, PAD_CH, pitch, start + i * 4, 3.5, vol)


# ═══════════════════════════════════════════════════════════════════════════════
#  DRUMS — Driving 80s rock beat
# ═══════════════════════════════════════════════════════════════════════════════

def drums_verse(builder, start, measures=2):
    """Driving verse beat: kick on 1/3, snare on 2/4, 8th hi-hats. 2 bars."""
    for m in range(measures):
        t = start + m * 4
        # Kick on 1 and 3, plus 'and' of 3 for extra push
        builder.add_drum_note(DRUMS, D.BassDrum1, t + 0.0, 0.25, 110)
        builder.add_drum_note(DRUMS, D.BassDrum1, t + 2.0, 0.25, 105)
        builder.add_drum_note(DRUMS, D.BassDrum1, t + 2.5, 0.25, 85)

        # Snare on 2 and 4
        builder.add_drum_note(DRUMS, D.AcousticSnare, t + 1.0, 0.25, 112)
        builder.add_drum_note(DRUMS, D.AcousticSnare, t + 3.0, 0.25, 112)

        # 8th-note hi-hats
        for i in range(8):
            ht = t + i * 0.5
            vel = 90 if i % 2 == 0 else 65
            builder.add_drum_note(DRUMS, D.ClosedHiHat, ht, 0.25, vel)

        # Open hi-hat on and-of-4
        builder.add_drum_note(DRUMS, D.OpenHiHat, t + 3.5, 0.25, 80)


def drums_chorus(builder, start, measures=2):
    """Chorus beat: bigger, ride cymbal, more kick energy. 2 bars."""
    for m in range(measures):
        t = start + m * 4
        # Driving kick: 1, and-of-1, 2.5, 3, and-of-3
        for k_off, k_vel in [(0.0, 115), (0.5, 80), (1.5, 90),
                              (2.0, 110), (2.5, 80), (3.5, 85)]:
            builder.add_drum_note(DRUMS, D.BassDrum1, t + k_off, 0.25, k_vel)

        # Snare on 2 and 4 (harder)
        builder.add_drum_note(DRUMS, D.AcousticSnare, t + 1.0, 0.25, 118)
        builder.add_drum_note(DRUMS, D.AcousticSnare, t + 3.0, 0.25, 118)

        # Ride cymbal 8th notes
        for i in range(8):
            vel = 95 if i % 2 == 0 else 70
            builder.add_drum_note(DRUMS, D.RideCymbal1, t + i * 0.5, 0.25, vel)


def drums_intro(builder, start):
    """Intro drums: builds from hi-hat count to full beat. 4 bars."""
    t = start
    # Bars 1-2: hi-hat count with light kick accents
    for i in range(16):
        ht = t + i * 0.5
        vel = 75 if i % 2 == 0 else 55
        builder.add_drum_note(DRUMS, D.ClosedHiHat, ht, 0.25, vel)
    # Light kick on 1 of each bar
    builder.add_drum_note(DRUMS, D.BassDrum1, t, 0.25, 80)
    builder.add_drum_note(DRUMS, D.BassDrum1, t + 4, 0.25, 85)

    # Pickup fill end of bar 2
    builder.add_drum_note(DRUMS, D.AcousticSnare, t + 6.0, 0.25, 80)
    builder.add_drum_note(DRUMS, D.AcousticSnare, t + 6.5, 0.25, 85)
    builder.add_drum_note(DRUMS, D.AcousticSnare, t + 7.0, 0.25, 95)
    builder.add_drum_note(DRUMS, D.HighTom, t + 7.5, 0.25, 100)

    # Bars 3-4: full verse beat
    builder.add_drum_note(DRUMS, D.CrashCymbal1, t + 8.0, 0.5, 115)
    drums_verse(builder, t + 8, measures=2)


def drums_outro(builder, start):
    """Outro drums: winding down. 4 bars."""
    # Bar 1: full beat
    drums_verse(builder, start, measures=1)
    # Bar 2: half-time
    t = start + 4
    builder.add_drum_note(DRUMS, D.BassDrum1, t, 0.25, 95)
    builder.add_drum_note(DRUMS, D.AcousticSnare, t + 2.0, 0.25, 100)
    for i in range(4):
        builder.add_drum_note(DRUMS, D.RideCymbal1, t + i, 0.25, 70 - i * 5)
    # Bar 3: sparse
    t = start + 8
    builder.add_drum_note(DRUMS, D.BassDrum1, t, 0.25, 85)
    builder.add_drum_note(DRUMS, D.RideCymbal1, t, 0.5, 65)
    builder.add_drum_note(DRUMS, D.AcousticSnare, t + 2.0, 0.25, 80)
    # Bar 4: final hit
    t = start + 12
    builder.add_drum_note(DRUMS, D.BassDrum1, t, 0.25, 90)
    builder.add_drum_note(DRUMS, D.CrashCymbal1, t, 1.5, 105)


def fill_tom_roll(builder, start):
    """Quick tom cascade into crash. Last 2 beats of a bar."""
    toms = [D.HighTom, D.HighTom, D.HiMidTom, D.HiMidTom,
            D.LowMidTom, D.LowMidTom, D.LowTom, D.LowTom]
    for i, tom in enumerate(toms):
        builder.add_drum_note(DRUMS, tom, start + i * 0.125, 0.125, 90 + i * 2)
    builder.add_drum_note(DRUMS, D.CrashCymbal1, start + 1.0, 0.5, 120)
    builder.add_drum_note(DRUMS, D.BassDrum1, start + 1.0, 0.25, 110)


def fill_snare_build(builder, start):
    """Snare roll crescendo. 1 beat."""
    for i in range(8):
        vel = 55 + i * 8
        builder.add_drum_note(DRUMS, D.AcousticSnare, start + i * 0.125, 0.125, vel)


def crash_accent(builder, time):
    """Crash on beat 1 of a new section."""
    builder.add_drum_note(DRUMS, D.CrashCymbal1, time, 0.5, 115)


# ═══════════════════════════════════════════════════════════════════════════════
#  SONG ASSEMBLY
# ═══════════════════════════════════════════════════════════════════════════════

def generate_song():
    builder = MidiBuilder(num_tracks=5, tempo=TEMPO)

    # Set up tracks
    builder.add_track(SYNTH_LEAD, "Synth Lead", GMInstrument.Lead5Charang, LEAD_CH)
    builder.add_track(GUITAR, "Rhythm Guitar", GMInstrument.ElectricGuitarClean, GUITAR_CH)
    builder.add_track(BASS, "Electric Bass", GMInstrument.ElectricBassPick, BASS_CH)
    builder.add_track(DRUMS, "Drums")  # channel 9 automatic
    builder.add_track(SYNTH_PAD, "Synth Pad", GMInstrument.Pad3Polysynth, PAD_CH)

    t = 0  # current time in beats

    # ── INTRO (4 bars = 16 beats) ─────────────────────────────────────────────
    print("  Building Intro...")
    lead_intro_riff(builder, t)
    guitar_intro(builder, t)
    bass_intro(builder, t)
    drums_intro(builder, t)
    pad_intro(builder, t)

    t += 16

    # ── VERSE HOOK (8 bars = 32 beats) ────────────────────────────────────────
    print("  Building Verse...")
    crash_accent(builder, t)
    for i, chord in enumerate(MAIN_CHORDS):
        bar_start = t + i * 8
        lead_verse_hook(builder, bar_start, chord, i)
        guitar_muted_chug(builder, bar_start, chord, measures=2, volume=88)
        bass_driving(builder, bar_start, chord, measures=2)
        pad_sustain(builder, bar_start, chord, measures=2, volume=55)

        if i < 3:
            drums_verse(builder, bar_start, measures=2)
        else:
            # Fill before chorus
            drums_verse(builder, bar_start, measures=1)
            drums_verse(builder, bar_start + 4, measures=1)
            fill_tom_roll(builder, bar_start + 6)

    t += 32

    # ── CHORUS (8 bars = 32 beats) ────────────────────────────────────────────
    print("  Building Chorus...")
    crash_accent(builder, t)
    for i, chord in enumerate(CHORUS_CHORDS):
        bar_start = t + i * 8
        lead_chorus(builder, bar_start, chord, i)
        guitar_open_strum(builder, bar_start, chord, measures=2, volume=98)
        bass_chorus(builder, bar_start, chord, measures=2)
        pad_sustain(builder, bar_start, chord, measures=2, volume=65)

        if i < 3:
            drums_chorus(builder, bar_start, measures=2)
        else:
            # Build to loop point
            drums_chorus(builder, bar_start, measures=1)
            fill_snare_build(builder, bar_start + 7)

        # Crash accents on chord changes
        if i > 0:
            crash_accent(builder, bar_start)

    t += 32

    # ── OUTRO / TURNAROUND (4 bars = 16 beats) ───────────────────────────────
    print("  Building Outro...")
    crash_accent(builder, t)
    guitar_outro(builder, t)
    bass_outro(builder, t)
    drums_outro(builder, t)
    pad_outro(builder, t)

    # Outro lead: reprise of intro hook, fading
    outro_riff = [
        (0.0,  A4,  0.5,  95),
        (0.5,  Cs5, 0.25, 85),
        (0.75, E5,  0.75, 90),
        (1.5,  D5,  0.5,  85),
        (2.0,  Cs5, 1.0,  90),
        (3.0,  B4,  0.5,  80),
        (3.5,  A4,  0.5,  75),
        (4.0,  Cs5, 0.5,  85),
        (4.5,  E5,  1.5,  90),
        (6.0,  D5,  0.5,  80),
        (6.5,  Cs5, 0.5,  75),
        (7.0,  A4,  2.0,  85),  # long held final note
    ]
    for offset, pitch, dur, vel in outro_riff:
        builder.add_note(SYNTH_LEAD, LEAD_CH, pitch, t + offset, dur, vel)

    t += 16

    # ── Save ──────────────────────────────────────────────────────────────────
    total_bars = t // 4
    duration_secs = t * 60.0 / TEMPO
    print(f"\n  Total: {total_bars} measures, {t} beats")
    print(f"  Duration: {duration_secs:.0f}s ({duration_secs/60:.1f} min) @ {TEMPO} BPM")

    output_path = os.path.join(os.path.dirname(__file__), "splash_theme.mid")
    builder.save(output_path)
    return output_path


if __name__ == "__main__":
    print("🎹 Splash Screen Theme Generator")
    print("  (Kim Mitchell 80s synth-rock style)")
    print("=" * 50)
    path = generate_song()
    print(f"\n✅ Done! Open '{path}' in GarageBand to convert.")
