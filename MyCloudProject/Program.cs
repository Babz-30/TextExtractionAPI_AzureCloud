using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using MyExperiment;
using MyExperiment.Source.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

/*
 *Author: Team - TechRookies Babitha Nadar and Akanksha
 *Date Created: August 18, 2025
 *Project: ML 24/25-10 Creating Text from images with OCR API
 *Moving to Azure cloud to implement SE Proj on large scale for multiple datasets having images with text
 *
 */
namespace MyCloudProject
{
    class Program
    {
        /// <summary>
        /// ML 24/25-10 Creating Text from images with OCR API 
        /// </summary>
        private static readonly string _projectName = "ML 24/25-10 Creating Text from images with OCR API";

        static async Task Main(string[] args)
        {
            CancellationTokenSource tokeSrc = new();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                tokeSrc.Cancel();
            };

            Console.WriteLine($"Started experiment: {_projectName}");

            // Init configuration
            var cfgRoot = Common.InitHelpers.InitConfiguration(args);

            var cfgSec = cfgRoot.GetSection("MyConfig");

            // InitLogging and other components
            var logFactory = InitHelpers.InitLogging(cfgRoot);
          
            var logger = logFactory.CreateLogger("Train.Console.Logs");

            logger?.LogInformation($"{DateTime.Now} -  Started experiment: {_projectName}");

            AzureStorageProvider storageProvider = new(cfgSec, logger);

            Experiment experiment = new(cfgSec, storageProvider, logger);

            ExperimentResult result = null; int indicator = 0;

            //OneTime SetUp Download for ML experiment execution
            await storageProvider.DownloadSetUpAsync();

            //Implements the step 3 in the architecture picture.
            while (tokeSrc.Token.IsCancellationRequested == false)
            {
                logger?.LogInformation($"Waiting for new request in queue.");
                // Step 3 Waiting for message in queue
                IExerimentRequest request = storageProvider.ReceiveExperimentRequestAsync(tokeSrc.Token);

                if (request != null)
                {
                    try
                    {
                        // Processing of the request begins
                        var inputFile = request.InputFile;
                        
                        // Indicator for combined result 
                        indicator = 1;

                        // Step 4. Downloading Image from Blob Storage
                        string localFileWithInputArgs = await storageProvider.DownloadInputAsync(inputFile);

                        // SE Project code started.(Between steps 4 and 5).
                        experiment.ExerimentRequest = request;
                        result = (ExperimentResult)await experiment.RunAsync(localFileWithInputArgs);

                        // Step 5. Uploading results to storage table and blob storage
                        await storageProvider.UploadExperimentResult(result);

                        // Removing processed request from the queue
                        await storageProvider.CommitRequestAsync(request);

                    }
                    catch (Exception ex)
                    {
                        // Log any errors that occur during the experiment run.
                        logger?.LogError(ex, "An error occurred while running the experiment.");
                        indicator = 0;
                    }
                    finally
                    {
                        logger?.LogInformation("Deleting temporary files!");
                        UtilityClass.DeleteAllFiles();
                    }
                }
                else
                {
                    if (indicator == 1)
                    {
                        // Uploading combined graph plotted to Blob storage for Dataset
                        await storageProvider.UploadResultAsync(result.ExperimentId, result);
                        indicator = 0;
                    }
                    logger?.LogInformation("Queue empty...");
                    // Delaying 5sec until next check for new request
                    await Task.Delay(5000);
                }
            }
            logger?.LogInformation($"{DateTime.Now} -  Experiment exit: {_projectName}");
        }
    }
}
