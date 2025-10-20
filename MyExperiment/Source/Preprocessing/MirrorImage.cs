using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace MyExperiment.Source.Preprocessing
{
    public class MirrorImage
    {
        /// <summary>
        /// Applies a horizontal mirror transformation to an image.
        /// </summary>
        /// <param name="inputImagePath">Path to input image.</param>
        /// <param name="outputImagePath">Path to save mirrored image.</param>
        /// <returns>Path to mirrored image.</returns>
        public static string Process(string inputImagePath, string outputImagePath)
        {
            try
            {
                using Image<Rgba32> image = Image.Load<Rgba32>(inputImagePath);

                image.Mutate(x => x.Flip(FlipMode.Horizontal));

                image.Save(outputImagePath);

                return outputImagePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during mirror image processing: {ex.Message}");
                throw;
            }
        }
    }
}
