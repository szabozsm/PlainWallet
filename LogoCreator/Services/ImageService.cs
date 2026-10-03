using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using Svg.Skia;

namespace LogoCreator.Services
{
    public static class ImageService
    {
        private const int MaxStoredSize = 256;

        public static byte[] ResizeImage(byte[] imageBytes, int maxWidth = MaxStoredSize, int maxHeight = MaxStoredSize)
        {
            try
            {
                if (imageBytes == null || imageBytes.Length == 0)
                    return imageBytes ?? Array.Empty<byte>();

                // 1. Identify the image format metadata without decoding the entire pixel buffer
                IImageFormat detectedFormat = Image.Identify(imageBytes).Metadata.DecodedImageFormat;

                // Fallback to JPEG if format can't be identified natively
                detectedFormat ??= SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance;

                // 2. Safely load the image from the raw byte array
                using Image image = Image.Load(imageBytes);

                // 3. Define aspect-ratio preserving dimensions
                var resizeOptions = new ResizeOptions
                {
                    Size = new Size(maxWidth, maxHeight),
                    Mode = ResizeMode.Max // Scales image down until it hits bounds without stretching
                };

                // 4. Process the resize mutation
                image.Mutate(x => x.Resize(resizeOptions));

                // 5. Stream the resulting image back into a byte array using its original format
                using var outputStream = new MemoryStream();
                image.SaveAsync(outputStream, detectedFormat);

                return outputStream.ToArray();
            }
            catch
            {
                // If resizing fails for any reason, safely return the original uncompressed bytes
                return imageBytes;
            }

        }

        public static byte[] RasterizeAndResizeSvg(byte[] svgBytes, int maxWidth = MaxStoredSize, int maxHeight = MaxStoredSize)
        {
            try
            {
                using var stream = new MemoryStream(svgBytes);

                // Use SKSvg from the Svg.Skia namespace
                using var svg = new SKSvg();
                if (svg.Load(stream) == null)
                    return svgBytes; // Return original if SVG loading fails completely

                // Safely extract width and height from the picture bounds
                float svgWidth = svg.Picture.CullRect.Width;
                float svgHeight = svg.Picture.CullRect.Height;

                if (svgWidth <= 0 || svgHeight <= 0)
                    return svgBytes;

                // Calculate dimensions maintaining aspect ratio
                float scaleX = maxWidth / svgWidth;
                float scaleY = maxHeight / svgHeight;
                float scale = Math.Min(scaleX, scaleY);

                int newWidth = (int)(svgWidth * scale);
                int newHeight = (int)(svgHeight * scale);

                // Render the vector instructions cleanly onto a pixel-based surface
                using var bitmap = new SKBitmap(newWidth, newHeight);
                using var canvas = new SKCanvas(bitmap);
                canvas.Clear(SKColors.Transparent);

                // Apply scaling matrix for high-quality resizing without distortion
                var matrix = SKMatrix.CreateScale(scale, scale);
                canvas.DrawPicture(svg.Picture, ref matrix);

                // Encode and output as a flat PNG byte array
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                return data.ToArray();
            }
            catch
            {
                // Fail-safe: Return original bytes if rasterization fails
                return svgBytes;
            }
        }

  public static string GetRasterBackgroundColor(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return "#FFFFFF"; // Safety fallback

        try
        {
            // Load the image pixel data into memory
            using Image<Rgba32> image = Image.Load<Rgba32>(imageBytes);
            
            int width = image.Width;
            int height = image.Height;

            // List to aggregate every pixel along the outmost structural perimeter
            var edgePixels = new List<Rgba32>();

            // 1. Extract Top and Bottom horizontal borders
            for (int x = 0; x < width; x++)
            {
                edgePixels.Add(image[x, 0]);              // Top row
                edgePixels.Add(image[x, height - 1]);     // Bottom row
            }

            // 2. Extract Left and Right vertical borders
            for (int y = 0; y < height; y++)
            {
                edgePixels.Add(image[0, y]);              // Left column
                edgePixels.Add(image[width - 1, y]);      // Right column
            }

            // STRATEGY 1: Check for Transparency first
            // If more than 30% of the border is transparent, consider the background transparent
            int transparentCount = edgePixels.Count(p => p.A == 0);
            if (transparentCount > (edgePixels.Count * 0.3))
            {
                return "#FFFFFF"; // Fallback for transparent images (change to your preferred color)
            }

            // STRATEGY 2: Fuzzy Grouping to counter JPEG compression noise.
            // We group by a simplified 24-bit key string to merge near-identical pixel values.
            var dominantColor = edgePixels
                .Where(p => p.A > 0) // Filter out any stray transparent pixels
                .GroupBy(p => $"{p.R},{p.G},{p.B}") 
                .OrderByDescending(group => group.Count())
                .First()
                .First(); // Pull the first actual pixel from the winning group

            // 3. Format the result cleanly as an HTML Hex string
            return $"#{dominantColor.R:X2}{dominantColor.G:X2}{dominantColor.B:X2}";
        }
        catch
        {
            return "#FFFFFF"; // Global safety catch-all
        }
    }

    }
}