using CsvHelper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MyExperiment.Source.Helpers
{
    /// <summary>
    /// Processes OCR results from a CSV file and determines the best-performing technique.
    /// </summary>
    public class Results
    {
        /// <summary>
        /// Through Data mining technique, reads computations from .csv files 
        /// and determines & display the best preprocessing technique
        /// </summary>
        /// <param name="filePath">CosineSimilarityMatrix.csv filepath</param>
        /// <param name="ocrTexts">Text extracted from all preprocessed images</param>
        public static BestTechnique PrintResults(string filePath, Dictionary<string, string> ocrTexts)
        {
            var config = Configuration.Config();
            var max = config.GetValue<int>("Max", 10); //Select top Pre-processing technique with highest Mean Cosine Simirarity 
            var lines = File.ReadAllLines(filePath);
            var results = new List<(string Technique, double Mean)>();

            foreach (var line in lines.Skip(1)) // Skip header
            {
                var parts = line.Split(',');
                string technique = parts[0];
                var values = parts.Skip(1).Select(s => double.Parse(s)).ToList();
                double mean = values.Average();
                results.Add((technique, mean));
            }

            // Select top techniques based on Mean Cosine Similarity
            var top = results.OrderByDescending(r => r.Mean).Take(max);

            /*Console.WriteLine("\n================================================================================================");
            Console.WriteLine("Top Techniques with Highest Mean Cosine similarity:");
            foreach (var (Technique, Mean) in top)
            {
                Console.WriteLine($"{Technique}: {Mean:F4}");
            }
            Console.WriteLine("================================================================================================");
            */

            // Read CSV file to fetch dictionary accuracy
            string filePathConf = UtilityClass.TesseractOutputPath("ExtractedTextMeanConfidence.csv");

            using var reader = new StreamReader(filePathConf);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<OCRAnalysis>().ToList();

            // Merge top results with dictionary accuracy from the CSV
            var topWithAccuracy = top
                .Join(records,
                      result => result.Technique,
                      record => record.Technique,
                      (result, record) => new
                      {
                          result.Technique,
                          result.Mean,
                          record.DictionaryAccuracy,
                          record.MeanConfidence
                      })
                .OrderByDescending(r => r.DictionaryAccuracy)    // Sort first by Dictionary Accuracy
                .ThenByDescending(r => r.MeanConfidence)       // Then by Mean Confidence
                .Take(max).ToList();

            // Best technique
            if (topWithAccuracy == null || topWithAccuracy.Count == 0)
            {
                Console.WriteLine("No technique results available to select the best one.");
                return new BestTechnique();
            }
            var bestTechnique = topWithAccuracy.First();

            if (bestTechnique.DictionaryAccuracy == 0)
            {
                Console.WriteLine("The above Techniques have 0 Dictionary Accuracy, hence eliminating false similarity.");
                topWithAccuracy = [.. records.Where(r => r.DictionaryAccuracy > 0).Join(results,
                      result => result.Technique,
                      record => record.Technique,
                      (result, record) => new
                      {
                          result.Technique,
                          record.Mean,
                          result.DictionaryAccuracy,
                          result.MeanConfidence
                      }).OrderByDescending(r => r.DictionaryAccuracy).ThenByDescending(r => r.MeanConfidence).Take(max)];
                if (topWithAccuracy.Count != 0)
                {
                    bestTechnique = topWithAccuracy.First();
                }
            }

            var techniques = topWithAccuracy.Select(result => $"\"{result.Technique}\"").ToList();
            var confidence = topWithAccuracy.Select(result => result.MeanConfidence.ToString("F4")).ToList();
            var accuracy = topWithAccuracy.Select(result => result.DictionaryAccuracy.ToString("F4")).ToList();
            var means = topWithAccuracy.Select(result => result.Mean.ToString("F4")).ToList();

            // Display results
            /*Console.WriteLine("Top Techniques (sorted by Dictionary Accuracy and then by Mean Confidence):");
            foreach (var item in topWithAccuracy)
            {
                Console.WriteLine($"Technique: {item.Technique}, Dictionary Accuracy: {item.DictionaryAccuracy}, Mean Confidence: {item.MeanConfidence}");
            }
            Console.WriteLine("================================================================================================");
            */
            Console.WriteLine("For Bar Graph:");
            Console.WriteLine($"Technique: [{string.Join(", ", techniques)}]");
            Console.WriteLine($"DictionaryAccuracy: [{string.Join(", ", accuracy)}]");
            Console.WriteLine($"MeanConfidence: [{string.Join(", ", confidence)}]");
            Console.WriteLine($"MeanCosineSimilarity: [{string.Join(", ", means)}]");

            Console.WriteLine("================================================================================================");
            Console.WriteLine($"Best Technique: {bestTechnique.Technique} \nMean Cosine Similarity: {bestTechnique.Mean} \nDictionary Accuracy: {bestTechnique.DictionaryAccuracy} \nMean Confidence: {bestTechnique.MeanConfidence}");
            Console.WriteLine($"Extracted Text:\n{ocrTexts[bestTechnique.Technique]}");
            Console.WriteLine("================================================================================================");

            var filename = MLBarChart.GenerateBestTechniqueBarChart(techniques, means, accuracy, confidence);


            var expResult = new BestTechnique
            {
                Technique = bestTechnique.Technique,
                Mean = bestTechnique.Mean,
                DictionaryAccuracy = bestTechnique.DictionaryAccuracy,
                MeanConfidence = bestTechnique.MeanConfidence,
                Output = ocrTexts[bestTechnique.Technique],
                FileName = filename
            };
            return expResult;
        }
    }

    // Represents an OCR analysis record from the CSV file.
    class OCRAnalysis
    {
        public required string Technique { get; set; }          // Name of preprocessing technique
        public double DictionaryAccuracy { get; set; } // Accuracy percentage from dictionary
        public double MeanConfidence { get; set; }     // Mean confidence percentage
    }
}
