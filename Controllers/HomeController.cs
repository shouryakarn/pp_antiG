using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EmotionMusicAnalyzer.Models;
using EmotionMusicAnalyzer.Services;

namespace EmotionMusicAnalyzer.Controllers;

public class HomeController : Controller
{
    private readonly IMusicService _musicService;

    public HomeController(IMusicService musicService)
    {
        _musicService = musicService;
    }

    public IActionResult Index()
    {
        var allTracks = _musicService.GetAllTracks();
        ViewBag.TotalTracks = allTracks.Count;
        return View(allTracks);
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
