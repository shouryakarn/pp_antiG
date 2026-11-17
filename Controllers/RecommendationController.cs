using Microsoft.AspNetCore.Mvc;
using EmotionMusicAnalyzer.Models;
using EmotionMusicAnalyzer.Services;

namespace EmotionMusicAnalyzer.Controllers;

[ApiController]
[Route("api")]
public class RecommendationController : ControllerBase
{
    private readonly IMusicService _musicService;
    private readonly IImageQualityService _qualityService;

    public RecommendationController(IMusicService musicService, IImageQualityService qualityService)
    {
        _musicService = musicService;
        _qualityService = qualityService;
    }

    [HttpPost("recommendations")]
    public ActionResult<RecommendationResponse> GetRecommendations([FromBody] RecommendationRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Invalid request payload." });
        }

        var response = _musicService.GenerateRecommendation(request);
        return Ok(response);
    }

    [HttpGet("tracks")]
    public ActionResult<IEnumerable<Track>> GetTracks([FromQuery] string? emotion)
    {
        if (!string.IsNullOrWhiteSpace(emotion) && Enum.TryParse<EmotionType>(emotion, true, out var emotionType))
        {
            return Ok(_musicService.GetTracksByEmotion(emotionType));
        }

        return Ok(_musicService.GetAllTracks());
    }

    [HttpGet("tracks/{id}")]
    public ActionResult<Track> GetTrack(string id)
    {
        var track = _musicService.GetTrackById(id);
        if (track == null)
        {
            return NotFound(new { error = $"Track with ID '{id}' was not found." });
        }

        return Ok(track);
    }

    [HttpPost("assess-image")]
    public ActionResult<ImageAssessmentResponse> AssessImage([FromBody] ImageAssessmentRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ImageBase64))
        {
            return BadRequest(new { error = "Image base64 data is required." });
        }

        var assessment = _qualityService.AssessImage(request.ImageBase64);
        return Ok(assessment);
    }

    [HttpPost("assess-metrics")]
    public ActionResult<ImageQualityMetrics> AssessMetrics([FromBody] ImageQualityMetrics clientMetrics)
    {
        if (clientMetrics == null)
        {
            return BadRequest(new { error = "Metrics payload is required." });
        }

        var result = _qualityService.AssessMetrics(
            clientMetrics.Width,
            clientMetrics.Height,
            clientMetrics.Brightness,
            clientMetrics.Contrast,
            clientMetrics.BlurVariance);

        return Ok(result);
    }
}
