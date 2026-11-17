/**
 * Main Application Orchestrator
 * Integrates ImagePreprocessor, EmotionEngine, AudioPlayer, and backend Recommendation API.
 */

import { ImagePreprocessor } from "./image-preprocessor.js";
import { EmotionEngine } from "./emotion-engine.js";
import { AudioPlayer } from "./audio-player.js";

document.addEventListener("DOMContentLoaded", async () => {
    // 1. Initialize core modules
    const preprocessor = new ImagePreprocessor();
    const emotionEngine = new EmotionEngine();
    const audioPlayer = new AudioPlayer();

    // Application state
    let webcamStream = null;
    let isWebcamRunning = false;
    let currentRecommendations = [];
    let activeEmotion = null;

    // DOM References
    // Tabs
    const tabWebcamBtn = document.getElementById("tab-webcam-btn");
    const tabUploadBtn = document.getElementById("tab-upload-btn");
    const viewWebcam = document.getElementById("view-webcam");
    const viewUpload = document.getElementById("view-upload");

    // Webcam elements
    const webcamVideo = document.getElementById("webcam-video");
    const btnStartCamera = document.getElementById("btn-start-camera");
    const btnCaptureWebcam = document.getElementById("btn-capture-webcam");
    const btnStopCamera = document.getElementById("btn-stop-camera");
    const cameraPlaceholder = document.getElementById("camera-placeholder");
    const cameraLiveBadge = document.getElementById("camera-live-badge");

    // Upload elements
    const dropZone = document.getElementById("drop-zone");
    const fileInput = document.getElementById("file-input");
    const btnSelectFile = document.getElementById("btn-select-file");
    const uploadPreviewContainer = document.getElementById("upload-preview-container");
    const uploadPreviewImg = document.getElementById("upload-preview-img");
    const uploadPlaceholder = document.getElementById("upload-placeholder");
    const btnAnalyzeUpload = document.getElementById("btn-analyze-upload");
    const btnClearUpload = document.getElementById("btn-clear-upload");

    // Sample portrait triggers
    const sampleButtons = document.querySelectorAll("[data-sample-file]");

    // Status & Feedback
    const processingOverlay = document.getElementById("processing-overlay");
    const processingMessage = document.getElementById("processing-message");
    const guidanceAlert = document.getElementById("guidance-alert");
    const guidanceText = document.getElementById("guidance-text");

    // Results container
    const resultsContainer = document.getElementById("results-container");
    const emotionBadge = document.getElementById("emotion-badge");
    const confidenceBadge = document.getElementById("confidence-badge");
    const emotionDescription = document.getElementById("emotion-description");
    const musicalRationale = document.getElementById("musical-rationale");
    const qualityBadge = document.getElementById("quality-badge");
    const breakdownContainer = document.getElementById("breakdown-container");
    const signalsSummary = document.getElementById("signals-summary");
    const recommendationsGrid = document.getElementById("recommendations-grid");
    const genrePillsContainer = document.getElementById("genre-pills-container");

    // Sticky Player elements
    const playerDock = document.getElementById("player-dock");
    const playerCover = document.getElementById("player-cover");
    const playerTitle = document.getElementById("player-title");
    const playerArtist = document.getElementById("player-artist");
    const playerGenre = document.getElementById("player-genre");
    const btnPlayerPlay = document.getElementById("btn-player-play");
    const btnPlayerPrev = document.getElementById("btn-player-prev");
    const btnPlayerNext = document.getElementById("btn-player-next");
    const playerProgress = document.getElementById("player-progress");
    const playerProgressContainer = document.getElementById("player-progress-container");
    const playerCurrentTime = document.getElementById("player-current-time");
    const playerTotalTime = document.getElementById("player-total-time");
    const playerVolume = document.getElementById("player-volume");
    const btnPlayerMute = document.getElementById("btn-player-mute");
    const playerVisualizer = document.getElementById("player-visualizer");

    // Pre-initialize MediaPipe model in background
    emotionEngine.initialize().catch(err => {
        console.warn("MediaPipe background warmup noticed:", err);
    });

    // -------------------------------------------------------------
    // Tab Switching
    // -------------------------------------------------------------
    tabWebcamBtn.addEventListener("click", () => {
        tabWebcamBtn.classList.add("active");
        tabUploadBtn.classList.remove("active");
        viewWebcam.classList.remove("d-none");
        viewUpload.classList.add("d-none");
    });

    tabUploadBtn.addEventListener("click", () => {
        tabUploadBtn.classList.add("active");
        tabWebcamBtn.classList.remove("active");
        viewUpload.classList.remove("d-none");
        viewWebcam.classList.add("d-none");
        // Pause webcam stream if active to save resources
        if (isWebcamRunning) {
            stopWebcam();
        }
    });

    // -------------------------------------------------------------
    // Webcam Handlers
    // -------------------------------------------------------------
    btnStartCamera.addEventListener("click", async () => {
        await startWebcam();
    });

    btnStopCamera.addEventListener("click", () => {
        stopWebcam();
    });

    btnCaptureWebcam.addEventListener("click", async () => {
        if (!isWebcamRunning || !webcamVideo.videoWidth) return;
        await processAndAnalyze(webcamVideo, "webcam");
    });

    async function startWebcam() {
        try {
            showProcessing("Requesting camera access...");
            const constraints = {
                video: {
                    width: { ideal: 1280 },
                    height: { ideal: 720 },
                    facingMode: "user"
                },
                audio: false
            };

            webcamStream = await navigator.mediaDevices.getUserMedia(constraints);
            webcamVideo.srcObject = webcamStream;
            await webcamVideo.play();

            isWebcamRunning = true;
            cameraPlaceholder.classList.add("d-none");
            webcamVideo.classList.remove("d-none");
            cameraLiveBadge.classList.remove("d-none");
            btnStartCamera.classList.add("d-none");
            btnCaptureWebcam.classList.remove("d-none");
            btnStopCamera.classList.remove("d-none");
            hideProcessing();
        } catch (err) {
            hideProcessing();
            console.error("Camera access error:", err);
            showGuidance("Camera access was denied or is unavailable. Please check your browser permissions, or use the 'Upload Photograph' tab to analyze a photo.");
        }
    }

    function stopWebcam() {
        if (webcamStream) {
            webcamStream.getTracks().forEach(track => track.stop());
            webcamStream = null;
        }
        isWebcamRunning = false;
        webcamVideo.classList.add("d-none");
        cameraPlaceholder.classList.remove("d-none");
        cameraLiveBadge.classList.add("d-none");
        btnStartCamera.classList.remove("d-none");
        btnCaptureWebcam.classList.add("d-none");
        btnStopCamera.classList.add("d-none");
    }

    // -------------------------------------------------------------
    // Upload & Drag-and-Drop Handlers
    // -------------------------------------------------------------
    btnSelectFile.addEventListener("click", () => fileInput.click());

    fileInput.addEventListener("change", (e) => {
        if (e.target.files && e.target.files[0]) {
            handleSelectedFile(e.target.files[0]);
        }
    });

    dropZone.addEventListener("dragover", (e) => {
        e.preventDefault();
        dropZone.classList.add("drag-over");
    });

    dropZone.addEventListener("dragleave", () => {
        dropZone.classList.remove("drag-over");
    });

    dropZone.addEventListener("drop", (e) => {
        e.preventDefault();
        dropZone.classList.remove("drag-over");
        if (e.dataTransfer.files && e.dataTransfer.files[0]) {
            handleSelectedFile(e.dataTransfer.files[0]);
        }
    });

    function handleSelectedFile(file) {
        const validTypes = ["image/jpeg", "image/jpg", "image/png"];
        if (!validTypes.includes(file.type)) {
            showGuidance("Unsupported format. Please upload a standard JPG, JPEG, or PNG photograph.");
            return;
        }

        const reader = new FileReader();
        reader.onload = (e) => {
            uploadPreviewImg.src = e.target.result;
            uploadPlaceholder.classList.add("d-none");
            uploadPreviewContainer.classList.remove("d-none");
            btnAnalyzeUpload.disabled = false;
            hideGuidance();
        };
        reader.readAsDataURL(file);
    }

    btnClearUpload.addEventListener("click", () => {
        fileInput.value = "";
        uploadPreviewImg.src = "";
        uploadPreviewContainer.classList.add("d-none");
        uploadPlaceholder.classList.remove("d-none");
        btnAnalyzeUpload.disabled = true;
    });

    btnAnalyzeUpload.addEventListener("click", async () => {
        if (!uploadPreviewImg.src) return;
        await processAndAnalyze(uploadPreviewImg, "upload");
    });

    // Sample Portraits quick select
    sampleButtons.forEach(btn => {
        btn.addEventListener("click", async () => {
            const sampleUrl = btn.getAttribute("data-sample-file");
            if (!sampleUrl) return;

            // Switch to upload tab if on webcam
            tabUploadBtn.click();

            showProcessing("Loading sample portrait...");
            try {
                uploadPreviewImg.crossOrigin = "anonymous";
                uploadPreviewImg.onload = async () => {
                    uploadPlaceholder.classList.add("d-none");
                    uploadPreviewContainer.classList.remove("d-none");
                    btnAnalyzeUpload.disabled = false;
                    hideProcessing();
                    // Automatically trigger analysis for seamless demo
                    await processAndAnalyze(uploadPreviewImg, "sample");
                };
                uploadPreviewImg.src = sampleUrl;
            } catch (err) {
                hideProcessing();
                showGuidance("Failed to load sample image: " + err.message);
            }
        });
    });

    // -------------------------------------------------------------
    // Core Pipeline: Preprocessing → Analysis → Recommendation
    // -------------------------------------------------------------
    async function processAndAnalyze(sourceElement, sourceType) {
        try {
            showProcessing("1/3 Checking image quality & preprocessing...");
            hideGuidance();

            // Step 1: Client-side Image Preprocessing
            const processed = await preprocessor.process(sourceElement);

            // Check quality warnings
            if (!processed.quality.isAcceptable) {
                hideProcessing();
                const warningMsg = processed.quality.warnings.join(" ");
                const suggestionMsg = processed.quality.suggestions.join(" ");
                showGuidance(`Image quality issue: ${warningMsg} ${suggestionMsg}`);
                return;
            }

            // Step 2: Facial Analysis using MediaPipe FaceLandmarker
            showProcessing("2/3 Performing facial expression analysis...");
            const analysis = await emotionEngine.analyze(processed.canvas);

            if (!analysis.hasFace) {
                hideProcessing();
                showGuidance(analysis.message || "No human face was detected in the frame. Please look directly into the camera or choose a clearer portrait.");
                return;
            }

            // Step 3: Fetch Recommendations from ASP.NET Core Backend
            showProcessing("3/3 Curating emotion-matched music...");

            const requestPayload = {
                emotion: analysis.emotion,
                confidence: analysis.confidence,
                blendshapes: analysis.blendshapes,
                quality: processed.quality,
                sourceType: sourceType
            };

            const response = await fetch("/api/recommendations", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(requestPayload)
            });

            if (!response.ok) {
                throw new Error(`Server returned HTTP ${response.status}`);
            }

            const recommendationData = await response.json();
            hideProcessing();

            // Step 4: Render Results and Recommendations
            renderResults(recommendationData, analysis, processed.quality);

            // Scroll smoothly to results
            resultsContainer.scrollIntoView({ behavior: "smooth", block: "start" });

        } catch (error) {
            hideProcessing();
            console.error("Processing pipeline error:", error);
            showGuidance(`Analysis encountered an error: ${error.message}. Please retry.`);
        }
    }

    // -------------------------------------------------------------
    // Rendering Results & Recommendations
    // -------------------------------------------------------------
    function renderResults(data, analysis, quality) {
        resultsContainer.classList.remove("d-none");
        activeEmotion = data.detectedEmotion;
        currentRecommendations = data.tracks || [];

        // Configure audio playlist
        audioPlayer.setPlaylist(currentRecommendations, 0);

        // Primary Emotion & Confidence
        emotionBadge.textContent = data.detectedEmotion;
        emotionBadge.className = `badge emotion-pill pill-${data.detectedEmotion.toLowerCase()}`;

        confidenceBadge.textContent = `${Math.round(data.confidence * 100)}% Confidence (${data.confidenceLevel})`;
        confidenceBadge.className = `badge bg-dark border border-secondary text-light`;

        emotionDescription.textContent = data.emotionDescription;
        musicalRationale.textContent = data.musicalRationale;

        // Image Quality Status Pill
        qualityBadge.innerHTML = `<i class="bi bi-shield-check text-success me-1"></i> Quality: ${quality.brightness > 55 ? "Optimal" : "Dim Adjusted"} • ${quality.width}x${quality.height} • ${quality.blurVariance > 25 ? "Sharp Focus" : "Soft Focus"}`;

        // Uncertainty Banner
        if (data.isUncertain && data.guidanceMessage) {
            showGuidance(data.guidanceMessage);
        } else {
            hideGuidance();
        }

        // Facial Signals Summary (Genuine Anatomical Details)
        renderSignals(analysis.signals);

        // Emotion Breakdown Progress Bars
        renderBreakdown(analysis.breakdown, data.detectedEmotion);

        // Genre Filter Pills
        renderGenreFilters(data.recommendedGenres);

        // Render Music Tracks Grid
        renderTracksGrid(currentRecommendations);

        // Auto-load first recommended track into player dock without forced autoplay
        if (currentRecommendations.length > 0) {
            playerDock.classList.remove("d-none");
            updatePlayerDockTrack(currentRecommendations[0]);
        }
    }

    function renderSignals(signals) {
        if (!signals || Object.keys(signals).length === 0) {
            signalsSummary.innerHTML = `<span class="text-muted small">No detailed signal data.</span>`;
            return;
        }

        signalsSummary.innerHTML = `
            <div class="d-flex flex-wrap gap-2">
                <span class="signal-tag">Smile: <strong>${Math.round((signals.smile || 0) * 100)}%</strong></span>
                <span class="signal-tag">Frown: <strong>${Math.round((signals.frown || 0) * 100)}%</strong></span>
                <span class="signal-tag">Brow Furrow: <strong>${Math.round((signals.browFurrow || 0) * 100)}%</strong></span>
                <span class="signal-tag">Inner Brow: <strong>${Math.round((signals.browInnerUp || 0) * 100)}%</strong></span>
                <span class="signal-tag">Jaw Open: <strong>${Math.round((signals.jawOpen || 0) * 100)}%</strong></span>
                <span class="signal-tag">Eye Wide: <strong>${Math.round((signals.eyeWide || 0) * 100)}%</strong></span>
            </div>
        `;
    }

    function renderBreakdown(breakdown, dominantEmotion) {
        if (!breakdown || !breakdown.length) {
            breakdownContainer.innerHTML = "";
            return;
        }

        breakdownContainer.innerHTML = breakdown.map(item => {
            const isDominant = item.emotion.toLowerCase() === dominantEmotion.toLowerCase();
            const pct = Math.min(100, Math.round(item.score * 100));
            const barClass = isDominant ? `bg-${item.emotion.toLowerCase()}` : "bg-secondary";

            return `
                <div class="mb-2">
                    <div class="d-flex justify-content-between small text-light mb-1">
                        <span><strong>${item.emotion}</strong></span>
                        <span class="${isDominant ? "text-info fw-bold" : "text-muted"}">${pct}%</span>
                    </div>
                    <div class="progress" style="height: 6px; background: rgba(255,255,255,0.1);">
                        <div class="progress-bar ${barClass}" style="width: ${pct}%"></div>
                    </div>
                </div>
            `;
        }).join("");
    }

    function renderGenreFilters(genres) {
        if (!genres || !genres.length) {
            genrePillsContainer.innerHTML = "";
            return;
        }

        genrePillsContainer.innerHTML = `
            <button class="btn btn-sm btn-outline-info active genre-pill" data-genre="">All Recommended</button>
            ${genres.map(g => `<button class="btn btn-sm btn-outline-secondary genre-pill" data-genre="${g}">${g}</button>`).join("")}
        `;

        genrePillsContainer.querySelectorAll(".genre-pill").forEach(pill => {
            pill.addEventListener("click", () => {
                genrePillsContainer.querySelectorAll(".genre-pill").forEach(p => {
                    p.classList.remove("active", "btn-outline-info");
                    p.classList.add("btn-outline-secondary");
                });
                pill.classList.add("active", "btn-outline-info");
                pill.classList.remove("btn-outline-secondary");

                const selected = pill.getAttribute("data-genre");
                if (!selected) {
                    renderTracksGrid(currentRecommendations);
                } else {
                    const filtered = currentRecommendations.filter(t =>
                        t.genre.toLowerCase().includes(selected.toLowerCase()) ||
                        t.moodTags.some(m => m.toLowerCase() === selected.toLowerCase())
                    );
                    renderTracksGrid(filtered.length ? filtered : currentRecommendations);
                }
            });
        });
    }

    function renderTracksGrid(tracks) {
        if (!tracks || !tracks.length) {
            recommendationsGrid.innerHTML = `<div class="col-12 text-center text-muted py-4">No matching tracks found for this selection.</div>`;
            return;
        }

        recommendationsGrid.innerHTML = tracks.map(t => {
            const isPlayingThis = audioPlayer.currentTrack && audioPlayer.currentTrack.id === t.id && audioPlayer.isPlaying;
            return `
                <div class="col-md-4 col-sm-6 mb-3">
                    <div class="track-card ${isPlayingThis ? "is-playing" : ""}" data-track-id="${t.id}">
                        <div class="track-artwork" style="background: ${t.coverGradient}">
                            <div class="track-play-overlay">
                                <button class="btn btn-circle btn-play-track" data-track-id="${t.id}" title="Play/Pause">
                                    <i class="bi ${isPlayingThis ? "bi-pause-fill" : "bi-play-fill"}"></i>
                                </button>
                            </div>
                            <span class="track-bpm-badge">${t.bpm} BPM</span>
                        </div>
                        <div class="track-info">
                            <h6 class="track-title text-truncate" title="${t.title}">${t.title}</h6>
                            <p class="track-artist text-truncate text-muted small mb-1">${t.artist}</p>
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <span class="track-genre-badge">${t.genre}</span>
                                <span class="track-duration text-muted small">${audioPlayer.formatTime(t.durationSeconds)}</span>
                            </div>
                            <p class="track-description text-muted small">${t.description}</p>
                            <div class="track-tags">
                                ${t.moodTags.map(tag => `<span class="badge bg-dark-subtle text-light border border-secondary small me-1">${tag}</span>`).join("")}
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }).join("");

        // Wire up play buttons inside cards
        recommendationsGrid.querySelectorAll(".btn-play-track").forEach(btn => {
            btn.addEventListener("click", (e) => {
                e.stopPropagation();
                const trackId = btn.getAttribute("data-track-id");
                const track = currentRecommendations.find(t => t.id === trackId);
                if (track) {
                    audioPlayer.playTrack(track);
                }
            });
        });
    }

    // -------------------------------------------------------------
    // Audio Player UI Synchronization
    // -------------------------------------------------------------
    function updatePlayerDockTrack(track) {
        if (!track) return;
        playerTitle.textContent = track.title;
        playerArtist.textContent = track.artist;
        playerGenre.textContent = track.genre;
        playerCover.style.background = track.coverGradient;
        playerTotalTime.textContent = audioPlayer.formatTime(track.durationSeconds);
        playerDock.classList.remove("d-none");
    }

    audioPlayer.onTrackChange((track) => {
        updatePlayerDockTrack(track);
        syncAllPlayButtons();
    });

    audioPlayer.onPlaybackState((isPlaying, track) => {
        btnPlayerPlay.innerHTML = `<i class="bi ${isPlaying ? "bi-pause-fill" : "bi-play-fill"} fs-4"></i>`;
        if (isPlaying) {
            playerVisualizer.classList.add("playing");
        } else {
            playerVisualizer.classList.remove("playing");
        }
        syncAllPlayButtons();
    });

    audioPlayer.onTimeUpdate((current, duration, progress) => {
        playerCurrentTime.textContent = audioPlayer.formatTime(current);
        if (duration > 0) {
            playerTotalTime.textContent = audioPlayer.formatTime(duration);
        }
        playerProgress.style.width = `${progress}%`;
    });

    function syncAllPlayButtons() {
        const activeId = audioPlayer.currentTrack ? audioPlayer.currentTrack.id : null;
        const isPlaying = audioPlayer.isPlaying;

        document.querySelectorAll(".track-card").forEach(card => {
            const cardId = card.getAttribute("data-track-id");
            const btn = card.querySelector(".btn-play-track");
            if (cardId === activeId && isPlaying) {
                card.classList.add("is-playing");
                if (btn) btn.innerHTML = `<i class="bi bi-pause-fill"></i>`;
            } else {
                card.classList.remove("is-playing");
                if (btn) btn.innerHTML = `<i class="bi bi-play-fill"></i>`;
            }
        });
    }

    // Global delegated listener for play buttons across recommendations & initial catalog
    document.addEventListener("click", async (e) => {
        const btn = e.target.closest(".btn-play-track");
        if (!btn) return;
        e.stopPropagation();
        const trackId = btn.getAttribute("data-track-id");
        if (!trackId) return;

        let track = currentRecommendations.find(t => t.id === trackId);
        if (!track) {
            try {
                const resp = await fetch(`/api/tracks/${trackId}`);
                if (resp.ok) {
                    track = await resp.json();
                }
            } catch (err) {
                console.error("Failed to load track:", err);
            }
        }

        if (track) {
            audioPlayer.playTrack(track);
        }
    });

    // Player dock controls
    btnPlayerPlay.addEventListener("click", () => audioPlayer.togglePlayPause());
    btnPlayerPrev.addEventListener("click", () => audioPlayer.previous());
    btnPlayerNext.addEventListener("click", () => audioPlayer.next());

    playerProgressContainer.addEventListener("click", (e) => {
        const rect = playerProgressContainer.getBoundingClientRect();
        const fraction = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
        audioPlayer.seek(fraction);
    });

    playerVolume.addEventListener("input", (e) => {
        audioPlayer.setVolume(parseFloat(e.target.value));
        updateMuteButtonIcon(audioPlayer.isMuted);
    });

    btnPlayerMute.addEventListener("click", () => {
        const muted = audioPlayer.toggleMute();
        updateMuteButtonIcon(muted);
        if (!muted) {
            playerVolume.value = audioPlayer.audio.volume;
        }
    });

    function updateMuteButtonIcon(isMuted) {
        btnPlayerMute.innerHTML = `<i class="bi ${isMuted ? "bi-volume-mute-fill" : "bi-volume-up-fill"}"></i>`;
    }

    // -------------------------------------------------------------
    // Helper Notification Routines
    // -------------------------------------------------------------
    function showProcessing(msg) {
        processingMessage.textContent = msg;
        processingOverlay.classList.remove("d-none");
    }

    function hideProcessing() {
        processingOverlay.classList.add("d-none");
    }

    function showGuidance(msg) {
        guidanceText.textContent = msg;
        guidanceAlert.classList.remove("d-none");
    }

    function hideGuidance() {
        guidanceAlert.classList.add("d-none");
    }
});
