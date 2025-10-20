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
    /// <summary>
    /// Inverts image colors (light to dark and vice versa) in a cross-platform way.
    /// </summary>
    public class InvertImage
    {
        /// <summary>
        /// Inverts the colors of the image using ImageSharp.
        /// </summary>
        /// <param name="inputImagePath">Path to the input image.</param>
        /// <param name="outputImagePath">Path to save the output image.</param>
        /// <returns>Path to the saved, processed image.</returns>
        public static string InvertingImage(string inputImagePath, string outputImagePath)
        {
            try
            {
                using var image = Image.Load<Rgba32>(inputImagePath);

                // Apply inversion
                image.Mutate(ctx =>
                {
                    ctx.Invert();
                });
                
                // Pick encoder based on extension
                string ext = Path.GetExtension(outputImagePath).ToLower();
                IImageEncoder encoder = ext switch
                {
                    ".jpg" or ".jpeg" => new JpegEncoder(),
                    ".png" => new PngEncoder(),
                    ".bmp" => new BmpEncoder(),
                    _ => throw new NotSupportedException($"Unsupported format: {ext}")
                };

                // Save the image 
                image.Save(outputImagePath, encoder);

                return outputImagePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during image invert: {ex.Message}");
                throw;
            }
        }
    }
}
