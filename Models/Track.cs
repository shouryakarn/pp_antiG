namespace EmotionMusicAnalyzer.Models;

public class Track
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public EmotionType Emotion { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public string CoverGradient { get; set; } = string.Empty;
    public List<string> MoodTags { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public int Bpm { get; set; }
    public string KeySignature { get; set; } = string.Empty;
}
