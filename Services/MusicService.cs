using EmotionMusicAnalyzer.Models;

namespace EmotionMusicAnalyzer.Services;

public class MusicService : IMusicService
{
    private readonly List<Track> _catalog;

    public MusicService()
    {
        _catalog = new List<Track>
        {
            // Happy → Happy, Party, Energetic
            new Track
            {
                Id = "trk-happy-01",
                Title = "Sunburst Horizons",
                Artist = "Solar Soundscape",
                Genre = "Party / Dance Pop",
                DurationSeconds = 24,
                Emotion = EmotionType.Happy,
                AudioUrl = "/audio/happy_1.wav",
                CoverGradient = "linear-gradient(135deg, #FF6B6B 0%, #FFE66D 100%)",
                MoodTags = new List<string> { "Happy", "Party", "Energetic", "Summer", "Uplifting" },
                Description = "A vibrant, infectious dance rhythm loaded with bright piano chords and driving kick drums to match peak joy.",
                Bpm = 124,
                KeySignature = "C Major"
            },
            new Track
            {
                Id = "trk-happy-02",
                Title = "Electric Radiance",
                Artist = "Neon Sunrise",
                Genre = "Energetic Synthwave",
                DurationSeconds = 24,
                Emotion = EmotionType.Happy,
                AudioUrl = "/audio/happy_2.wav",
                CoverGradient = "linear-gradient(135deg, #F37335 0%, #FDC830 100%)",
                MoodTags = new List<string> { "Happy", "Energetic", "Future Pop", "Vibrant" },
                Description = "Upbeat synths and celebratory melodic hooks designed to maintain momentum and elevate positive energy.",
                Bpm = 128,
                KeySignature = "G Major"
            },
            new Track
            {
                Id = "trk-happy-03",
                Title = "Celebration Groove",
                Artist = "The Funk Collective",
                Genre = "Funky Disco & Pop",
                DurationSeconds = 24,
                Emotion = EmotionType.Happy,
                AudioUrl = "/audio/happy_3.wav",
                CoverGradient = "linear-gradient(135deg, #FA709A 0%, #FEE140 100%)",
                MoodTags = new List<string> { "Party", "Happy", "Funky", "Feel-Good" },
                Description = "Bouncy rhythmic bassline and syncopated keys that invite spontaneous dancing and bright celebrations.",
                Bpm = 120,
                KeySignature = "F Major"
            },

            // Sad → Emotional, Acoustic, Sad
            new Track
            {
                Id = "trk-sad-01",
                Title = "Rainy Reflections",
                Artist = "Nocturne Trio",
                Genre = "Emotional Acoustic Piano",
                DurationSeconds = 26,
                Emotion = EmotionType.Sad,
                AudioUrl = "/audio/sad_1.wav",
                CoverGradient = "linear-gradient(135deg, #4A00E0 0%, #8E2DE2 100%)",
                MoodTags = new List<string> { "Emotional", "Acoustic", "Sad", "Contemplative" },
                Description = "Delicate minor-key piano arpeggios that resonate with contemplative melancholy and deep emotional release.",
                Bpm = 68,
                KeySignature = "A Minor"
            },
            new Track
            {
                Id = "trk-sad-02",
                Title = "Autumn Reverie",
                Artist = "Cello & Pine",
                Genre = "Acoustic Folk Ballad",
                DurationSeconds = 26,
                Emotion = EmotionType.Sad,
                AudioUrl = "/audio/sad_2.wav",
                CoverGradient = "linear-gradient(135deg, #2C3E50 0%, #4CA1AF 100%)",
                MoodTags = new List<string> { "Sad", "Acoustic", "Warmth", "Solitude" },
                Description = "Intimate acoustic guitar fingerpicking layered with warm cello swells for introspective and cathartic moments.",
                Bpm = 72,
                KeySignature = "D Minor"
            },
            new Track
            {
                Id = "trk-sad-03",
                Title = "Echoes in the Mist",
                Artist = "Silvia Thorne",
                Genre = "Emotional Ambient String",
                DurationSeconds = 26,
                Emotion = EmotionType.Sad,
                AudioUrl = "/audio/sad_3.wav",
                CoverGradient = "linear-gradient(135deg, #1F1C2C 0%, #928DAB 100%)",
                MoodTags = new List<string> { "Emotional", "Sad", "Atmospheric", "Gentle" },
                Description = "Slow-blooming pads and tender minor cadence designed to provide comfort without forcing hasty cheerfulness.",
                Bpm = 65,
                KeySignature = "E Minor"
            },

            // Angry → Calm, Relaxing, Instrumental
            new Track
            {
                Id = "trk-angry-01",
                Title = "Tranquil Waters",
                Artist = "Zenith Sanctuary",
                Genre = "Calm Ambient Soundscape",
                DurationSeconds = 28,
                Emotion = EmotionType.Angry,
                AudioUrl = "/audio/angry_1.wav",
                CoverGradient = "linear-gradient(135deg, #11998E 0%, #38EF7D 100%)",
                MoodTags = new List<string> { "Calm", "Relaxing", "Instrumental", "Stress-Relief" },
                Description = "Soft ocean-like swells and 432 Hz calming tones crafted to lower heart rate and restore peaceful clarity.",
                Bpm = 56,
                KeySignature = "D Major"
            },
            new Track
            {
                Id = "trk-angry-02",
                Title = "Breath & Balance",
                Artist = "Serenade Mind",
                Genre = "Relaxing Zen Instrumental",
                DurationSeconds = 28,
                Emotion = EmotionType.Angry,
                AudioUrl = "/audio/angry_2.wav",
                CoverGradient = "linear-gradient(135deg, #0BA360 0%, #3CBA92 100%)",
                MoodTags = new List<string> { "Calm", "Relaxing", "Meditation", "Peaceful" },
                Description = "Resonant Tibetan singing bowl overtones and gentle bamboo chimes designed to dissolve tension and frustration.",
                Bpm = 60,
                KeySignature = "Pentatonic Neutral"
            },
            new Track
            {
                Id = "trk-angry-03",
                Title = "Deep Horizon",
                Artist = "Aura Bloom",
                Genre = "Instrumental Ambient Pad",
                DurationSeconds = 28,
                Emotion = EmotionType.Angry,
                AudioUrl = "/audio/angry_3.wav",
                CoverGradient = "linear-gradient(135deg, #00B4DB 0%, #0083B0 100%)",
                MoodTags = new List<string> { "Calm", "Instrumental", "Soothing", "Focus" },
                Description = "Spacious, non-intrusive harmonic pads that ground turbulent emotions into an even, composed mental state.",
                Bpm = 54,
                KeySignature = "A Major"
            },

            // Surprised → Upbeat, Pop, Energetic
            new Track
            {
                Id = "trk-surprised-01",
                Title = "Cosmic Spark",
                Artist = "Starlight Orbit",
                Genre = "Upbeat Pop & Synthwave",
                DurationSeconds = 24,
                Emotion = EmotionType.Surprised,
                AudioUrl = "/audio/surprised_1.wav",
                CoverGradient = "linear-gradient(135deg, #FF0844 0%, #FFB199 100%)",
                MoodTags = new List<string> { "Upbeat", "Pop", "Energetic", "Sparkling" },
                Description = "Dynamic arpeggiated synths and snappy rhythms that ride the exhilarating wave of novelty and excitement.",
                Bpm = 132,
                KeySignature = "F Major"
            },
            new Track
            {
                Id = "trk-surprised-02",
                Title = "Neon Velocity",
                Artist = "Hyperdrive",
                Genre = "Energetic Future Pop",
                DurationSeconds = 24,
                Emotion = EmotionType.Surprised,
                AudioUrl = "/audio/surprised_2.wav",
                CoverGradient = "linear-gradient(135deg, #B224EF 0%, #7579FF 100%)",
                MoodTags = new List<string> { "Energetic", "Upbeat", "Electro", "Vibrant" },
                Description = "High-octane pulses and rapid chord modulations matching unexpected revelation and intense curiosity.",
                Bpm = 136,
                KeySignature = "G Major"
            },
            new Track
            {
                Id = "trk-surprised-03",
                Title = "Quantum Wonder",
                Artist = "Prism Theory",
                Genre = "Upbeat Electro Bounce",
                DurationSeconds = 24,
                Emotion = EmotionType.Surprised,
                AudioUrl = "/audio/surprised_3.wav",
                CoverGradient = "linear-gradient(135deg, #FF5858 0%, #F09819 100%)",
                MoodTags = new List<string> { "Pop", "Energetic", "Playful", "Wonder" },
                Description = "Glitchy, lively basslines paired with sparkling accents that channel the playful side of spontaneous surprises.",
                Bpm = 130,
                KeySignature = "A Minor"
            },

            // Neutral → Chill, Lo-fi, Balanced
            new Track
            {
                Id = "trk-neutral-01",
                Title = "Sunday Lo-Fi Study",
                Artist = "Mellow Café",
                Genre = "Chill Lo-fi Hip Hop",
                DurationSeconds = 26,
                Emotion = EmotionType.Neutral,
                AudioUrl = "/audio/neutral_1.wav",
                CoverGradient = "linear-gradient(135deg, #89F7FE 0%, #66A6FF 100%)",
                MoodTags = new List<string> { "Chill", "Lo-fi", "Balanced", "Study", "Focus" },
                Description = "Warming Rhodes electric piano, nostalgic vinyl crackle, and an easy boom-bap beat for centered calm.",
                Bpm = 78,
                KeySignature = "C Major 7"
            },
            new Track
            {
                Id = "trk-neutral-02",
                Title = "Chillout Lounge",
                Artist = "Velvet Breeze",
                Genre = "Balanced Downtempo",
                DurationSeconds = 26,
                Emotion = EmotionType.Neutral,
                AudioUrl = "/audio/neutral_2.wav",
                CoverGradient = "linear-gradient(135deg, #667EEA 0%, #764BA2 100%)",
                MoodTags = new List<string> { "Chill", "Balanced", "Lounge", "Mellow" },
                Description = "Subtle acoustic bass and jazzy chords that maintain equilibrium during everyday workflow or relaxed downtime.",
                Bpm = 82,
                KeySignature = "D Minor 9"
            },
            new Track
            {
                Id = "trk-neutral-03",
                Title = "Quiet Afternoon",
                Artist = "Komorebi Beats",
                Genre = "Lo-fi Jazzhop",
                DurationSeconds = 26,
                Emotion = EmotionType.Neutral,
                AudioUrl = "/audio/neutral_3.wav",
                CoverGradient = "linear-gradient(135deg, #E0C3FC 0%, #8EC5FC 100%)",
                MoodTags = new List<string> { "Lo-fi", "Chill", "Balanced", "Atmospheric" },
                Description = "Gentle finger snaps and smooth harmonic progressions that enhance presence and sustained concentration.",
                Bpm = 75,
                KeySignature = "G Major"
            },

            // Fearful → Gentle, Calm, Soothing
            new Track
            {
                Id = "trk-fearful-01",
                Title = "Safe Haven",
                Artist = "Lullaby & Fern",
                Genre = "Gentle Soothing Lullaby",
                DurationSeconds = 28,
                Emotion = EmotionType.Fearful,
                AudioUrl = "/audio/fearful_1.wav",
                CoverGradient = "linear-gradient(135deg, #A8EDEA 0%, #FED6E3 100%)",
                MoodTags = new List<string> { "Gentle", "Calm", "Soothing", "Safe", "Reassurance" },
                Description = "Sweet music-box chimes and a slow comforting heartbeat pulse designed to instill a feeling of safety and protection.",
                Bpm = 54,
                KeySignature = "C Major"
            },
            new Track
            {
                Id = "trk-fearful-02",
                Title = "Gentle Sanctuary",
                Artist = "Harps of Solace",
                Genre = "Calm Acoustic Harp",
                DurationSeconds = 28,
                Emotion = EmotionType.Fearful,
                AudioUrl = "/audio/fearful_2.wav",
                CoverGradient = "linear-gradient(135deg, #D4FC79 0%, #96E6A1 100%)",
                MoodTags = new List<string> { "Gentle", "Soothing", "Warmth", "Grounding" },
                Description = "Reassuring warm harp plucking and tender pads that envelop the listener in a calm, protective auditory blanket.",
                Bpm = 52,
                KeySignature = "F Major 7"
            },
            new Track
            {
                Id = "trk-fearful-03",
                Title = "Guiding Light",
                Artist = "Aetheria",
                Genre = "Soothing Ambient Glow",
                DurationSeconds = 28,
                Emotion = EmotionType.Fearful,
                AudioUrl = "/audio/fearful_3.wav",
                CoverGradient = "linear-gradient(135deg, #FBC2EB 0%, #A6C1EE 100%)",
                MoodTags = new List<string> { "Calm", "Gentle", "Soothing", "Hopeful" },
                Description = "Slow harmonic swells with tender acoustic chimes offering grounding reassurance against anxiety and apprehension.",
                Bpm = 58,
                KeySignature = "G Major"
            }
        };
    }

    public IReadOnlyList<Track> GetAllTracks() => _catalog.AsReadOnly();

    public Track? GetTrackById(string id) => _catalog.FirstOrDefault(t => t.Id == id);

    public IReadOnlyList<Track> GetTracksByEmotion(EmotionType emotion) =>
        _catalog.Where(t => t.Emotion == emotion).ToList().AsReadOnly();

    public IReadOnlyList<string> GetGenresForEmotion(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => new List<string> { "Happy", "Party", "Energetic" },
            EmotionType.Sad => new List<string> { "Emotional", "Acoustic", "Sad" },
            EmotionType.Angry => new List<string> { "Calm", "Relaxing", "Instrumental" },
            EmotionType.Surprised => new List<string> { "Upbeat", "Pop", "Energetic" },
            EmotionType.Neutral => new List<string> { "Chill", "Lo-fi", "Balanced" },
            EmotionType.Fearful => new List<string> { "Gentle", "Calm", "Soothing" },
            _ => new List<string> { "Chill", "Balanced" }
        };
    }

    public string GetEmotionDescription(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => "Elevated cheek muscles, genuine zygomatic smile lift, and relaxed radiant expression.",
            EmotionType.Sad => "Downturned lip corners, elevated inner brow arch, and soft contemplative gaze.",
            EmotionType.Angry => "Furrowed brow (corrugator activation), compressed lips, and heightened tension.",
            EmotionType.Surprised => "Opened jaw, widened eyes, and elevated outer brows reflecting novelty or wonder.",
            EmotionType.Neutral => "Relaxed facial muscles, balanced lip line, and calm centered baseline state.",
            EmotionType.Fearful => "Widened eyes, raised inner brows, and tense lip stretch indicating apprehension.",
            _ => "Balanced baseline expression."
        };
    }

    public string GetMusicalRationale(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => "Recommending upbeat party and energetic anthems to synchronize with your vibrant joy and keep the celebratory momentum going.",
            EmotionType.Sad => "Recommending emotive, acoustic, and melancholic music to validate your feelings and provide gentle, cathartic companionship without rushing healing.",
            EmotionType.Angry => "Recommending calm, relaxing, instrumental tones and 432 Hz ambient soundscapes designed to de-escalate tension, steady breathing, and restore composure.",
            EmotionType.Surprised => "Recommending upbeat pop and energetic synthwave tracks that harness your sudden burst of adrenaline and spark creative enthusiasm.",
            EmotionType.Neutral => "Recommending chill, lo-fi, and balanced downtempo beats that support steady focus, calm mindfulness, and relaxed everyday equilibrium.",
            EmotionType.Fearful => "Recommending gentle, calm, and soothing melodies with protective music-box and warm harp tones to ground apprehension into a safe, reassuring embrace.",
            _ => "Recommending balanced, harmonious music aligned with your current state."
        };
    }

    public RecommendationResponse GenerateRecommendation(RecommendationRequest request)
    {
        if (!Enum.TryParse<EmotionType>(request.Emotion, true, out var emotion))
        {
            emotion = EmotionType.Neutral;
        }

        double confidence = Math.Clamp(request.Confidence, 0.0, 1.0);
        bool isUncertain = confidence < 0.40;

        string confidenceLevel = confidence switch
        {
            >= 0.75 => "High",
            >= 0.55 => "Moderate",
            >= 0.40 => "Subtle",
            _ => "Uncertain"
        };

        var tracks = GetTracksByEmotion(emotion).ToList();

        // If user specifically filtered by genre
        if (!string.IsNullOrWhiteSpace(request.SelectedGenre))
        {
            var filtered = tracks.Where(t => t.Genre.Contains(request.SelectedGenre, StringComparison.OrdinalIgnoreCase) ||
                                            t.MoodTags.Any(m => m.Equals(request.SelectedGenre, StringComparison.OrdinalIgnoreCase))).ToList();
            if (filtered.Any())
            {
                tracks = filtered;
            }
        }

        string? guidanceMessage = null;
        if (isUncertain)
        {
            guidanceMessage = "The detected facial expression is subtle or ambiguous. For best accuracy, look directly into the camera, ensure balanced ambient lighting without harsh backlighting, and let your natural emotion show.";
        }
        else if (request.Quality != null && !request.Quality.IsAcceptable && request.Quality.Warnings.Any())
        {
            guidanceMessage = string.Join(" ", request.Quality.Warnings);
        }

        var response = new RecommendationResponse
        {
            DetectedEmotion = emotion.ToString(),
            Confidence = Math.Round(confidence, 2),
            ConfidenceLevel = confidenceLevel,
            EmotionDescription = GetEmotionDescription(emotion),
            MusicalRationale = GetMusicalRationale(emotion),
            RecommendedGenres = GetGenresForEmotion(emotion).ToList(),
            Tracks = tracks,
            Quality = request.Quality,
            IsUncertain = isUncertain,
            GuidanceMessage = guidanceMessage,
            Breakdown = BuildScoreBreakdown(emotion, confidence, request.Blendshapes)
        };

        return response;
    }

    private List<EmotionScoreBreakdown> BuildScoreBreakdown(EmotionType primaryEmotion, double primaryConfidence, Dictionary<string, double>? blendshapes)
    {
        var list = new List<EmotionScoreBreakdown>();

        foreach (EmotionType e in Enum.GetValues<EmotionType>())
        {
            double score;
            string keySignals;

            if (e == primaryEmotion)
            {
                score = Math.Round(primaryConfidence, 2);
                keySignals = GetKeySignalSummary(e, blendshapes);
            }
            else
            {
                // Secondary non-dominant score proportional to distance
                score = Math.Round(Math.Max(0.05, (1.0 - primaryConfidence) / 5.0), 2);
                keySignals = "Secondary or baseline activity";
            }

            list.Add(new EmotionScoreBreakdown
            {
                Emotion = e.ToString(),
                Score = score,
                KeySignals = keySignals
            });
        }

        return list.OrderByDescending(b => b.Score).ToList();
    }

    private string GetKeySignalSummary(EmotionType emotion, Dictionary<string, double>? blendshapes)
    {
        if (blendshapes == null || !blendshapes.Any())
        {
            return "Landmark contour and geometric proportion analysis";
        }

        return emotion switch
        {
            EmotionType.Happy => "Zygomatic smile lift, cheek elevation, relaxed brow",
            EmotionType.Sad => "Downturned lip corners, elevated inner brow, lowered eyelid tension",
            EmotionType.Angry => "Corrugator eyebrow furrow, compressed lip margin, focused stare",
            EmotionType.Surprised => "Jaw drop opening, widened palpebral fissure, arched brow elevation",
            EmotionType.Neutral => "Even resting muscle tone, balanced lip line, neutral brow",
            EmotionType.Fearful => "Wide eye opening, inner brow rise with horizontal lip stretch",
            _ => "Facial blendshape integration"
        };
    }
}
