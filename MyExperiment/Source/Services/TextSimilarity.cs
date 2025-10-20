using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyExperiment.Source.Services
{
    public class TextSimilarity
    {
        /// <summary>
        /// Computes the cosine similarity between two numeric vectors.
        /// Cosine similarity measures the angle between two vectors in a multi-dimensional space.
        /// </summary>
        /// <param name="vectorA">First vector of numerical values.</param>
        /// <param name="vectorB">Second vector of numerical values.</param>
        /// <returns>Cosine similarity score ranging from -1 (opposite) to 1 (identical).</returns>
        public static double ComputeCosineSimilarity(List<double> vectorA, List<double> vectorB)
        {
            double dotProduct = 0, magA = 0, magB = 0;

            // Ensure both vectors have the same length before computation
            if (vectorA.Count != vectorB.Count)
            {
                throw new ArgumentException("Vectors must have the same dimensions.");
            }

            // Compute dot product and magnitudes of the vectors
            for (int i = 0; i < vectorA.Count; i++)
            {
                dotProduct += vectorA[i] * vectorB[i]; // Summing element-wise products
                magA += Math.Pow(vectorA[i], 2); // Squaring elements for magnitude A
                magB += Math.Pow(vectorB[i], 2); // Squaring elements for magnitude B
            }

            // Compute cosine similarity (avoid division by zero)
            return (magA == 0 || magB == 0) ? 0 : dotProduct / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }

        /// <summary>
        /// Generates the cosine similarity matrix and saves it to a CSV
        /// </summary>
        /// <param name="preprocessingEmbeddings"></param>
        /// <param name="filePath">File path of saved cosine similarity matrix</param>
        public static void GenerateCosineSimilarityMatrix(Dictionary<string, List<double>> preprocessingEmbeddings, string filePath)
        {
            // Remove entries with List<double> where all values are 0
            var keysToRemove = preprocessingEmbeddings
                .Where(pair => pair.Value.All(value => value == 0.0))
                .Select(pair => pair.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                preprocessingEmbeddings.Remove(key);
            }

            // Get the list of preprocessing techniques (keys)
            var techniques = preprocessingEmbeddings.Keys.ToList();

            // Initialize the matrix
            var matrix = new double[techniques.Count, techniques.Count];

            // Calculate cosine similarity for each pair
            for (int i = 0; i < techniques.Count; i++)
            {
                for (int j = 0; j < techniques.Count; j++)
                {
                    matrix[i, j] = ComputeCosineSimilarity(preprocessingEmbeddings[techniques[i]], preprocessingEmbeddings[techniques[j]]);
                }
            }

            // Write the matrix to a CSV file
            using (var writer = new StreamWriter(filePath))
            {
                // Write the header row
                writer.WriteLine(string.Join(",", new[] { "" }.Concat(techniques)));

                // Write each row
                for (int i = 0; i < techniques.Count; i++)
                {
                    var row = new List<string> { techniques[i] };
                    row.AddRange(matrix.GetRow(i).Select(value => value.ToString("F4"))); // Format the value with 4 decimal places
                    writer.WriteLine(string.Join(",", row));
                }
            }

            Console.WriteLine($"Cosine similarity matrix has been saved.");
        }
    }

    public static class MatrixExtensions
    {
        // Helper extension to get a row from a 2D array
        public static IEnumerable<double> GetRow(this double[,] matrix, int rowIndex)
        {
            int columns = matrix.GetLength(1);
            for (int i = 0; i < columns; i++)
            {
                yield return matrix[rowIndex, i];
            }
        }
    }
}
