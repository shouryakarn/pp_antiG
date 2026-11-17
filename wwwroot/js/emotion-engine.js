/**
 * EmotionEngine
 * Loads MediaPipe FaceLandmarker (with local-first and CDN fallback)
 * Evaluates 52 blendshape coefficients & 3D geometric ratios to classify genuine emotions:
 * Happy, Sad, Angry, Surprised, Neutral, and Fearful.
 * Note: Landmark dots and meshes are NEVER drawn or displayed on the user interface.
 */

import { FaceLandmarker, FilesetResolver } from "/lib/tasks-vision/vision_bundle.mjs";

export class EmotionEngine {
    constructor() {
        this.landmarker = null;
        this.isReady = false;
        this.isLoading = false;
        this.loadError = null;
    }

    /**
     * Initializes the MediaPipe Face Landmarker model
     */
    async initialize() {
        if (this.isReady) return;
        if (this.isLoading) return;
        this.isLoading = true;

        try {
            // First try local WASM files, fallback to CDN if necessary
            let visionResolver;
            try {
                visionResolver = await FilesetResolver.forVisionTasks("/lib/tasks-vision/wasm");
            } catch (wasmErr) {
                console.warn("Local WASM resolver failed, falling back to CDN:", wasmErr);
                visionResolver = await FilesetResolver.forVisionTasks("https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.14/wasm");
            }

            // First try local model, fallback to CDN if necessary
            let modelPath = "/models/face_landmarker.task";
            try {
                this.landmarker = await FaceLandmarker.createFromOptions(visionResolver, {
                    baseOptions: {
                        modelAssetPath: modelPath,
                        delegate: "GPU"
                    },
                    outputFaceBlendshapes: true,
                    outputFacialTransformationMatrixes: false,
                    runningMode: "IMAGE",
                    numFaces: 1
                });
            } catch (modelErr) {
                console.warn("GPU delegate or local model failed, retrying CPU delegate / CDN:", modelErr);
                try {
                    this.landmarker = await FaceLandmarker.createFromOptions(visionResolver, {
                        baseOptions: {
                            modelAssetPath: modelPath,
                            delegate: "CPU"
                        },
                        outputFaceBlendshapes: true,
                        runningMode: "IMAGE",
                        numFaces: 1
                    });
                } catch (cdnErr) {
                    // Fallback to Google storage CDN if local file had any issue
                    this.landmarker = await FaceLandmarker.createFromOptions(visionResolver, {
                        baseOptions: {
                            modelAssetPath: "https://storage.googleapis.com/mediapipe-models/face_landmarker/face_landmarker/float16/1/face_landmarker.task",
                            delegate: "CPU"
                        },
                        outputFaceBlendshapes: true,
                        runningMode: "IMAGE",
                        numFaces: 1
                    });
                }
            }

            this.isReady = true;
            this.isLoading = false;
            console.log("MediaPipe FaceLandmarker successfully initialized.");
        } catch (err) {
            this.isLoading = false;
            this.loadError = err.message || "Failed to initialize MediaPipe Face Landmarker.";
            console.error("EmotionEngine initialization error:", err);
            throw new Error(`MediaPipe model initialization failed: ${this.loadError}`);
        }
    }

    /**
     * Analyzes facial expression from a preprocessed canvas or image
     * Returns: { hasFace, emotion, confidence, confidenceLevel, isUncertain, breakdown, signals, blendshapes }
     */
    async analyze(canvasOrImage) {
        if (!this.isReady) {
            await this.initialize();
        }

        const results = this.landmarker.detect(canvasOrImage);

        if (!results || !results.faceLandmarks || results.faceLandmarks.length === 0) {
            return {
                hasFace: false,
                emotion: "Unknown",
                confidence: 0,
                confidenceLevel: "Uncertain",
                isUncertain: true,
                message: "No human face was detected. Please ensure your face is clearly visible, facing forward, and unobstructed by hands or accessories.",
                breakdown: [],
                signals: {},
                blendshapes: {}
            };
        }

        const blendshapesData = results.faceBlendshapes && results.faceBlendshapes.length > 0
            ? results.faceBlendshapes[0].categories
            : [];

        const landmarksData = results.faceLandmarks[0];

        // Map blendshapes into a key-value dictionary
        const blendshapesMap = {};
        for (const item of blendshapesData) {
            blendshapesMap[item.categoryName] = item.score;
        }

        // Interpret genuine emotion using blendshapes + landmark geometry
        const interpretation = this.interpretFacialExpression(blendshapesMap, landmarksData);

        return {
            hasFace: true,
            ...interpretation,
            blendshapes: blendshapesMap
        };
    }

    /**
     * Genuine expression interpretation algorithm mapping facial action units
     */
    interpretFacialExpression(b, landmarks) {
        const val = (name) => b[name] || 0.0;

        // Composite signals
        const smile = (val("mouthSmileLeft") + val("mouthSmileRight")) / 2;
        const frown = (val("mouthFrownLeft") + val("mouthFrownRight")) / 2;
        const browFurrow = (val("browDownLeft") + val("browDownRight")) / 2;
        const browInnerUp = val("browInnerUp");
        const browOuterUp = (val("browOuterUpLeft") + val("browOuterUpRight")) / 2;
        const jawOpen = val("jawOpen");
        const eyeWide = (val("eyeWideLeft") + val("eyeWideRight")) / 2;
        const mouthStretch = (val("mouthStretchLeft") + val("mouthStretchRight")) / 2;
        const mouthClose = val("mouthClose");
        const noseSneer = (val("noseSneerLeft") + val("noseSneerRight")) / 2;
        const eyeSquint = (val("eyeSquintLeft") + val("eyeSquintRight")) / 2;

        // 3D Landmark Geometric Check: Lip Corner Elevation
        let geoSmile = 0;
        let mar = 0; // Mouth aspect ratio
        if (landmarks && landmarks.length >= 300) {
            const leftCorner = landmarks[61];
            const rightCorner = landmarks[291];
            const upperLip = landmarks[13];
            const lowerLip = landmarks[14];

            const mouthWidth = Math.hypot(rightCorner.x - leftCorner.x, rightCorner.y - leftCorner.y);
            const mouthHeight = Math.hypot(lowerLip.x - upperLip.x, lowerLip.y - upperLip.y);
            mar = mouthWidth > 0 ? (mouthHeight / mouthWidth) : 0;

            const avgCornerY = (leftCorner.y + rightCorner.y) / 2;
            const centerLipY = (upperLip.y + lowerLip.y) / 2;
            // In normalized screen coordinates, Y points down. Corners higher than center means avgCornerY < centerLipY
            const cornerLift = (centerLipY - avgCornerY) / (mouthWidth || 1);
            if (cornerLift > 0.02) {
                geoSmile = Math.min(1.0, cornerLift * 12);
            }
        }

        // 1. HAPPY: High smile, cheek squint, low frown, low brow furrow
        let happyScore = (smile * 0.70 + geoSmile * 0.30 + eyeSquint * 0.15) * 1.45 - (frown * 0.6) - (browFurrow * 0.4);
        happyScore = Math.max(0, happyScore);

        // 2. SURPRISED: High jawOpen, high eyeWide, high browInnerUp + outerUp, low furrow
        let surprisedScore = (jawOpen * 0.45 + eyeWide * 0.35 + (browInnerUp + browOuterUp) * 0.20) * 1.35 - (browFurrow * 0.4);
        surprisedScore = Math.max(0, surprisedScore);

        // 3. ANGRY: High browFurrow, nose sneer, mouth close/tension, low smile, low jaw open
        let angryScore = (browFurrow * 0.65 + noseSneer * 0.20 + mouthClose * 0.15) * 1.40 - (smile * 0.8) - (jawOpen * 0.4);
        angryScore = Math.max(0, angryScore);

        // 4. SAD: High frown, inner brow raised with outer brow flat, mouth downturned, low smile
        let sadBrow = Math.max(0, browInnerUp - (browOuterUp * 0.5));
        let sadScore = (frown * 0.55 + sadBrow * 0.45) * 1.35 - (smile * 0.8) - (jawOpen * 0.5);
        sadScore = Math.max(0, sadScore);

        // 5. FEARFUL: High eyeWide, raised browInnerUp + outer, lateral mouth stretch, tense mouth without relaxed smile
        let fearfulScore = (eyeWide * 0.40 + browInnerUp * 0.30 + mouthStretch * 0.30) * 1.30 - (smile * 0.7);
        fearfulScore = Math.max(0, fearfulScore);

        // 6. NEUTRAL: When all active expressive metrics are low / close to baseline
        const maxExpressive = Math.max(happyScore, surprisedScore, angryScore, sadScore, fearfulScore);
        let neutralScore = 0;
        if (maxExpressive < 0.32) {
            neutralScore = Math.min(1.0, 1.0 - (maxExpressive * 2.0));
        } else {
            neutralScore = Math.max(0, 0.40 - (maxExpressive * 0.5));
        }

        const candidates = [
            { emotion: "Happy", score: Number(happyScore.toFixed(3)) },
            { emotion: "Surprised", score: Number(surprisedScore.toFixed(3)) },
            { emotion: "Angry", score: Number(angryScore.toFixed(3)) },
            { emotion: "Sad", score: Number(sadScore.toFixed(3)) },
            { emotion: "Fearful", score: Number(fearfulScore.toFixed(3)) },
            { emotion: "Neutral", score: Number(neutralScore.toFixed(3)) }
        ];

        candidates.sort((a, b) => b.score - a.score);
        const top = candidates[0];
        const runnerUp = candidates[1];

        // Genuine confidence calculation
        let confidence = 0.5;
        if (top.emotion === "Neutral") {
            confidence = Math.min(0.92, Math.max(0.40, 1.0 - (maxExpressive * 1.8)));
        } else {
            const margin = top.score - runnerUp.score;
            confidence = Math.min(0.96, Math.max(0.35, (top.score * 0.65) + (margin * 0.35)));
        }

        confidence = Number(confidence.toFixed(2));

        const isUncertain = (top.score < 0.22 && top.emotion !== "Neutral") || (confidence < 0.40);

        let confidenceLevel = "Uncertain";
        if (confidence >= 0.75) confidenceLevel = "High";
        else if (confidence >= 0.55) confidenceLevel = "Moderate";
        else if (confidence >= 0.40) confidenceLevel = "Subtle";

        return {
            emotion: top.emotion,
            confidence,
            confidenceLevel,
            isUncertain,
            topScore: top.score,
            margin: Number((top.score - runnerUp.score).toFixed(3)),
            breakdown: candidates,
            signals: {
                smile: Number(smile.toFixed(2)),
                frown: Number(frown.toFixed(2)),
                browFurrow: Number(browFurrow.toFixed(2)),
                browInnerUp: Number(browInnerUp.toFixed(2)),
                jawOpen: Number(jawOpen.toFixed(2)),
                eyeWide: Number(eyeWide.toFixed(2)),
                mouthStretch: Number(mouthStretch.toFixed(2)),
                mouthAspectRatio: Number(mar.toFixed(2))
            }
        };
    }
}
