using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using MyExperiment.Source.Helpers;
using System;
using System.Threading.Tasks;

namespace MyExperiment
{
    /// <summary>
    /// This class implements the ML experiment that will run in the cloud. This is refactored code from my SE project.
    /// </summary>
    public class Experiment : IExperiment
    {
        private readonly IStorageProvider storageProvider;

        private readonly ILogger logger;

        private readonly MyConfig config;

        public IExerimentRequest ExerimentRequest { get; set; }

        public Experiment(IConfigurationSection configSection, IStorageProvider storageProvider, ILogger log)
        {
            this.storageProvider = storageProvider;
            logger = log;

            config = new MyConfig();
            configSection.Bind(config);
        }

        /// <summary>
        /// Experiment to extract text from images using tesseract OCR after applying several preprocessing techniques
        /// Refer https://github.com/Babz-30/TextExtractionAPI_Team_TechRookies
        /// </summary>
        /// <param name="inputData">Image with text</param>
        /// <returns>Experiment result with best preprocessing technique and its extracted text.</returns>
        public async Task<IExperimentResult> RunAsync(string inputData)
        {
            // Generate a unique RowKey for each experiment result
            string uniqueRowKey = Guid.NewGuid().ToString();

            ExperimentResult res = new(config.GroupId, uniqueRowKey)
            {
                StartTimeUtc = DateTime.UtcNow
            };
            logger?.LogInformation($"{DateTime.UtcNow} - TesseractExperiment Experiment started.");

            // Run experiment to extract text from image
            BestTechnique expResult = await TessractExperiment.RunExperiment(inputData, logger);
           
            logger?.LogInformation($"{DateTime.UtcNow} - TesseractExperiment Experiment completed.");

            res.EndTimeUtc = DateTime.UtcNow;

            var elapsedTime = res.EndTimeUtc - res.StartTimeUtc;
            Console.WriteLine($"Elapsed Seconds: {elapsedTime.GetValueOrDefault().TotalSeconds}");
            res.DurationSec = (long)elapsedTime.GetValueOrDefault().TotalSeconds;
            res.InputFileUrl = inputData;
            res.ExperimentId = ExerimentRequest.ExperimentId;
            res.Name = ExerimentRequest.Name;
            res.Description = ExerimentRequest.Description;
            res.BestTechniqueResult = expResult;
            res.Accuracy = (float)(expResult?.DictionaryAccuracy ?? 0f);

            return await Task.FromResult<IExperimentResult>(res); 
        }
    }
}
