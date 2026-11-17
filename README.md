# EmotionMusicAnalyzer — Emotion-Based Music Recommendation System

**EmotionMusicAnalyzer** is an emotion-driven music recommendation application built from scratch with **ASP.NET Core MVC (.NET 10)**, **MediaPipe Face Landmarker**, and **HTML5 Audio**.

It accurately interprets real-time facial expressions from a **live webcam** or **uploaded photograph (JPG, JPEG, PNG)**, assesses image lighting and quality, classifies affective states without displaying intrusive landmark dots or meshes, and streams curated, royalty-free audio compositions tailored to each emotion.

---

## 🌟 Key Features

1. **Dual Capture Modes**:
   - **Live Webcam Mode**: Real-time video preview with instant expression capture and camera permission handling.
   - **Photograph Upload Mode**: Drag-and-drop or file selection for JPG, JPEG, and PNG images, plus 6 instant sample test portraits (Happy, Surprised, Neutral, Sad, Angry, Fearful).
2. **Lightweight Client-Side Image Preprocessing**:
   - Evaluates image dimensions (flags < 120x120 or downscales oversized images up to 1280px preserving aspect ratio).
   - Computes grayscale brightness and contrast metrics.
   - Performs Laplacian variance convolution to detect motion blur and soft focus.
   - Applies adaptive exposure compensation to recover facial contours under dim ambient light.
   - Returns actionable feedback if an image is unsuitable rather than returning deceptive predictions.
3. **Genuine Facial Expression Analysis**:
   - Powered by Google's **MediaPipe Face Landmarker** running client-side via WebAssembly/WebGL shaders.
   - Evaluates **52 blendshape coefficients** (action units like `mouthSmileLeft/Right`, `mouthFrownLeft/Right`, `browDownLeft/Right`, `browInnerUp`, `jawOpen`, `eyeWideLeft/Right`, `mouthStretchLeft/Right`) combined with **3D landmark geometric ratios** (lip corner elevation, mouth aspect ratio).
   - Generates genuine confidence scores and separation margins.
   - Flags uncertainty when expressions are ambiguous and invites the user to retry with clear guidance.
   - **Clean UI Guarantee**: Facial landmark dots, connected lines, and wireframe meshes are intentionally not displayed.
4. **Affective Music Recommendation Engine**:
   - Built on ASP.NET Core MVC service layer (`IMusicService`).
   - Implements strict therapeutic and affective mapping rules.
5. **Interactive HTML5 Audio Playback**:
   - Pre-generated multi-track, 16-bit 44.1kHz stereo PCM audio library (18 tracks total, 3 per emotion).
   - Persistent floating player dock with progress scrubber, seek controls, animated sound-wave visualizer, and volume/mute controls.
   - **Strict Single-Track Playback**: Automatically pauses any active song when another track is selected.

---

## 🎵 Emotion-to-Music Recommendation Matrix

| Detected Emotion | Facial Action Units & Markers | Target Genres | Musical Properties | Affective & Therapeutic Rationale |
| :--- | :--- | :--- | :--- | :--- |
| **Happy** | Zygomatic smile lift, cheek elevation, relaxed brow | **Happy, Party, Energetic** | 120–128 BPM, Major keys (C/G Maj) | Synchronize with elevated mood and sustain celebratory dopamine momentum. |
| **Sad** | Depressor anguli oris frown, elevated inner brow arch | **Emotional, Acoustic, Sad** | 65–72 BPM, Minor keys (Am/Dm) | Validate emotional state; provide gentle catharsis without rushing recovery. |
| **Angry** | Corrugator brow furrow, compressed lips, focused stare | **Calm, Relaxing, Instrumental** | 54–60 BPM, 432 Hz Zen drone & bells | De-escalate physiological tension, steady breathing, and restore composed calm. |
| **Surprised** | Dropped jaw opening, wide eye fissure, elevated brows | **Upbeat, Pop, Energetic** | 130–136 BPM, Vibrant Synthwave | Harness sudden adrenaline surge; channel wonder into creative curiosity. |
| **Neutral** | Relaxed resting facial muscle tone, balanced lip line | **Chill, Lo-fi, Balanced** | 75–82 BPM, Rhodes Lo-fi Jazzhop | Maintain mindfulness, support calm focus, and sustain daily equilibrium. |
| **Fearful** | Wide palpebral eye opening, lateral lip tension stretch | **Gentle, Calm, Soothing** | 52–58 BPM, Music Box & Harp Lullaby | Ground apprehension; provide protective auditory warmth and reassurance. |

---

## 🛠️ Technology Stack

- **Backend**: C# 13, ASP.NET Core MVC, .NET 10.0 (Kestrel web server)
- **Frontend**: HTML5, CSS3 (Modern Glassmorphism & Custom Dark Theme), JavaScript (ES Modules), Bootstrap 5.3.3, Bootstrap Icons 1.11.3
- **Computer Vision & AI**: MediaPipe Tasks Vision FaceLandmarker (0.10.14) with local WASM and task model asset fallback
- **Audio Engine**: Native HTML5 Audio API with custom UI synchronization

---

## 📁 Project Architecture

```
EmotionMusicAnalyzer/
├── Controllers/
│   ├── HomeController.cs              # Serves Studio view (Index) & Architecture documentation (About)
│   └── RecommendationController.cs    # REST API (/api/recommendations, /api/tracks, /api/assess-image)
├── Models/
│   ├── EmotionType.cs                 # Enum: Happy, Sad, Angry, Surprised, Neutral, Fearful
│   ├── Track.cs                       # Song entity (Id, Title, Artist, Genre, AudioUrl, MoodTags, etc.)
│   ├── ImageQualityMetrics.cs         # Brightness, Contrast, BlurVariance, Warnings, Suggestions
│   ├── RecommendationRequest.cs       # Request payload (Emotion, Confidence, Blendshapes, Quality)
│   ├── RecommendationResponse.cs      # Response payload (Tracks, Breakdown, Rationale, Guidance)
│   └── ImageAssessmentModels.cs       # Server-side validation models
├── Services/
│   ├── IMusicService.cs               # Music recommendation contract
│   ├── MusicService.cs                # In-memory catalog of 18 tracks & emotion mapping rules
│   ├── AudioTrackGenerator.cs         # Synthesizes 18 multi-track 44.1kHz stereo WAV files on startup
│   ├── IImageQualityService.cs        # Quality check contract
│   └── ImageQualityService.cs         # Image format and quality assessment
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml               # Main Studio UI (Webcam, Upload, Results, Music Player)
│   │   └── About.cshtml               # Architecture, flow diagram, and mapping documentation
│   └── Shared/
│       └── _Layout.cshtml             # Master dark layout, sticky player dock, and navigation
├── wwwroot/
│   ├── audio/                         # Synthesized 16-bit PCM WAV tracks for all 6 emotions
│   ├── css/
│   │   └── site.css                   # Glassmorphic dark design system and responsive tokens
│   ├── js/
│   │   ├── image-preprocessor.js      # Client-side canvas preprocessing, blur/exposure metrics
│   │   ├── emotion-engine.js          # MediaPipe FaceLandmarker loader & blendshape classification
│   │   ├── audio-player.js            # HTML5 Audio controller (enforces single active track)
│   │   └── app.js                     # Main client orchestrator connecting UI, vision, and API
│   ├── lib/
│   │   └── tasks-vision/              # Local MediaPipe Tasks Vision JS and WASM binaries
│   ├── models/
│   │   └── face_landmarker.task       # MediaPipe FaceLandmarker float16 neural model (3.75 MB)
│   └── samples/                       # Verified sample portraits (Happy, Surprised, Neutral, Sad, Angry, Fearful)
├── Program.cs                         # ASP.NET Core 10 pipeline, static file MIME types, startup routines
└── EmotionMusicAnalyzer.csproj        # .NET 10 project file
```

---

## 🚀 Setup & Launch Instructions (Windows)

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download) installed on Windows.
- Modern web browser (Google Chrome, Microsoft Edge, Mozilla Firefox, or Safari).

### Exact Windows Terminal / PowerShell Commands

1. **Navigate to the project directory**:
   ```powershell
   cd c:\Users\hp\Desktop\pp_antiG
   ```

2. **Restore dependencies**:
   ```powershell
   dotnet restore
   ```

3. **Build the application**:
   ```powershell
   dotnet build
   ```

4. **Run the application**:
   ```powershell
   dotnet run
   ```

5. **Open in browser**:
   Navigate to the local URL displayed in the terminal:
   ```
   http://localhost:5214
   ```

---

## 🧪 Testing the Application

### 1. Photo Upload / Sample Portrait Flow
1. Navigate to `http://localhost:5214`.
2. Click the **"Upload Photograph"** tab.
3. Click any of the **Quick-Test Sample Portraits** (Happy, Surprised, Neutral, Sad, Angry, Fearful), or drag and drop your own photo file.
4. The system will run the image preprocessor, invoke MediaPipe Face Landmarker, classify the emotion, and request matching tracks from the ASP.NET Core backend.
5. Review the **Emotion Badge**, **Confidence Rating**, **Facial Expression Interpretation**, and **Therapeutic Musical Rationale**.

### 2. Audio Playback Flow
1. Under **Recommended Tracks**, click the **Play** button on any track card.
2. The persistent **Sticky Audio Player Dock** at the bottom will display the track title, artist, animated equalizer bars, and seek bar.
3. Click the **Play** button on another track. Notice that the previous track stops immediately and only the new track plays (strictly single playback).
4. Use the player dock scrubber to seek forward/backward, adjust volume, or click previous/next track.

### 3. Live Webcam Flow
1. Click the **"Live Webcam"** tab.
2. Click **"Start Webcam"** and allow camera permissions in your browser.
3. Make an expression (e.g. smile or open your mouth in surprise) and click **"Analyze My Expression"**.
4. The application analyzes your video frame directly in the browser and updates the recommendations.

---

## 🛡️ Privacy & Performance Notes

- **Zero Server Image Upload**: Facial landmark detection and blendshape evaluation run 100% inside your browser using WebAssembly. Facial photographs and camera video are never sent to or stored on the server.
- **Offline Audio**: All 18 music tracks are generated locally into `wwwroot/audio/` and served directly by ASP.NET Core with standard MIME types, requiring no external streaming services or third-party API keys.
