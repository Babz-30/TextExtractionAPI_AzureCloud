using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace MyExperiment.Source.Preprocessing
{
    /// <summary>
    /// Character Edge detecting filter 
    /// </summary>
    public class CannyFilter
    {
        /// <summary>
        /// Converts image to grayscale, applies Gaussian blur and Canny edge detection.
        /// </summary>
        /// <param name="inputImagePath">Path to input image.</param>
        /// <param name="outputImagePath">Path to output image.</param>
        /// <param name="threshold1">First threshold for the hysteresis procedure.</param>
        /// <param name="threshold2">Second threshold for the hysteresis procedure.</param>
        /// <returns>Path to processed image.</returns>
        public static string ApplyCannyEdgeDetection(string inputImagePath, string outputImagePath, int threshold1)
        {
            try
            {
                using (var image = Image.Load<L8>(inputImagePath))
                {
                    int width = image.Width;
                    int height = image.Height;

                    // Step 1: Gaussian Blur
                    image.Mutate(ctx => ctx.GaussianBlur(1.5f));

                    // Step 2: Prepare result image
                    var result = new Image<L8>(width, height);

                    // Sobel operator kernels
                    int[,] sobelX =
                    {
                        { -1, 0, 1 },
                        { -2, 0, 2 },
                        { -1, 0, 1 }
                    };

                    int[,] sobelY =
                    {
                        { -1, -2, -1 },
                        {  0,  0,  0 },
                        {  1,  2,  1 }
                    };

                    // Step 3: Apply Sobel filter
                    for (int y = 1; y < height - 1; y++)
                    {
                        for (int x = 1; x < width - 1; x++)
                        {
                            float gx = 0;
                            float gy = 0;

                            for (int ky = -1; ky <= 1; ky++)
                            {
                                for (int kx = -1; kx <= 1; kx++)
                                {
                                    byte intensity = image[x + kx, y + ky].PackedValue;
                                    gx += sobelX[ky + 1, kx + 1] * intensity;
                                    gy += sobelY[ky + 1, kx + 1] * intensity;
                                }
                            }

                            float magnitude = MathF.Sqrt(gx * gx + gy * gy);
                            result[x, y] = magnitude >= threshold1 ? new L8(255) : new L8(0);
                        }
                    }

                    // Step 4: Save output
                    result.Save(outputImagePath);
                }

                return outputImagePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during image edge detection: {ex.Message}");
                throw;
            }
        }
    }
}
