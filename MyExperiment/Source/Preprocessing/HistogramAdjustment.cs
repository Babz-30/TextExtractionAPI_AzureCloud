using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace MyExperiment.Source.Preprocessing
{
    public class HistogramAdjustment
    {
        /// <summary>
        /// Adjusts the histogram of an image by applying saturation and intensity modifications.
        /// </summary>
        /// <param name="inputImagePath">Path to the input image.</param>
        /// <param name="outputImagePath">Path where the adjusted image will be saved.</param>
        /// <param name="saturationFactor">Factor by which to adjust image saturation.</param>        
        /// <param name="intensityFactor">Factor by which to adjust image intensity.</param>
        /// <returns>Path to the output adjusted image.</returns>
        public static string ApplyHistogramAdjustment(string inputImagePath, string outputImagePath, double saturationFactor, double intensityFactor)
        {
            try
            {
                using Image<Rgba32> inputImage = Image.Load<Rgba32>(inputImagePath);

                // Use the same ProcessPixelRows approach that works in your Binarization code
                inputImage.ProcessPixelRows(accessor =>
                {
                    int width = inputImage.Width;
                    int height = inputImage.Height;

                    for (int y = 0; y < height; y++)
                    {
                        // Get the span for the current row of pixels from the accessor
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                        for (int x = 0; x < width; x++)
                        {
                            ref Rgba32 pixel = ref pixelRow[x];

                            var converter = new ColorSpaceConverter();
                            var hsl = converter.ToHsl(pixel);

                            // Convert from Rgba32 to HSL for adjustment
                            //Hsl hsl = ColorSpaceConverter.ToHsl(pixel);

                            // Apply the saturation and intensity (Lightness) factors
                            float newSaturation = Math.Clamp(hsl.S * (float)saturationFactor, 0f, 1f);
                            float newLightness = Math.Clamp(hsl.L * (float)intensityFactor, 0f, 1f);

                            // Create a new Hsl color with the adjusted values
                            Hsl adjustedHsl = new(hsl.H, newSaturation, newLightness);

                            // Convert the adjusted Hsl back to Rgba32 and assign it to the pixel
                            pixel = converter.ToRgb(adjustedHsl);
                        }
                    }
                });

                inputImage.Save(outputImagePath);

                return outputImagePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during histogram adjustment: {ex.Message}");
                throw;
            }
        }
    }
}