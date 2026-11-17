using EmotionMusicAnalyzer.Models;

namespace EmotionMusicAnalyzer.Services;

public interface IImageQualityService
{
    ImageAssessmentResponse AssessImage(string base64Data);
    ImageQualityMetrics AssessMetrics(int width, int height, double brightness, double contrast, double blurVariance);
}
