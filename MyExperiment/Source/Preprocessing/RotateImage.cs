using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace MyExperiment.Source.Preprocessing
{
    public class RotateImage
    {
        /// <summary>
        /// Rotates the image by a specified angle.
        /// </summary>
        /// <param name="inputPath">Path to input image.</param>
        /// <param name="outputImagePath">Path to output image.</param>
        /// <param name="angle">Angle in degrees to rotate image.</param>
        /// <returns>Path to the output image.</returns>
        public static string ApplyRotation(string inputPath, string outputImagePath, float angle)
        {
            try
            {
                using var image = Image.Load<Rgba32>(inputPath);

                image.Mutate(x => x.Rotate(angle));

                // Determine encoder from file extension
                var ext = Path.GetExtension(outputImagePath).ToLower();
                IImageEncoder encoder = ext switch
                {
                    ".png" => new PngEncoder(),
                    ".jpg" or ".jpeg" => new JpegEncoder(),
                    ".bmp" => new BmpEncoder(),
                    _ => throw new NotSupportedException($"Unsupported file format: {ext}")
                };

                image.Save(outputImagePath, encoder);

                return outputImagePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during image rotation: {ex.Message}");
                throw;
            }
        }
    }
}
