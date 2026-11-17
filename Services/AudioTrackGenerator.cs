using System.Text;

namespace EmotionMusicAnalyzer.Services;

public class AudioTrackGenerator
{
    private const int SampleRate = 44100;

    public static void EnsureTracksGenerated(string webRootPath)
    {
        var audioDir = Path.Combine(webRootPath, "audio");
        if (!Directory.Exists(audioDir))
        {
            Directory.CreateDirectory(audioDir);
        }

        var trackDefinitions = new (string fileName, string mood, int bpm, double durationSeconds)[]
        {
            // Happy: Upbeat, Party, Energetic (Bright major progressions, driving rhythm)
            ("happy_1.wav", "happy_dance", 124, 24.0),
            ("happy_2.wav", "happy_pop", 128, 24.0),
            ("happy_3.wav", "happy_funk", 120, 24.0),

            // Sad: Emotional, Acoustic, Sad (Melancholy minor piano & acoustic strings)
            ("sad_1.wav", "sad_piano", 68, 26.0),
            ("sad_2.wav", "sad_acoustic", 72, 26.0),
            ("sad_3.wav", "sad_solitude", 65, 26.0),

            // Angry: Calm, Relaxing, Instrumental (Tranquil ambient, singing bowls, stress relief)
            ("angry_1.wav", "angry_ambient", 56, 28.0),
            ("angry_2.wav", "angry_zen", 60, 28.0),
            ("angry_3.wav", "angry_serenity", 54, 28.0),

            // Surprised: Upbeat, Pop, Energetic (Fast vibrant synth arpeggios, electro pulse)
            ("surprised_1.wav", "surprised_synth", 132, 24.0),
            ("surprised_2.wav", "surprised_electro", 136, 24.0),
            ("surprised_3.wav", "surprised_velocity", 130, 24.0),

            // Neutral: Chill, Lo-fi, Balanced (Mellow Rhodes, vinyl warmth, relaxed boom-bap)
            ("neutral_1.wav", "neutral_lofi", 78, 26.0),
            ("neutral_2.wav", "neutral_lounge", 82, 26.0),
            ("neutral_3.wav", "neutral_chill", 75, 26.0),

            // Fearful: Gentle, Calm, Soothing (Music box, warm harp, protective comforting pad)
            ("fearful_1.wav", "fearful_lullaby", 54, 28.0),
            ("fearful_2.wav", "fearful_sanctuary", 52, 28.0),
            ("fearful_3.wav", "fearful_glow", 58, 28.0)
        };

        foreach (var track in trackDefinitions)
        {
            var filePath = Path.Combine(audioDir, track.fileName);
            if (!File.Exists(filePath) || new FileInfo(filePath).Length < 1000)
            {
                GenerateTrackFile(filePath, track.mood, track.bpm, track.durationSeconds);
            }
        }
    }

    private static void GenerateTrackFile(string outputPath, string style, int bpm, double durationSeconds)
    {
        int totalSamples = (int)(SampleRate * durationSeconds);
        float[] leftBuffer = new float[totalSamples];
        float[] rightBuffer = new float[totalSamples];

        double beatDuration = 60.0 / bpm;

        switch (style)
        {
            case "happy_dance":
            case "happy_pop":
            case "happy_funk":
                SynthesizeHappy(leftBuffer, rightBuffer, bpm, beatDuration, style);
                break;

            case "sad_piano":
            case "sad_acoustic":
            case "sad_solitude":
                SynthesizeSad(leftBuffer, rightBuffer, bpm, beatDuration, style);
                break;

            case "angry_ambient":
            case "angry_zen":
            case "angry_serenity":
                SynthesizeCalm(leftBuffer, rightBuffer, bpm, beatDuration, style);
                break;

            case "surprised_synth":
            case "surprised_electro":
            case "surprised_velocity":
                SynthesizeSurprised(leftBuffer, rightBuffer, bpm, beatDuration, style);
                break;

            case "neutral_lofi":
            case "neutral_lounge":
            case "neutral_chill":
                SynthesizeLoFi(leftBuffer, rightBuffer, bpm, beatDuration, style);
                break;

            case "fearful_lullaby":
            case "fearful_sanctuary":
            case "fearful_glow":
                SynthesizeGentle(leftBuffer, rightBuffer, bpm, beatDuration, style);
                break;

            default:
                SynthesizeHappy(leftBuffer, rightBuffer, bpm, beatDuration, style);
                break;
        }

        // Apply smooth master fade in and fade out
        ApplyFade(leftBuffer, rightBuffer, 0.4, 1.2);

        // Normalize and write standard 16-bit PCM stereo WAV
        WritePcmWav(outputPath, leftBuffer, rightBuffer);
    }

    // Happy: C - G - Am - F upbeat progression
    private static void SynthesizeHappy(float[] left, float[] right, int bpm, double beatSec, string style)
    {
        int totalSamples = left.Length;
        // Chords: C major (C4, E4, G4), G major (B3, D4, G4), A minor (A3, C4, E4), F major (F3, A3, C4)
        double[][] chords = new double[][]
        {
            new double[] { 261.63, 329.63, 392.00, 523.25 }, // C
            new double[] { 246.94, 293.66, 392.00, 493.88 }, // G
            new double[] { 220.00, 261.63, 329.63, 440.00 }, // Am
            new double[] { 174.61, 220.00, 261.63, 349.23 }  // F
        };
        double[] bassNotes = new double[] { 65.41, 49.00, 55.00, 43.65 }; // C2, G1, A1, F1

        double barSec = beatSec * 4;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;
            int barIndex = (int)(t / barSec) % 4;
            double tInBar = t % barSec;
            double tInBeat = t % beatSec;
            int beatIndex = (int)(tInBar / beatSec);

            double[] currentChord = chords[barIndex];
            double currentBass = bassNotes[barIndex];

            // 1. Upbeat Piano / Synth Chords on offbeats & 8th notes
            double chordT = tInBar % (beatSec / 2);
            double chordEnv = Math.Exp(-chordT * 6.5);
            float chordSample = 0f;
            foreach (var freq in currentChord)
            {
                chordSample += (float)(Math.Sin(2 * Math.PI * freq * t) * 0.12 * chordEnv);
                chordSample += (float)(Math.Sin(2 * Math.PI * freq * 2 * t) * 0.05 * chordEnv); // harmonic
            }

            // 2. Punchy Bassline (playing 8th note groove)
            double bassT = tInBar % (beatSec / 2);
            double bassEnv = Math.Exp(-bassT * 4.0);
            float bassSample = (float)(
                Math.Sin(2 * Math.PI * currentBass * t) * 0.28 +
                Math.Sin(2 * Math.PI * currentBass * 2 * t) * 0.12
            ) * (float)bassEnv;

            // 3. Kick drum on beats 1, 2, 3, 4
            float kickSample = 0f;
            double kickT = tInBeat;
            if (kickT < 0.25)
            {
                double kickFreq = 140.0 * Math.Exp(-kickT * 35.0) + 45.0;
                kickSample = (float)(Math.Sin(2 * Math.PI * kickFreq * kickT) * Math.Exp(-kickT * 12.0) * 0.45);
            }

            // 4. Snare / Clap on beats 2 and 4
            float snareSample = 0f;
            if (beatIndex == 1 || beatIndex == 3)
            {
                double snareT = tInBeat;
                if (snareT < 0.22)
                {
                    double noise = (Math.Sin(t * 12345.67) + Math.Sin(t * 98765.43)) * 0.5;
                    snareSample = (float)(noise * Math.Exp(-snareT * 22.0) * 0.25);
                }
            }

            // 5. Hi-hats on every 8th note
            float hihatSample = 0f;
            double hatT = t % (beatSec / 2);
            if (hatT < 0.08)
            {
                double noise = Math.Sin(t * 88888.88);
                hihatSample = (float)(noise * Math.Exp(-hatT * 45.0) * 0.12);
            }

            // 6. Bright Arpeggio Lead
            int arpStep = (int)(t / (beatSec / 4)) % 4;
            double arpFreq = currentChord[arpStep] * 2;
            double arpT = t % (beatSec / 4);
            float arpSample = (float)(Math.Sin(2 * Math.PI * arpFreq * t) * Math.Exp(-arpT * 8.0) * 0.14);

            // Mix stereo
            left[i] += (chordSample * 0.85f) + (bassSample * 0.95f) + kickSample + (snareSample * 0.9f) + (hihatSample * 0.6f) + (arpSample * 0.7f);
            right[i] += (chordSample * 0.75f) + (bassSample * 0.95f) + kickSample + (snareSample * 0.9f) + (hihatSample * 1.1f) + (arpSample * 1.1f);
        }
    }

    // Sad: A minor - F major - C major - Em emotional piano & acoustic progression
    private static void SynthesizeSad(float[] left, float[] right, int bpm, double beatSec, string style)
    {
        int totalSamples = left.Length;
        double[][] chords = new double[][]
        {
            new double[] { 220.00, 261.63, 329.63 }, // Am (A3, C4, E4)
            new double[] { 174.61, 220.00, 261.63 }, // F (F3, A3, C4)
            new double[] { 130.81, 164.81, 196.00 }, // C (C3, E3, G3)
            new double[] { 164.81, 196.00, 246.94 }  // Em (E3, G3, B3)
        };
        double[] bassNotes = new double[] { 55.00, 43.65, 65.41, 41.20 }; // A1, F1, C2, E1

        double barSec = beatSec * 4;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;
            int barIndex = (int)(t / barSec) % 4;
            double tInBar = t % barSec;

            double[] chord = chords[barIndex];
            double bass = bassNotes[barIndex];

            // 1. Expressive Acoustic Piano plucking / arpeggios
            int noteIndex = (int)(tInBar / (beatSec * 0.75)) % chord.Length;
            double noteT = tInBar % (beatSec * 0.75);
            double pianoEnv = Math.Exp(-noteT * 2.8);
            double noteFreq = chord[noteIndex];

            float pianoSample = (float)(
                Math.Sin(2 * Math.PI * noteFreq * t) * 0.22 +
                Math.Sin(2 * Math.PI * noteFreq * 2 * t) * 0.08 +
                Math.Sin(2 * Math.PI * noteFreq * 3 * t) * 0.03
            ) * (float)pianoEnv;

            // 2. Deep warm cello / acoustic bass
            double bassT = tInBar;
            double bassEnv = Math.Min(1.0, bassT * 2.0) * Math.Exp(-bassT * 0.4);
            float bassSample = (float)(
                Math.Sin(2 * Math.PI * bass * t) * 0.24 +
                Math.Sin(2 * Math.PI * bass * 2 * t) * 0.10
            ) * (float)bassEnv;

            // 3. Melancholic String Pad Swell
            double padEnv = 0.5 + 0.5 * Math.Sin(2 * Math.PI * (t / barSec));
            float padSample = 0f;
            foreach (var f in chord)
            {
                padSample += (float)(Math.Sin(2 * Math.PI * (f * 2) * t) * 0.04 * padEnv);
            }

            // 4. Subtle gentle acoustic guitar strum
            double strumT = tInBar % beatSec;
            float strumSample = (float)(Math.Sin(2 * Math.PI * chord[0] * 2 * t) * Math.Exp(-strumT * 4.0) * 0.06);

            // Mix
            left[i] += (pianoSample * 1.1f) + bassSample + (padSample * 0.8f) + (strumSample * 0.7f);
            right[i] += (pianoSample * 0.8f) + bassSample + (padSample * 1.1f) + (strumSample * 1.1f);
        }
    }

    // Angry: Calm, Relaxing, Instrumental (432Hz ambient drone, Tibetan singing bowl, serene calm)
    private static void SynthesizeCalm(float[] left, float[] right, int bpm, double beatSec, string style)
    {
        int totalSamples = left.Length;
        // Soothing frequencies: 432Hz tuning, gentle F#m9 / Dmaj7 ambient chords
        double[] freqs = new double[] { 108.0, 162.0, 216.0, 288.0, 324.0, 432.0, 540.0 };

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;

            // 1. Deep Binaural Calming Drone (Alpha wave ~10Hz difference between ears)
            float binauralLeft = (float)(Math.Sin(2 * Math.PI * 108.0 * t) * 0.18 + Math.Sin(2 * Math.PI * 216.0 * t) * 0.08);
            float binauralRight = (float)(Math.Sin(2 * Math.PI * 118.0 * t) * 0.18 + Math.Sin(2 * Math.PI * 226.0 * t) * 0.08);

            // 2. Singing Bowl Resonance every 6 seconds
            double bowlCycle = t % 6.0;
            double bowlEnv = Math.Exp(-bowlCycle * 0.45);
            float bowl = (float)(
                Math.Sin(2 * Math.PI * 432.0 * t) * 0.18 +
                Math.Sin(2 * Math.PI * 864.0 * t) * 0.06 +
                Math.Sin(2 * Math.PI * 1296.0 * t) * 0.02
            ) * (float)bowlEnv;

            // 3. Gentle Ocean / Ambient Breeze Swell
            double oceanLfo = 0.5 + 0.5 * Math.Sin(2 * Math.PI * 0.12 * t);
            float ambientNoise = (float)((Math.Sin(t * 3333.3) * Math.Sin(t * 777.7)) * 0.035 * oceanLfo);

            // 4. Soft Wind Chime accents every 3.5s
            double chimeT = t % 3.5;
            float chime = 0f;
            if (chimeT < 1.5)
            {
                double chimeEnv = Math.Exp(-chimeT * 3.0);
                chime = (float)(Math.Sin(2 * Math.PI * 864.0 * t) * 0.08 * chimeEnv);
            }

            left[i] += binauralLeft + (bowl * 0.95f) + ambientNoise + (chime * 0.7f);
            right[i] += binauralRight + (bowl * 0.95f) + ambientNoise + (chime * 1.1f);
        }
    }

    // Surprised: Upbeat, Pop, Energetic (Fast upbeat arpeggiator, dynamic sidechain, high energy)
    private static void SynthesizeSurprised(float[] left, float[] right, int bpm, double beatSec, string style)
    {
        int totalSamples = left.Length;
        // Chords: F - G - Em - Am dynamic pop progression
        double[][] chords = new double[][]
        {
            new double[] { 349.23, 440.00, 523.25 }, // F4
            new double[] { 392.00, 493.88, 587.33 }, // G4
            new double[] { 329.63, 392.00, 493.88 }, // Em4
            new double[] { 440.00, 523.25, 659.25 }  // Am4
        };
        double[] bassNotes = new double[] { 87.31, 98.00, 82.41, 110.00 }; // F2, G2, E2, A2

        double barSec = beatSec * 4;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;
            int barIndex = (int)(t / barSec) % 4;
            double tInBar = t % barSec;
            double tInBeat = t % beatSec;

            double[] chord = chords[barIndex];
            double bass = bassNotes[barIndex];

            // 1. Fast 16th-note synth pluck arpeggio
            int arpStep = (int)(t / (beatSec / 4)) % chord.Length;
            double arpT = t % (beatSec / 4);
            double arpFreq = chord[arpStep] * (1.0 + (arpStep % 2));
            float arpSample = (float)(Math.Sin(2 * Math.PI * arpFreq * t) * Math.Exp(-arpT * 14.0) * 0.16);

            // 2. High-energy sidechained Synth Chords on beats
            double sidechainEnv = Math.Min(1.0, tInBeat * 5.0);
            float padSample = 0f;
            foreach (var f in chord)
            {
                padSample += (float)(Math.Sin(2 * Math.PI * f * t) * 0.08);
            }
            padSample *= (float)sidechainEnv;

            // 3. Punchy Electro Kick
            float kickSample = 0f;
            if (tInBeat < 0.2)
            {
                double kickFreq = 160.0 * Math.Exp(-tInBeat * 40.0) + 50.0;
                kickSample = (float)(Math.Sin(2 * Math.PI * kickFreq * tInBeat) * Math.Exp(-tInBeat * 14.0) * 0.42);
            }

            // 4. Snappy electro snare on beat 2 & 4
            int beatIndex = (int)(tInBar / beatSec);
            float snareSample = 0f;
            if (beatIndex == 1 || beatIndex == 3)
            {
                if (tInBeat < 0.2)
                {
                    double noise = Math.Sin(t * 76543.21);
                    snareSample = (float)(noise * Math.Exp(-tInBeat * 24.0) * 0.22);
                }
            }

            // 5. Running Bassline
            double bassT = t % (beatSec / 2);
            float bassSample = (float)(Math.Sin(2 * Math.PI * bass * t) * Math.Exp(-bassT * 5.0) * 0.25);

            left[i] += (arpSample * 0.8f) + (padSample * 0.85f) + kickSample + (snareSample * 0.9f) + bassSample;
            right[i] += (arpSample * 1.2f) + (padSample * 0.85f) + kickSample + (snareSample * 0.9f) + bassSample;
        }
    }

    // Neutral: Chill, Lo-fi, Balanced (Warm Rhodes chords, vinyl crackle, relaxed lo-fi beat)
    private static void SynthesizeLoFi(float[] left, float[] right, int bpm, double beatSec, string style)
    {
        int totalSamples = left.Length;
        // Jazz chords: Dm7 - G7 - Cmaj7 - Am7
        double[][] chords = new double[][]
        {
            new double[] { 146.83, 174.61, 220.00, 261.63 }, // Dm7
            new double[] { 196.00, 246.94, 293.66, 349.23 }, // G7
            new double[] { 130.81, 164.81, 196.00, 246.94 }, // Cmaj7
            new double[] { 220.00, 261.63, 329.63, 392.00 }  // Am7
        };
        double[] bassNotes = new double[] { 73.42, 49.00, 65.41, 55.00 }; // D2, G1, C2, A1

        double barSec = beatSec * 4;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;
            int barIndex = (int)(t / barSec) % 4;
            double tInBar = t % barSec;
            double tInBeat = t % beatSec;
            int beatIndex = (int)(tInBar / beatSec);

            double[] chord = chords[barIndex];
            double bass = bassNotes[barIndex];

            // 1. Warm Electric Piano (Rhodes) with tremolo
            double chordT = tInBar % (beatSec * 2);
            double chordEnv = Math.Exp(-chordT * 1.8);
            double tremolo = 0.85 + 0.15 * Math.Sin(2 * Math.PI * 4.5 * t);
            float rhodesSample = 0f;
            foreach (var f in chord)
            {
                rhodesSample += (float)(
                    (Math.Sin(2 * Math.PI * f * t) * 0.16 +
                     Math.Sin(2 * Math.PI * f * 2 * t) * 0.05) * chordEnv * tremolo
                );
            }

            // 2. Mellow Lo-Fi Kick on beat 1 and syncopated beat 3.5
            float kickSample = 0f;
            if (beatIndex == 0 && tInBeat < 0.25)
            {
                double kickFreq = 95.0 * Math.Exp(-tInBeat * 25.0) + 40.0;
                kickSample = (float)(Math.Sin(2 * Math.PI * kickFreq * tInBeat) * Math.Exp(-tInBeat * 10.0) * 0.38);
            }
            else if (beatIndex == 2 && tInBeat > (beatSec * 0.5) && (tInBeat - (beatSec * 0.5)) < 0.25)
            {
                double subT = tInBeat - (beatSec * 0.5);
                double kickFreq = 90.0 * Math.Exp(-subT * 25.0) + 40.0;
                kickSample = (float)(Math.Sin(2 * Math.PI * kickFreq * subT) * Math.Exp(-subT * 10.0) * 0.32);
            }

            // 3. Relaxed Lo-fi Snare / Rimshot on beat 2 and 4
            float snareSample = 0f;
            if (beatIndex == 1 || beatIndex == 3)
            {
                if (tInBeat < 0.18)
                {
                    double noise = (Math.Sin(t * 43210.0) + Math.Sin(t * 12345.0)) * 0.5;
                    snareSample = (float)(noise * Math.Exp(-tInBeat * 18.0) * 0.18);
                }
            }

            // 4. Chill Walking Bassline
            double bassT = tInBar % beatSec;
            float bassSample = (float)(Math.Sin(2 * Math.PI * bass * t) * Math.Exp(-bassT * 2.5) * 0.26);

            // 5. Vinyl crackle & warm tape hiss
            float vinyl = (float)((Math.Sin(t * 99999.9) > 0.985 ? 0.05 : 0.0) + Math.Sin(t * 55555.5) * 0.008);

            left[i] += rhodesSample + kickSample + snareSample + bassSample + (vinyl * 0.8f);
            right[i] += (rhodesSample * 1.05f) + kickSample + snareSample + bassSample + (vinyl * 1.2f);
        }
    }

    // Fearful: Gentle, Calm, Soothing (Soft music box, gentle warm harp, soothing reassuring lullaby)
    private static void SynthesizeGentle(float[] left, float[] right, int bpm, double beatSec, string style)
    {
        int totalSamples = left.Length;
        // Warm major soothing progression: Cmaj9 - Fmaj7 - G6 - C
        double[][] chords = new double[][]
        {
            new double[] { 261.63, 329.63, 392.00, 493.88 }, // Cmaj7
            new double[] { 174.61, 220.00, 261.63, 329.63 }, // Fmaj7
            new double[] { 196.00, 246.94, 293.66, 392.00 }, // G6
            new double[] { 261.63, 329.63, 392.00, 523.25 }  // C
        };

        double barSec = beatSec * 4;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;
            int barIndex = (int)(t / barSec) % 4;
            double tInBar = t % barSec;

            double[] chord = chords[barIndex];

            // 1. Music Box / Celestial Bell melody
            int noteIndex = (int)(tInBar / (beatSec * 0.5)) % chord.Length;
            double noteT = tInBar % (beatSec * 0.5);
            double noteFreq = chord[noteIndex] * 2.0; // Higher register for bell
            double bellEnv = Math.Exp(-noteT * 3.5);

            float bellSample = (float)(
                Math.Sin(2 * Math.PI * noteFreq * t) * 0.16 +
                Math.Sin(2 * Math.PI * noteFreq * 2.76 * t) * 0.04 + // inharmonic bell chime
                Math.Sin(2 * Math.PI * noteFreq * 5.4 * t) * 0.01
            ) * (float)bellEnv;

            // 2. Warm Reassuring Blanket Pad (slow breathing swell)
            double breathLfo = 0.5 + 0.5 * Math.Sin(2 * Math.PI * 0.18 * t);
            float padSample = 0f;
            foreach (var f in chord)
            {
                padSample += (float)(Math.Sin(2 * Math.PI * f * t) * 0.06 * breathLfo);
            }

            // 3. Gentle Soft Acoustic Harp Arpeggio
            double harpT = tInBar % (beatSec * 0.75);
            float harpSample = (float)(Math.Sin(2 * Math.PI * chord[0] * 1.5 * t) * Math.Exp(-harpT * 3.0) * 0.08);

            // 4. Calming gentle heartbeat rhythm (very soft warm low pulse)
            double pulseT = t % beatSec;
            float heartSample = 0f;
            if (pulseT < 0.22)
            {
                heartSample = (float)(Math.Sin(2 * Math.PI * 52.0 * pulseT) * Math.Exp(-pulseT * 12.0) * 0.14);
            }

            left[i] += (bellSample * 0.8f) + padSample + (harpSample * 0.9f) + heartSample;
            right[i] += (bellSample * 1.1f) + padSample + (harpSample * 1.1f) + heartSample;
        }
    }

    private static void ApplyFade(float[] left, float[] right, double fadeInSec, double fadeOutSec)
    {
        int fadeInSamples = (int)(SampleRate * fadeInSec);
        int fadeOutSamples = (int)(SampleRate * fadeOutSec);
        int total = left.Length;

        for (int i = 0; i < fadeInSamples && i < total; i++)
        {
            float factor = (float)i / fadeInSamples;
            left[i] *= factor;
            right[i] *= factor;
        }

        for (int i = 0; i < fadeOutSamples && i < total; i++)
        {
            int index = total - 1 - i;
            float factor = (float)i / fadeOutSamples;
            left[index] *= factor;
            right[index] *= factor;
        }
    }

    private static void WritePcmWav(string filePath, float[] left, float[] right)
    {
        int totalSamples = left.Length;

        // Peak normalization to -1.0 dB to guarantee crisp loudness without clipping
        float maxPeak = 0f;
        for (int i = 0; i < totalSamples; i++)
        {
            float absL = Math.Abs(left[i]);
            float absR = Math.Abs(right[i]);
            if (absL > maxPeak) maxPeak = absL;
            if (absR > maxPeak) maxPeak = absR;
        }

        float normFactor = maxPeak > 0.001f ? 0.88f / maxPeak : 1.0f;

        short[] pcmData = new short[totalSamples * 2];
        for (int i = 0; i < totalSamples; i++)
        {
            float l = left[i] * normFactor;
            float r = right[i] * normFactor;

            // Soft clamp
            l = Math.Clamp(l, -1.0f, 1.0f);
            r = Math.Clamp(r, -1.0f, 1.0f);

            pcmData[i * 2] = (short)(l * 32767);
            pcmData[i * 2 + 1] = (short)(r * 32767);
        }

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        int byteRate = SampleRate * 2 * 2;
        int blockAlign = 4;
        int subChunk2Size = totalSamples * 4;
        int chunkSize = 36 + subChunk2Size;

        // RIFF header
        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(chunkSize);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));

        // "fmt " sub-chunk
        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16); // subchunk1 size
        bw.Write((short)1); // PCM
        bw.Write((short)2); // Stereo
        bw.Write(SampleRate);
        bw.Write(byteRate);
        bw.Write((short)blockAlign);
        bw.Write((short)16); // 16-bit

        // "data" sub-chunk
        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(subChunk2Size);

        // Raw PCM samples
        byte[] buffer = new byte[pcmData.Length * 2];
        Buffer.BlockCopy(pcmData, 0, buffer, 0, buffer.Length);
        bw.Write(buffer);
    }
}
