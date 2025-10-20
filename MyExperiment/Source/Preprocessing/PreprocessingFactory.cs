using MyExperiment.Source.Helpers;
using System.Collections.Generic;

namespace MyExperiment.Source.Preprocessing
{
    public class PreprocessingFactory
    {
        /// <summary>
        /// Produces preprocessed image based on technique selected for different variation within the technique.
        /// </summary>
        /// <param name="imagePath">Path to input image.</param>
        /// <param name="technique">Preprocessing technique.</param>
        /// <returns>Dictionary of preprocessing technique (with variations) as key and value as the processed image path.</returns>
        public static Dictionary<string, string> PreprocessImage(string imagePath, string technique)
        {
            Dictionary<string, string> processedImages = [];

            // Define output directory for processed images
            string outputDir = UtilityClass.OutputImageDirectory();
            string variation;
            string outputImagePath;

            var config = Configuration.Config();

            // Define parameter sets for different preprocessing techniques
            float[] rotateAngles = UtilityClass.GetConfigurationArray<float>(config, "PreprocessingSettings:RotateAngles");
            int[] thresholds = UtilityClass.GetConfigurationArray<int>(config, "PreprocessingSettings:Thresholds");
            int[] targerDPIs = UtilityClass.GetConfigurationArray<int>(config, "PreprocessingSettings:TargetDPIs");
            double[] satFactors = UtilityClass.GetConfigurationArray<double>(config, "PreprocessingSettings:SatFactors");
            double[] intensityFactors = UtilityClass.GetConfigurationArray<double>(config, "PreprocessingSettings:IntensityFactors");

            switch (technique)
            {
                case "rotation_resize":
                    // Apply rotation to the image for different angles and resize variations

                    foreach (float angle in rotateAngles)
                    {
                        variation = $"rotated_{angle}";
                        outputImagePath = $"{outputDir}/{variation}.jpg";
                        var rotatedImg = RotateImage.ApplyRotation(imagePath, outputImagePath, angle);

                        foreach (int targerDPI in targerDPIs)
                        {
                            variation = $"rotated_{angle}_resized_{targerDPI}";
                            outputImagePath = $"{outputDir}/{variation}.jpg";
                            ResizeImage.ResizingImage(rotatedImg, outputImagePath, targerDPI);
                            processedImages[variation] = outputImagePath;
                        }
                    }
                    break;

                case "cannyfilter_invert":
                    // Apply Canny edge detection and invert the resulting image

                    foreach (int threshold in thresholds)
                    {
                        variation = $"cannyfilter_{threshold}";
                        outputImagePath = $"{outputDir}/{variation}.jpg";
                        var cannyImage = CannyFilter.ApplyCannyEdgeDetection(imagePath, outputImagePath, threshold);

                        variation = $"cannyfilter_{threshold}_invert";
                        outputImagePath = $"{outputDir}/{variation}.jpg";
                        InvertImage.InvertingImage(cannyImage, outputImagePath);
                        processedImages[variation] = outputImagePath;
                    }
                    break;

                case "denoise":
                    // Convert image to grayscale
                    variation = "grayscale";
                    outputImagePath = $"{outputDir}/{variation}.jpg";
                    var imgGray = Grayscale.ConvertToGrayscale(imagePath, outputImagePath);

                    // Denoise image
                    variation = "grayscale_binarized";
                    outputImagePath = $"{outputDir}/{variation}.jpg";
                    Binarization.ApplyOtsuBinarization(imgGray, outputImagePath);
                    processedImages[variation] = outputImagePath;
                    break;

                case "chainfilter":
                    // Apply a sequence of filters: grayscale -> binarization -> resize
                    variation = "grayscale";
                    outputImagePath = $"{outputDir}/{variation}.jpg";
                    var grayImg = Grayscale.ConvertToGrayscale(imagePath, outputImagePath);

                    variation = "grayscale_binarize";
                    outputImagePath = $"{outputDir}/{variation}.jpg";
                    var binImg = Binarization.ApplyOtsuBinarization(grayImg, outputImagePath);

                    foreach (int targerDPI in targerDPIs)
                    {
                        variation = $"grayscale_binarize_resized_{targerDPI}";
                        outputImagePath = $"{outputDir}/{variation}.jpg";
                        ResizeImage.ResizingImage(binImg, outputImagePath, targerDPI);
                        processedImages[variation] = outputImagePath;
                    }
                    break;

                case "invert":
                    // Invert the image
                    variation = "inverted";
                    outputImagePath = $"{outputDir}/{variation}.jpg";
                    InvertImage.InvertingImage(imagePath, outputImagePath);
                    processedImages[variation] = outputImagePath;
                    break;

                case "hsi_adjustment":
                    // Apply Histogram Adjustment with different saturation and intensity factors

                    foreach (double sat in satFactors)
                    {
                        foreach (double intensity in intensityFactors)
                        {
                            variation = $"hsi_s{sat}_i{intensity}";
                            outputImagePath = $"{outputDir}/{variation}.jpg";
                            HistogramAdjustment.ApplyHistogramAdjustment(imagePath, outputImagePath, sat, intensity);
                            processedImages[variation] = outputImagePath;
                        }
                    }
                    break;

                case "mirror_horizontal":
                    // Mirror the image horizontally
                    variation = "mirror";
                    outputImagePath = $"{outputDir}/{variation}.jpg";
                    MirrorImage.Process(imagePath, outputImagePath);
                    processedImages[variation] = outputImagePath;
                    break;

                default:
                    // If no valid technique is provided, return original image
                    processedImages[technique] = imagePath;
                    break;
            }

            return processedImages; // Return dictionary containing variations and processed image paths
        }



        /// <summary>
        ///  Generates dictionary of all combined preprocessing technique (with variations) as key and value as the processed image path.
        /// </summary>
        /// <param name="inputImagePath">Path to input image.</param>
        /// <param name="techniques">List of preprocessing technique.</param>
        /// <returns>Combined dictionary of all preprocessing technique with variations</returns>
        public static Dictionary<string, string> ApplyPreprocessing(string inputImagePath, List<string> techniques)
        {
            Dictionary<string, string> preprocessedImages = [];

            foreach (var technique in techniques)
            {
                Dictionary<string, string> processedImages = PreprocessingFactory.PreprocessImage(inputImagePath, technique);

                foreach (var img in processedImages)
                {
                    // Store all preprocessed variations
                    preprocessedImages[img.Key] = img.Value;
                }
            }

            return preprocessedImages;
        }
    }
}
