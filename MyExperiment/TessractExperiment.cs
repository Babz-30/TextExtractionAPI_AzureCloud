using Microsoft.Extensions.Logging;
using MyExperiment.Source.Helpers;
using MyExperiment.Source.Preprocessing;
using MyExperiment.Source.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyExperiment
{
    /// <summary>
    /// SE project to extract text from images and Identify the Best preprocessing technique for an Image
    /// </summary>
    public class TessractExperiment
    {
        /// <summary>
        /// Extracts text from images using Tesseract OCR, performs embeddings using OpenAPI and calculates cosine similarity, to find the best
        /// preprocessing tecnique and verifies dictionary accuracy to detect the best preprocessing technique
        /// </summary>
        /// <param name="inputImage">Image with text</param>
        /// <param name="logger">Log for the application</param>
        /// <returns>BestTechnique output object with all necessary details for the experiment</returns>
        public static async Task<BestTechnique> RunExperiment(string inputImage, ILogger logger)
        {
            try
            {
                logger?.LogInformation("Computing the best pre-processing technique to extract text from image by tesseract OCR.");

                var config = Configuration.Config();

                // Path to output cosine similarity matrix file
                string cosineSimilarityPath = UtilityClass.CosineSimilarityOutputPath(UtilityClass.GetRequiredConfigValue(config, "CosineSimilarityFileName"));

                //Preprocessing Techniques
                List<string> techniques = UtilityClass.GetConfigurationList<string>(config, "Techniques");

                // Apply preprocessing techniques
                Dictionary<string, string> preprocessedImages = PreprocessingFactory.ApplyPreprocessing(inputImage, techniques);

                // Perform OCR text extraction on preprocessed images
                Dictionary<string, string> ocrTexts = TextExtraction.GetTexts(preprocessedImages);

                logger?.LogInformation("Computing Embeddings for extracted text and then calculating cosine similarity between preprocessing techniques...");

                // Generate embeddings of the extracted texts
                Dictionary<string, List<double>> textEmbeddings = await TextEmbedding.GetTextEmbeddingsAsync(ocrTexts);

                // Compute Cosine Similarity between text embeddings
                TextSimilarity.GenerateCosineSimilarityMatrix(textEmbeddings, cosineSimilarityPath);

                // Data mining to determine the best preprocessing technique and generate plot
                BestTechnique bestTechnique = Results.PrintResults(cosineSimilarityPath, ocrTexts);

                return bestTechnique;
            }
            catch (Exception ex)
            {
                logger?.LogError($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
