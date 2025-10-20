using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyExperiment.Source.Helpers
{
    public class MLBarChart
    {
        /// <summary>
        /// Final plot for ML project technique vs no of images with successrate
        /// </summary>
        /// <param name="data"> Best Technique for the image and noof images with the same best technique</param>
        /// <returns>bar chart image png</returns>
        public static string GenerateTechniqueVsImagesBarChart(Dictionary<string, int> data)
        {
            try
            {
                // Ensure 'Not Processed' exists for missing techniques
                if (!data.TryGetValue("Not Processed", out int failed))
                {
                    failed = 0;
                    data["Not Processed"] = failed;
                }

                // Total and successful image counts
                int total = data.Values.Sum();
                int successful = total - failed;
                double successRate = total > 0 ? (double)successful / total * 100 : 0;

                // Identify best technique (excluding "Not Processed")
                string bestTechnique = data
                    .Where(kvp => kvp.Key != "Not Processed" && kvp.Value > 0)
                    .OrderByDescending(kvp => kvp.Value)
                    .FirstOrDefault().Key ?? "None";

                // Sort techniques alphabetically
                var sortedTechniques = data.OrderBy(kvp => kvp.Key).ToList();

                // Create plot model with success rate in title
                var model = new PlotModel
                {
                    Title = $"Technique vs No.of Images Processed (Success Rate: {successRate:F1}% for Total: {total} images processed)",
                    Background = OxyColors.White
                };

                // Category Axis on Left (Y-axis for horizontal bars)
                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Left,
                    TickStyle = TickStyle.Outside
                };

                // Add category labels
                foreach (var kvp in sortedTechniques)
                {
                    categoryAxis.Labels.Add(kvp.Key);
                }
                model.Axes.Add(categoryAxis);

                // Linear Axis at Bottom (X-axis for horizontal bars)
                var valueAxis = new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Number of Images",
                    Minimum = 0,
                    Maximum = data.Values.Max() * 1.1, // Add 10% padding
                    MajorGridlineStyle = LineStyle.Solid,
                    MajorGridlineColor = OxyColor.FromRgb(230, 230, 230)
                };
                model.Axes.Add(valueAxis);

                // Create BarSeries (horizontal bars)
                var barSeries = new BarSeries
                {
                    LabelPlacement = LabelPlacement.Outside,
                    LabelFormatString = "{0}"
                };

                // Add bars with appropriate colors
                for (int i = 0; i < sortedTechniques.Count; i++)
                {
                    var kvp = sortedTechniques[i];
                    var color = GetBarColor(kvp.Key, bestTechnique);

                    var barItem = new BarItem(kvp.Value)
                    {
                        Color = color
                    };

                    barSeries.Items.Add(barItem);
                }

                model.Series.Add(barSeries);

                var filename = "FinalDatasetAnalysisPlot.png";


                // Export chart to PNG using SkiaSharp backend
                using var stream = File.Create(filename);
                var exporter = new PngExporter(1000, 600, 96);
                exporter.Export(model, stream);

                return filename;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate comparison chart: {ex.Message}", ex);
                return null;
            }
        }

        private static OxyColor GetBarColor(string technique, string bestTechnique)
        {
            return technique switch
            {
                "Not Processed" => OxyColors.Red,
                var t when t == bestTechnique => OxyColors.Green,
                _ => OxyColors.SteelBlue
            };
        }

        /// <summary>
        /// Best Technique plot for an Image among many techniques
        /// </summary>
        /// <param name="techniques">List of Techniques</param>
        /// <param name="meanCosine">List with MeanCosineSimilarity</param>
        /// <param name="dictAccuracy">List with DictionaryAccuracy</param>
        /// <param name="meanConf">List with MeanConfidence</param>
        /// <returns>bar chart image png</returns>
        public static string GenerateBestTechniqueBarChart(List<string> techniques, List<string> meanCosine, List<string> dictAccuracy, List<string> meanConf)
        {
            try
            {
                if (techniques == null || meanCosine == null || dictAccuracy == null || meanConf == null)
                {
                    Console.WriteLine("All input lists cannot be null");
                    return null;
                }

                if (techniques.Count != meanCosine.Count ||
                    techniques.Count != dictAccuracy.Count ||
                    techniques.Count != meanConf.Count)
                {
                    Console.WriteLine("All input lists must have the same length");
                    return null;
                }  

                var cosineValues = techniques.Select((t, i) => double.TryParse(meanCosine[i], out var v) ? v : 0.0).ToList();
                var accuracyValues = techniques.Select((t, i) => double.TryParse(dictAccuracy[i], out var v) ? v : 0.0).ToList();
                var confidenceValues = techniques.Select((t, i) => double.TryParse(meanConf[i], out var v) ? v : 0.0).ToList();

                // Determine the best technique (priority: DictionaryAccuracy -> MeanCosine -> MeanConfidence)
                int bestTechniqueIndex = Enumerable.Range(0, techniques.Count)
                    .OrderByDescending(i => accuracyValues[i])
                    .ThenByDescending(i => cosineValues[i])
                    .ThenByDescending(i => confidenceValues[i])
                    .First();
                var bestTechnique = techniques[bestTechniqueIndex];

                // Sort alphabetically for category axis
                var sortedIndices = Enumerable.Range(0, techniques.Count)
                    .OrderBy(i => techniques[i])
                    .ToArray();

                var model = new PlotModel
                {
                    Title = $"Comparison of Techniques (Best: {bestTechnique})",
                    Background = OxyColors.White,
                    IsLegendVisible = true
                };

                // Category axis (left for horizontal bars)
                var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
                foreach (var index in sortedIndices)
                {
                    categoryAxis.Labels.Add(techniques[index]);
                }

                model.Axes.Add(categoryAxis);

                // Value axis
                var valueAxis = new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Values",
                    Minimum = 0,
                    Maximum = 1.1,
                    MajorGridlineStyle = LineStyle.Solid,
                    MajorGridlineColor = OxyColor.FromRgb(230, 230, 230),
                    MajorStep = 0.2
                };
                model.Axes.Add(valueAxis);

                // Helper to lighten colors
                static OxyColor Lighten(OxyColor color, double factor)
                {
                    byte r = (byte)Math.Min(255, color.R + (255 - color.R) * factor);
                    byte g = (byte)Math.Min(255, color.G + (255 - color.G) * factor);
                    byte b = (byte)Math.Min(255, color.B + (255 - color.B) * factor);
                    return OxyColor.FromRgb(r, g, b);
                }

                // Series for each metric
                var meanCosineSeries = new BarSeries
                {
                    Title = "MeanCosineSimilarity",
                    FillColor = OxyColors.Blue,
                    BarWidth = 0.2
                };
                var dictAccuracySeries = new BarSeries
                {
                    Title = "DictionaryAccuracy",
                    FillColor = OxyColors.Green,
                    BarWidth = 0.2
                };
                var meanConfSeries = new BarSeries
                {
                    Title = "MeanConfidence",
                    FillColor = OxyColors.Red,
                    BarWidth = 0.2
                };

                // Add bars with highlight for best technique
                foreach (var index in sortedIndices)
                {
                    bool isBest = index == bestTechniqueIndex;

                    meanCosineSeries.Items.Add(new BarItem
                    {
                        Value = cosineValues[index],
                        Color = isBest ? OxyColors.Blue : Lighten(OxyColors.Blue, 0.5)
                    });
                    dictAccuracySeries.Items.Add(new BarItem
                    {
                        Value = accuracyValues[index],
                        Color = isBest ? OxyColors.Green : Lighten(OxyColors.Green, 0.5)
                    });
                    meanConfSeries.Items.Add(new BarItem
                    {
                        Value = confidenceValues[index],
                        Color = isBest ? OxyColors.Red : Lighten(OxyColors.Red, 0.5)
                    });
                }

                // Add series to model
                model.Series.Add(meanCosineSeries);
                model.Series.Add(dictAccuracySeries);
                model.Series.Add(meanConfSeries);

                // Manual Legend in top-right
                model.Annotations.Add(new RectangleAnnotation
                {
                    MinimumX = 0.65,
                    MaximumX = 1.05,
                    MinimumY = techniques.Count + 0.5,
                    MaximumY = techniques.Count + 2.5,
                    Fill = OxyColor.FromAColor(200, OxyColors.White),
                    Stroke = OxyColors.Black
                });

                model.Annotations.Add(new TextAnnotation
                {
                    Text = "Legend:\nBlue → MeanCosineSimilarity\nGreen → DictionaryAccuracy\nRed → MeanConfidence",
                    TextPosition = new DataPoint(1.0, techniques.Count + 2.2),
                    Stroke = OxyColors.Transparent,
                    TextHorizontalAlignment = HorizontalAlignment.Right,
                    TextVerticalAlignment = VerticalAlignment.Top,
                    FontSize = 12
                });

                var filename = "ImageBestTechniquePlot.png";
                using var stream = File.Create(filename);
                var exporter = new PngExporter(1200, 700, 96);
                exporter.Export(model, stream);

                return filename;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate comparison chart: {ex.Message}", ex);
                return null;
            }
        }
    }
}