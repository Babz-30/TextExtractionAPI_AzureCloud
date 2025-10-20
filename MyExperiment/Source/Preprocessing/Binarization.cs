using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace MyExperiment.Source.Preprocessing
{
    public static class Binarization
    {
        /// <summary>
        /// Applies Otsu's binarization to convert a grayscale image into a binary (black and white) image.
        /// </summary>
        /// <param name="inputImagePath">Path to the input image.</param>
        /// <param name="outputImagePath">Path where the binarized image will be saved.</param>
        /// <returns>Path to the output binarized image.</returns>
        public static string ApplyOtsuBinarization(string inputImagePath, string outputImagePath)
        {
            try
            {
                using var image = Image.Load<L8>(inputImagePath); // Grayscale
                int width = image.Width;
                int height = image.Height;

                // Build histogram
                int[] histogram = new int[256];
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        for (int x = 0; x < width; x++)
                        {
                            byte intensity = row[x].PackedValue;
                            histogram[intensity]++;
                        }
                    }
                });

                // Compute Otsu threshold
                int totalPixels = width * height;
                int sum = 0;
                for (int t = 0; t < 256; t++)
                {
                    sum += t * histogram[t];
                }

                int sumB = 0, wB = 0, wF = 0;
                float maxVariance = 0;
                int threshold = 0;

                for (int t = 0; t < 256; t++)
                {
                    wB += histogram[t];
                    if (wB == 0)
                    {
                        continue;
                    }

                    wF = totalPixels - wB;
                    if (wF == 0)
                    {
                        break;
                    }

                    sumB += t * histogram[t];
                    float mB = sumB / (float)wB;
                    float mF = (sum - sumB) / (float)wF;

                    float varianceBetween = wB * wF * (mB - mF) * (mB - mF);
                    if (varianceBetween > maxVariance)
                    {
                        maxVariance = varianceBetween;
                        threshold = t;
                    }
                }

                // Binarize the image
                image.Mutate(ctx =>
                {
                    ctx.BinaryThreshold(threshold / 255.0f);
                });

                Directory.CreateDirectory(Path.GetDirectoryName(outputImagePath)!);
                image.Save(outputImagePath, new PngEncoder());

                return outputImagePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during Otsu binarization: {ex.Message}");
                throw;
            }
        }
    }
}
