/**
 * ImagePreprocessor
 * Handles client-side image validation, quality metrics (brightness, contrast, blur),
 * orientation normalization, proportional resizing, and mild exposure compensation.
 */
export class ImagePreprocessor {
    constructor() {
        this.maxDimension = 1280; // Optimal resolution for MediaPipe Face Landmarker
        this.minDimension = 120;
    }

    /**
     * Preprocesses an image source (HTMLImageElement, HTMLVideoElement, or File/Blob)
     * Returns: { canvas, width, height, quality: { isAcceptable, brightness, contrast, blurVariance, warnings, suggestions } }
     */
    async process(source) {
        let imageElement;

        if (source instanceof HTMLVideoElement) {
            imageElement = source;
        } else if (source instanceof HTMLImageElement) {
            imageElement = source;
        } else if (source instanceof File || source instanceof Blob) {
            imageElement = await this.loadImageFromFile(source);
        } else {
            throw new Error("Unsupported image source provided to ImagePreprocessor.");
        }

        const naturalWidth = imageElement.videoWidth || imageElement.naturalWidth || imageElement.width;
        const naturalHeight = imageElement.videoHeight || imageElement.naturalHeight || imageElement.height;

        if (!naturalWidth || !naturalHeight) {
            throw new Error("Unable to determine image dimensions.");
        }

        // Calculate proportional scale if oversized
        let targetWidth = naturalWidth;
        let targetHeight = naturalHeight;

        if (Math.max(naturalWidth, naturalHeight) > this.maxDimension) {
            const scale = this.maxDimension / Math.max(naturalWidth, naturalHeight);
            targetWidth = Math.round(naturalWidth * scale);
            targetHeight = Math.round(naturalHeight * scale);
        }

        // Render to canvas
        const canvas = document.createElement("canvas");
        canvas.width = targetWidth;
        canvas.height = targetHeight;
        const ctx = canvas.getContext("2d", { willReadFrequently: true });

        ctx.drawImage(imageElement, 0, 0, targetWidth, targetHeight);

        // Analyze image quality metrics: brightness, contrast, and blur variance
        const quality = this.analyzeQuality(ctx, targetWidth, targetHeight);

        // Apply mild brightness/contrast compensation if image is somewhat dark or low-contrast
        if (quality.brightness < 60 && quality.brightness > 30) {
            this.applyMildExposureCompensation(ctx, targetWidth, targetHeight, 1.25);
            quality.adjusted = true;
        }

        return {
            canvas,
            width: targetWidth,
            height: targetHeight,
            originalWidth: naturalWidth,
            originalHeight: naturalHeight,
            quality
        };
    }

    /**
     * Loads a File or Blob into an HTMLImageElement
     */
    loadImageFromFile(file) {
        return new Promise((resolve, reject) => {
            const url = URL.createObjectURL(file);
            const img = new Image();
            img.crossOrigin = "anonymous";
            img.onload = () => {
                URL.revokeObjectURL(url);
                resolve(img);
            };
            img.onerror = (err) => {
                URL.revokeObjectURL(url);
                reject(new Error("Failed to load image file. Please ensure it is a valid JPG, JPEG, or PNG photograph."));
            };
            img.src = url;
        });
    }

    /**
     * Computes brightness, contrast, and Laplacian blur variance
     */
    analyzeQuality(ctx, width, height) {
        const warnings = [];
        const suggestions = [];
        let isAcceptable = true;

        if (width < this.minDimension || height < this.minDimension) {
            isAcceptable = false;
            warnings.push(`Image dimensions (${width}x${height}) are too low.`);
            suggestions.push("Please use an image of at least 320x320 pixels for reliable facial detection.");
        }

        // Sample pixels for speed: sample down to a maximum 160x160 grid
        const sampleW = Math.min(width, 160);
        const sampleH = Math.min(height, 160);

        const sampleCanvas = document.createElement("canvas");
        sampleCanvas.width = sampleW;
        sampleCanvas.height = sampleH;
        const sampleCtx = sampleCanvas.getContext("2d", { willReadFrequently: true });
        sampleCtx.drawImage(ctx.canvas, 0, 0, sampleW, sampleH);

        const imgData = sampleCtx.getImageData(0, 0, sampleW, sampleH);
        const data = imgData.data;
        const totalPixels = sampleW * sampleH;

        let sumY = 0;
        let sumY2 = 0;
        const gray = new Float32Array(totalPixels);

        // Compute grayscale luminances: Y = 0.299R + 0.587G + 0.114B
        for (let i = 0, p = 0; i < data.length; i += 4, p++) {
            const y = 0.299 * data[i] + 0.587 * data[i + 1] + 0.114 * data[i + 2];
            gray[p] = y;
            sumY += y;
            sumY2 += y * y;
        }

        const avgBrightness = sumY / totalPixels;
        const variance = Math.max(0, (sumY2 / totalPixels) - (avgBrightness * avgBrightness));
        const contrast = Math.sqrt(variance);

        // Laplacian blur estimation
        // Kernel: [0, 1, 0; 1, -4, 1; 0, 1, 0]
        let lapSum = 0;
        let lapSum2 = 0;
        let lapCount = 0;

        for (let y = 1; y < sampleH - 1; y++) {
            for (let x = 1; x < sampleW - 1; x++) {
                const idx = y * sampleW + x;
                const lap = gray[idx - sampleW] + gray[idx + sampleW] + gray[idx - 1] + gray[idx + 1] - (4 * gray[idx]);
                lapSum += lap;
                lapSum2 += lap * lap;
                lapCount++;
            }
        }

        const lapMean = lapSum / lapCount;
        const blurVariance = (lapSum2 / lapCount) - (lapMean * lapMean);

        // Evaluate brightness
        if (avgBrightness < 35) {
            isAcceptable = false;
            warnings.push("Image is severely underexposed / dark.");
            suggestions.push("Increase room lighting or move closer to a warm front-facing light.");
        } else if (avgBrightness > 225) {
            isAcceptable = false;
            warnings.push("Image is overexposed / washed out.");
            suggestions.push("Avoid direct bright light or strong glare directly behind your head.");
        } else if (avgBrightness < 55) {
            warnings.push("Lighting is somewhat dim.");
            suggestions.push("Subtle software exposure boost applied, but brighter lighting will improve accuracy.");
        }

        // Evaluate contrast
        if (contrast < 20) {
            warnings.push("Low contrast detected.");
            suggestions.push("Check that the camera lens is clean and avoid hazy backlighting.");
        }

        // Evaluate blur
        if (blurVariance < 18.0) {
            isAcceptable = false;
            warnings.push("Image appears blurry or out of focus.");
            suggestions.push("Hold the camera still and ensure your face is sharply focused.");
        } else if (blurVariance < 32.0) {
            warnings.push("Minor motion blur or soft focus detected.");
            suggestions.push("Keeping completely steady during capture will yield the most confident prediction.");
        }

        return {
            width,
            height,
            brightness: Math.round(avgBrightness),
            contrast: Math.round(contrast),
            blurVariance: Math.round(blurVariance),
            isAcceptable,
            warnings,
            suggestions
        };
    }

    /**
     * Applies mild gamma/brightness curve directly on canvas
     */
    applyMildExposureCompensation(ctx, width, height, factor = 1.25) {
        const imgData = ctx.getImageData(0, 0, width, height);
        const d = imgData.data;
        for (let i = 0; i < d.length; i += 4) {
            d[i] = Math.min(255, d[i] * factor);
            d[i + 1] = Math.min(255, d[i + 1] * factor);
            d[i + 2] = Math.min(255, d[i + 2] * factor);
        }
        ctx.putImageData(imgData, 0, 0);
    }
}
