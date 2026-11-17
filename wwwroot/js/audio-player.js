/**
 * AudioPlayer
 * Centralized audio player managing HTML5 Audio playback.
 * Ensures strictly ONLY ONE song plays at a time.
 * Syncs track cards with sticky bottom player dock.
 */
export class AudioPlayer {
    constructor() {
        this.audio = new Audio();
        this.audio.preload = "auto";
        this.currentTrack = null;
        this.playlist = [];
        this.currentIndex = -1;
        this.isPlaying = false;
        this.isMuted = false;
        this.lastVolume = 0.85;

        this.onTrackChangeCallbacks = [];
        this.onPlaybackStateCallbacks = [];
        this.onTimeUpdateCallbacks = [];

        this.initAudioListeners();
    }

    initAudioListeners() {
        this.audio.volume = this.lastVolume;

        this.audio.addEventListener("play", () => {
            this.isPlaying = true;
            this.notifyPlaybackState(true);
        });

        this.audio.addEventListener("pause", () => {
            this.isPlaying = false;
            this.notifyPlaybackState(false);
        });

        this.audio.addEventListener("ended", () => {
            this.isPlaying = false;
            this.notifyPlaybackState(false);
            // Auto-advance to next track in playlist
            this.next();
        });

        this.audio.addEventListener("timeupdate", () => {
            const current = this.audio.currentTime || 0;
            const duration = this.audio.duration || (this.currentTrack ? this.currentTrack.durationSeconds : 0);
            const progress = duration > 0 ? (current / duration) * 100 : 0;

            this.notifyTimeUpdate(current, duration, progress);
        });

        this.audio.addEventListener("error", (e) => {
            console.error("Audio playback error:", e);
            this.isPlaying = false;
            this.notifyPlaybackState(false);
            this.showPlaybackError();
        });
    }

    setPlaylist(tracks, startIndex = 0) {
        this.playlist = tracks || [];
        if (this.playlist.length > 0 && startIndex >= 0 && startIndex < this.playlist.length) {
            this.currentIndex = startIndex;
        }
    }

    playTrack(track) {
        if (!track || !track.audioUrl) return;

        // If clicking the currently playing track, toggle play/pause
        if (this.currentTrack && this.currentTrack.id === track.id) {
            if (this.isPlaying) {
                this.pause();
            } else {
                this.resume();
            }
            return;
        }

        // Switch to new track
        this.currentTrack = track;
        this.currentIndex = this.playlist.findIndex(t => t.id === track.id);
        if (this.currentIndex === -1 && track) {
            this.playlist = [track];
            this.currentIndex = 0;
        }

        this.audio.src = track.audioUrl;
        this.audio.load();

        const playPromise = this.audio.play();
        if (playPromise !== undefined) {
            playPromise.then(() => {
                this.isPlaying = true;
                this.notifyTrackChange(this.currentTrack);
                this.notifyPlaybackState(true);
            }).catch(error => {
                console.warn("Autoplay was prevented by browser:", error);
                this.isPlaying = false;
                this.notifyTrackChange(this.currentTrack);
                this.notifyPlaybackState(false);
            });
        }
    }

    togglePlayPause() {
        if (!this.currentTrack) {
            if (this.playlist.length > 0) {
                this.playTrack(this.playlist[0]);
            }
            return;
        }

        if (this.isPlaying) {
            this.pause();
        } else {
            this.resume();
        }
    }

    pause() {
        this.audio.pause();
        this.isPlaying = false;
        this.notifyPlaybackState(false);
    }

    resume() {
        if (!this.audio.src && this.currentTrack) {
            this.audio.src = this.currentTrack.audioUrl;
        }

        const p = this.audio.play();
        if (p !== undefined) {
            p.then(() => {
                this.isPlaying = true;
                this.notifyPlaybackState(true);
            }).catch(err => {
                console.warn("Playback resume failed:", err);
            });
        }
    }

    next() {
        if (!this.playlist.length) return;
        let nextIndex = this.currentIndex + 1;
        if (nextIndex >= this.playlist.length) {
            nextIndex = 0; // Loop playlist
        }
        this.playTrack(this.playlist[nextIndex]);
    }

    previous() {
        if (!this.playlist.length) return;
        let prevIndex = this.currentIndex - 1;
        if (prevIndex < 0) {
            prevIndex = this.playlist.length - 1;
        }
        this.playTrack(this.playlist[prevIndex]);
    }

    seek(fraction) {
        if (!this.audio.duration && !this.currentTrack) return;
        const duration = this.audio.duration || this.currentTrack.durationSeconds;
        this.audio.currentTime = Math.max(0, Math.min(duration, fraction * duration));
    }

    setVolume(value) {
        const vol = Math.max(0, Math.min(1, value));
        this.audio.volume = vol;
        if (vol > 0) {
            this.lastVolume = vol;
            this.isMuted = false;
        } else {
            this.isMuted = true;
        }
    }

    toggleMute() {
        if (this.isMuted) {
            this.audio.volume = this.lastVolume > 0 ? this.lastVolume : 0.7;
            this.isMuted = false;
        } else {
            this.lastVolume = this.audio.volume;
            this.audio.volume = 0;
            this.isMuted = true;
        }
        return this.isMuted;
    }

    showPlaybackError() {
        const errorAlert = document.getElementById("audio-error-toast");
        if (errorAlert) {
            errorAlert.classList.remove("d-none");
            setTimeout(() => {
                errorAlert.classList.add("d-none");
            }, 5000);
        }
    }

    // Event Subscriptions
    onTrackChange(cb) { this.onTrackChangeCallbacks.push(cb); }
    onPlaybackState(cb) { this.onPlaybackStateCallbacks.push(cb); }
    onTimeUpdate(cb) { this.onTimeUpdateCallbacks.push(cb); }

    notifyTrackChange(track) {
        this.onTrackChangeCallbacks.forEach(cb => cb(track));
    }

    notifyPlaybackState(isPlaying) {
        this.onPlaybackStateCallbacks.forEach(cb => cb(isPlaying, this.currentTrack));
    }

    notifyTimeUpdate(current, duration, progress) {
        this.onTimeUpdateCallbacks.forEach(cb => cb(current, duration, progress));
    }

    formatTime(seconds) {
        if (isNaN(seconds) || seconds < 0) return "0:00";
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs < 10 ? "0" : ""}${secs}`;
    }
}
