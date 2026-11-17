using Microsoft.AspNetCore.StaticFiles;
using EmotionMusicAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register application domain services
builder.Services.AddSingleton<IMusicService, MusicService>();
builder.Services.AddSingleton<IImageQualityService, ImageQualityService>();

var app = builder.Build();

// Configure static file options with explicit MIME types for MediaPipe WASM, model assets, and audio files
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".mjs"] = "application/javascript";
contentTypeProvider.Mappings[".wasm"] = "application/wasm";
contentTypeProvider.Mappings[".task"] = "application/octet-stream";
contentTypeProvider.Mappings[".wav"] = "audio/wav";
contentTypeProvider.Mappings[".mp3"] = "audio/mpeg";

var staticFileOptions = new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
};

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Ensure all synthesized audio tracks are pre-generated on startup
try
{
    var webRoot = app.Environment.WebRootPath;
    if (string.IsNullOrEmpty(webRoot))
    {
        webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }
    AudioTrackGenerator.EnsureTracksGenerated(webRoot);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Error pre-generating audio tracks on startup.");
}

app.UseStaticFiles(staticFileOptions);
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
