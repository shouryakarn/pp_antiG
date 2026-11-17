namespace EmotionMusicAnalyzer.Models;

public class ImageAssessmentRequest
{
    public string? ImageBase64 { get; set; }
}

public class ImageAssessmentResponse
{
    public bool IsValid { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double Brightness { get; set; }
    public double Contrast { get; set; }
    public double BlurVariance { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}
