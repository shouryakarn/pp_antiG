using EmotionMusicAnalyzer.Models;

namespace EmotionMusicAnalyzer.Services;

public interface IMusicService
{
    IReadOnlyList<Track> GetAllTracks();
    Track? GetTrackById(string id);
    IReadOnlyList<Track> GetTracksByEmotion(EmotionType emotion);
    RecommendationResponse GenerateRecommendation(RecommendationRequest request);
    IReadOnlyList<string> GetGenresForEmotion(EmotionType emotion);
    string GetEmotionDescription(EmotionType emotion);
    string GetMusicalRationale(EmotionType emotion);
}
