namespace EmotionMusicAnalyzer.Models;

public class RecommendationResponse
{
    public string DetectedEmotion { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string ConfidenceLevel { get; set; } = string.Empty; // "High", "Moderate", "Subtle", "Uncertain"
    public string EmotionDescription { get; set; } = string.Empty;
    public string MusicalRationale { get; set; } = string.Empty;
    public List<string> RecommendedGenres { get; set; } = new();
    public List<Track> Tracks { get; set; } = new();
    public ImageQualityMetrics? Quality { get; set; }
    public bool IsUncertain { get; set; }
    public string? GuidanceMessage { get; set; }
    public List<EmotionScoreBreakdown> Breakdown { get; set; } = new();
}

public class EmotionScoreBreakdown
{
    public string Emotion { get; set; } = string.Empty;
    public double Score { get; set; }
    public string KeySignals { get; set; } = string.Empty;
}
