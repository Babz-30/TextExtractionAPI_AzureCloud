using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace MyExperiment.Source.Preprocessing
{
    public class ResizeImage
    {
        /// <summary>
        /// Changes the resolution of image based on passed DPI
        /// </summary>
        /// <param name="inputImagePath">Path to input image.</param>
        /// <param name="outputImagePath">Path to output image.</param>
        /// <param name="targetDPI">Target resolution in DPI.</param>
        /// <returns>Path to output processed image.</returns>
        public static string ResizingImage(string inputImagePath, string outputImagePath, int targetDPI)
        {
            try
            {
                ResizeAndSetDPI(inputImagePath, outputImagePath, targetDPI);
                return outputImagePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during image resizing: {ex.Message}");
                throw;
            }
        }

        private static void ResizeAndSetDPI(string inputPath, string outputPath, int targetDPI)
        {
            using var image = Image.Load(inputPath);

            float dpiX = (float)image.Metadata.HorizontalResolution;
            float dpiY = (float)image.Metadata.VerticalResolution;

            // Fallback if DPI is not set
            if (dpiX <= 0)
            {
                dpiX = 96;
            }

            if (dpiY <= 0)
            {
                dpiY = 96;
            }

            // Calculate scaling ratio
            float scaleX = targetDPI / dpiX;
            float scaleY = targetDPI / dpiY;

            int newWidth = (int)(image.Width * scaleX);
            int newHeight = (int)(image.Height * scaleY);

            // Cap dimensions to avoid excessive memory use
            const int maxDimension = 10000;
            if (newWidth > maxDimension || newHeight > maxDimension)
            {
                float capScale = Math.Min((float)maxDimension / image.Width, (float)maxDimension / image.Height);
                newWidth = (int)(image.Width * capScale);
                newHeight = (int)(image.Height * capScale);
            }

            image.Mutate(x => x.Resize(newWidth, newHeight));

            image.Metadata.HorizontalResolution = targetDPI;
            image.Metadata.VerticalResolution = targetDPI;

            // Choose encoder
            var ext = Path.GetExtension(outputPath).ToLower();
            IImageEncoder encoder = ext switch
            {
                ".png" => new PngEncoder(),
                ".jpg" or ".jpeg" => new JpegEncoder(),
                ".bmp" => new BmpEncoder(),
                _ => throw new NotSupportedException($"Unsupported file format: {ext}")
            };

            image.Save(outputPath, encoder);
        }
    }
}
