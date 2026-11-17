using EmotionMusicAnalyzer.Models;

namespace EmotionMusicAnalyzer.Services;

public class ImageQualityService : IImageQualityService
{
    public ImageAssessmentResponse AssessImage(string base64Data)
    {
        var response = new ImageAssessmentResponse
        {
            IsValid = true,
            Status = "OK"
        };

        if (string.IsNullOrWhiteSpace(base64Data))
        {
            response.IsValid = false;
            response.Status = "EmptyData";
            response.Warnings.Add("No image data received.");
            response.Suggestions.Add("Please select or capture a valid JPG, JPEG, or PNG photograph.");
            return response;
        }

        try
        {
            // Strip data:image/...;base64, prefix if present
            string raw = base64Data;
            int commaIndex = raw.IndexOf(',');
            if (commaIndex >= 0)
            {
                raw = raw.Substring(commaIndex + 1);
            }

            byte[] bytes = Convert.FromBase64String(raw);

            if (bytes.Length < 16)
            {
                response.IsValid = false;
                response.Status = "TooSmallBytes";
                response.Warnings.Add("The uploaded file is corrupt or empty.");
                response.Suggestions.Add("Please select a standard photo in JPG, JPEG, or PNG format.");
                return response;
            }

            // Extract dimensions from headers if standard PNG or JPEG
            int width = 0;
            int height = 0;

            if (bytes.Length >= 24 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                // PNG
                width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
                height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
            }
            else if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xD8)
            {
                // JPEG: parse markers
                int i = 2;
                while (i + 8 < bytes.Length)
                {
                    if (bytes[i] == 0xFF)
                    {
                        byte marker = bytes[i + 1];
                        if (marker == 0xC0 || marker == 0xC1 || marker == 0xC2) // SOF0, SOF1, SOF2
                        {
                            height = (bytes[i + 5] << 8) | bytes[i + 6];
                            width = (bytes[i + 7] << 8) | bytes[i + 8];
                            break;
                        }
                        int length = (bytes[i + 2] << 8) | bytes[i + 3];
                        i += 2 + length;
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            response.Width = width;
            response.Height = height;

            if (width > 0 && height > 0)
            {
                if (width < 120 || height < 120)
                {
                    response.IsValid = false;
                    response.Warnings.Add($"Image resolution ({width}x{height}) is too small for accurate landmark analysis.");
                    response.Suggestions.Add("Use an image of at least 320x320 pixels for reliable facial recognition.");
                }
                else if (width > 4096 || height > 4096)
                {
                    response.Warnings.Add($"Image resolution ({width}x{height}) is very large; it will be pre-scaled for performance.");
                    response.Suggestions.Add("Image scaling is automatically applied to optimize processing time.");
                }
            }
        }
        catch (Exception ex)
        {
            response.IsValid = false;
            response.Status = "ParseError";
            response.Warnings.Add($"Failed to parse image data: {ex.Message}");
            response.Suggestions.Add("Please upload an uncorrupted JPG, JPEG, or PNG image.");
        }

        return response;
    }

    public ImageQualityMetrics AssessMetrics(int width, int height, double brightness, double contrast, double blurVariance)
    {
        var metrics = new ImageQualityMetrics
        {
            Width = width,
            Height = height,
            Brightness = Math.Round(brightness, 1),
            Contrast = Math.Round(contrast, 1),
            BlurVariance = Math.Round(blurVariance, 1),
            IsAcceptable = true
        };

        // Check dimensions
        if (width < 120 || height < 120)
        {
            metrics.IsAcceptable = false;
            metrics.Warnings.Add("Image dimensions are too low for reliable feature detection.");
            metrics.Suggestions.Add("Capture a closer photo or upload an image with at least 320x320 resolution.");
        }

        // Check brightness (0 - 255)
        if (brightness < 38.0)
        {
            metrics.IsAcceptable = false;
            metrics.Warnings.Add("Image is underexposed / too dark.");
            metrics.Suggestions.Add("Increase room lighting or face towards a light source to reveal facial contours clearly.");
        }
        else if (brightness > 225.0)
        {
            metrics.IsAcceptable = false;
            metrics.Warnings.Add("Image is overexposed / washed out.");
            metrics.Suggestions.Add("Reduce harsh glare or direct backlighting behind your head.");
        }
        else if (brightness < 55.0)
        {
            metrics.Warnings.Add("Lighting is dim. Facial feature accuracy may be reduced.");
            metrics.Suggestions.Add("Mild software exposure compensation has been applied, but brighter lighting is recommended.");
        }

        // Check contrast
        if (contrast < 22.0)
        {
            metrics.Warnings.Add("Low image contrast detected.");
            metrics.Suggestions.Add("Ensure uniform lighting without strong haze or fog on the camera lens.");
        }

        // Check blur variance (Laplacian gradient variance on client)
        if (blurVariance > 0 && blurVariance < 25.0)
        {
            metrics.IsAcceptable = false;
            metrics.Warnings.Add("Image appears blurry or out of focus.");
            metrics.Suggestions.Add("Hold the camera steady or wipe the camera lens before capturing.");
        }

        return metrics;
    }
}
