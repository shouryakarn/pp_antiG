namespace EmotionMusicAnalyzer.Models;

public class ImageQualityMetrics
{
    public int Width { get; set; }
    public int Height { get; set; }
    public double Brightness { get; set; } // 0 to 255
    public double Contrast { get; set; }
    public double BlurVariance { get; set; }
    public bool IsAcceptable { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}
