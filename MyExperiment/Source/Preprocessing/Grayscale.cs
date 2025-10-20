using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MyExperiment.Source.Preprocessing
{
    public static class Grayscale
    {
        /// <summary>
        /// Converts an input image to grayscale using a weighted sum method.
        /// </summary>
        /// <param name="inputImagePath">Path to the input image.</param>
        /// <param name="outputImagePath">Path where the grayscale image will be saved.</param>
        /// <returns>Path to the output grayscale image.</returns>
        public static string ConvertToGrayscale(string inputImagePath, string outputImagePath)
        {
            try
            {
                // Load the image in Rgba32 format for easy color access
                using var image = Image.Load<Rgba32>(inputImagePath);

                // Use ProcessPixelRows to iterate through pixels efficiently
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                        for (int x = 0; x < accessor.Width; x++)
                        {
                            ref Rgba32 pixel = ref pixelRow[x];

                            // Convert to grayscale using weighted sum
                            int grayValue = (int)(0.3 * pixel.R + 0.59 * pixel.G + 0.11 * pixel.B);

                            // Set the pixel to the new grayscale value
                            pixel = new Rgba32((byte)grayValue, (byte)grayValue, (byte)grayValue);
                        }
                    }
                });

                // Save the processed grayscale image
                image.Save(outputImagePath);

                return outputImagePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during image grayscale conversion: {ex.Message}");
                throw;
            }
        }
    }
}