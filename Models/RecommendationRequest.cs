namespace EmotionMusicAnalyzer.Models;

public class RecommendationRequest
{
    public string Emotion { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, double>? Blendshapes { get; set; }
    public Dictionary<string, double>? GeometricMetrics { get; set; }
    public ImageQualityMetrics? Quality { get; set; }
    public string? SourceType { get; set; } // "webcam" or "upload"
    public string? SelectedGenre { get; set; }
}
