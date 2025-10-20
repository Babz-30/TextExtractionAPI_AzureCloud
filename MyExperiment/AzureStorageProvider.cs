using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using MyExperiment.Source.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MyExperiment
{
    /// <summary>
    /// All connectivity to Azure storage to store and retrieve data Blob, Table and Queue
    /// </summary>
    public class AzureStorageProvider : IStorageProvider
    {
        private readonly MyConfig _config;
        private readonly ILogger logger;
        private readonly string _connectionString;

        public AzureStorageProvider(IConfigurationSection configSection, ILogger log)
        {
            _config = new MyConfig();
            configSection.Bind(_config);
            logger = log;
            _connectionString = _config.StorageConnectionString;

        }

        /// <summary>
        /// Commit the request that was processed from the queue by deleting the message from the queue
        /// </summary>
        /// <param name="request">Request processed from the queue</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Task CommitRequestAsync(IExerimentRequest request)
        {
            return Task.Run(async () =>
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request), "Request cannot be null");
                }

                // Initialize the QueueClient with the storage connection string and queue name
                QueueClient queueClient = new(_connectionString, _config.Queue);

                try
                {
                    // Delete the message from the queue
                    await queueClient.DeleteMessageAsync(request.MessageId, request.MessageReceipt);

                    // Log that the message was successfully deleted
                    logger?.LogInformation($"{DateTime.Now} - Successfully Deleted the request with MessageId: {request.MessageId}");
                }
                catch (Exception ex)
                {
                    logger?.LogError($"{DateTime.Now} - An error occurred while deleting the request. Exception: {ex.Message}");
                }
                finally
                {
                    queueClient = null;
                }
            });
        }

        /// <summary>
        /// Downloading setup files for the experiment
        /// </summary>
        public async Task DownloadSetUpAsync()
        {
            try
            {
                var containerClient = new BlobContainerClient(_connectionString, _config.SetUpContainer);

                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
                {
                    string blobName = blobItem.Name;
                    var blobClient = containerClient.GetBlobClient(blobName);

                    string localFilePath = Path.Combine(UtilityClass.TrainedDataPath(), blobName);

                    logger?.LogInformation($"Downloading: {blobName} to {localFilePath}");
                    await blobClient.DownloadToAsync(localFilePath);
                    logger?.LogInformation($"Downloaded: {blobName}");
                }
            }
            catch (Exception ex)
            {
                // Log an error for any other exceptions during setup files download
                logger?.LogError($"{DateTime.Now} - An error occurred while downloading the setup files. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Downloading the input image from  blob storage
        /// </summary>
        /// <param name="blobName">Name of file in Blob Storage</param>
        /// <returns>Path of the file stored in memory</returns>
        public Task<string> DownloadInputAsync(string blobName)
        {
            return Task.Run(async () =>
            {
                string fileName = Path.GetFileName(blobName);

                string localFilePath = UtilityClass.InputImagePath(fileName);

                logger?.LogInformation($"Downloading: {blobName} to {localFilePath}");

                // Create the Blob client with the storage connection string, container name and blobName
                var blobClient = new BlobClient(_connectionString, _config.TrainingContainer, blobName);

                // Download to a local file
                await blobClient.DownloadToAsync(localFilePath);

                logger?.LogInformation($"Downloaded: {blobName}");

                return localFilePath;

            });
        }

        /// <summary>
        /// Receives experiment request for processing from queue until cancellation is requested from user
        /// </summary>
        /// <param name="token">Exit by keypress by user</param>
        /// <returns>Experiment Request for processing</returns>
        /// Returns null if no messages are received or the operation is canceled.
        public IExerimentRequest ReceiveExperimentRequestAsync(CancellationToken token)
        {
            return Task.Run(async () =>
            {
                // Set visibility timeout to the max calculated (~1020 seconds)
                TimeSpan visibilityTimeout = TimeSpan.FromSeconds(1020);

                // Initialize the QueueClient with the storage connection string and queue name
                QueueClient queueClient = new(_connectionString, _config.Queue);

                // Receive messages from the queue asynchronously with custom visibility timeout
                QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(maxMessages: 1, visibilityTimeout: visibilityTimeout);

                // Check if any messages were received
                if (messages != null && messages.Length > 0)
                {
                    foreach (var message in messages)
                    {
                        try
                        {
                            // Extract message text from the message body
                            string msgTxt = message.Body.ToString();

                            // Log that a message has been received
                            logger?.LogInformation($"{DateTime.Now} - Received the trigger-queue message:\n {msgTxt}");

                            // Deserialize the message JSON into an ExerimentRequestMessage object
                            var request = JsonSerializer.Deserialize<ExerimentRequestMessage>(msgTxt);

                            // Check if deserialization was successful
                            if (request != null)
                            {
                                var fileName = request.InputFile;
                                if (!(fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
                                {
                                    //Enqueue all the images in the folder request to individual image request
                                    await EnqueueFilesInFolderAsync(request, queueClient);

                                    // Assign message ID and pop receipt to the request object
                                    request.MessageId = message.MessageId;
                                    request.MessageReceipt = message.PopReceipt;

                                    //Deleting the folder request
                                    await CommitRequestAsync(request);

                                }
                                else
                                {
                                    // Assign message ID and pop receipt to the request object
                                    request.MessageId = message.MessageId;
                                    request.MessageReceipt = message.PopReceipt;
                                    logger?.LogInformation($"{DateTime.Now} - Received the trigger-queue message:\n {request.MessageId}");
                                    // Return the deserialized request object
                                    return (IExerimentRequest)request;
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            // Log an error if deserialization fails
                            logger?.LogError($"{DateTime.Now} - Failed to deserialize the message. Exception: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            // Log an error for any other exceptions during message processing
                            logger?.LogError($"{DateTime.Now} - An error occurred while processing the message. Exception: {ex.Message}");
                        }
                    }
                }

                // Return null if cancellation is requested or no messages are received
                return null;

            }).GetAwaiter().GetResult();
        }


        /// <summary>
        /// For request with location of folder having all the images, creates a request for each image and uploads in queue
        /// </summary>
        /// <param name="request">Request with folder as input</param>
        /// <param name="queueClient">Queue where the created requests are uploadded</param>
        private async Task EnqueueFilesInFolderAsync(ExerimentRequestMessage request, QueueClient queueClient)
        {
            var blobContainerClient = new BlobContainerClient(_connectionString, _config.TrainingContainer);

            try
            {
                var counter = 1;
                await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync(prefix: request.InputFile))
                {
                    if (blobItem != null)
                    {
                        {
                            string imageFile = blobItem.Name;

                            ExerimentRequestMessage requestMessage = new()
                            {
                                ExperimentId = $"{request.ExperimentId}",
                                InputFile = imageFile,
                                Name = "Dataset",
                                Description = $"{request.Description} for image {imageFile}",

                            };

                            string msgJson = JsonSerializer.Serialize<ExerimentRequestMessage>(requestMessage);

                            await queueClient.SendMessageAsync(msgJson);

                            logger?.LogInformation($"{DateTime.Now} - Enqueued the trigger-queue message:\n {msgJson}");
                            counter++;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log an error for any other exceptions during message enqueuing
                logger?.LogError($"{DateTime.Now} - An error occurred while enqueuing the message. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Uploading each request processed succesfully in table storage with the details of text extracted and best technique
        /// and uploading plotted graph showing best technique for individual image processed in Blob storage
        /// </summary>
        /// <param name="result">Result of the experiment</param>
        public Task UploadExperimentResult(ExperimentResult result)
        {
            return Task.Run(async () =>
            {
                try
                {
                    logger?.LogInformation("Uploading result to Table Storage.");
                    // New instance of the TableClient class
                    TableServiceClient tableServiceClient = new(_connectionString);
                    TableClient tableClient = tableServiceClient.GetTableClient(tableName: _config.ResultTable);
                    await tableClient.CreateIfNotExistsAsync();

                    // Creating a table entity from the result
                    var entity = new TableEntity(result.PartitionKey, result.RowKey)
                    {
                        { "ExperimentId", result.ExperimentId },
                        { "InputImage", result.InputFileUrl },
                        { "Name", result.Name },
                        { "Description", result.Description },
                        { "StartTimeUtc", result.StartTimeUtc },
                        { "EndTimeUtc", result.EndTimeUtc },
                        { "DurationInSec", result.DurationSec },
                        { "Technique", result.BestTechniqueResult?.Technique ?? string.Empty },
                        { "ExtractedText", result.BestTechniqueResult?.Output ?? string.Empty },
                        { "CosineSimilarityMean", result.BestTechniqueResult?.Mean ?? 0.0 },
                        { "DictionaryAccuracy", result.Accuracy },
                        { "MeanConfidence", result.BestTechniqueResult?.MeanConfidence ?? 0.0 },
                        { "Timestamp", DateTimeOffset.UtcNow },

                    };

                    // Adding the newly created entity to the Azure Table.
                    await tableClient.AddEntityAsync(entity);

                    logger?.LogInformation("Successfully uploaded the result into Table Storage.");

                    string outputFile = result.BestTechniqueResult?.FileName;

                    if (!string.IsNullOrWhiteSpace(outputFile))
                    {

                        logger?.LogInformation("Uploading all processed files for individual image to blob storage.");
                        // Initialize the BlobServiceClient with the storage connection string from the configuration
                        BlobServiceClient blobServiceClient = new(_connectionString);

                        // Initialize the BlobContainerClient for the specified result container from the configuration
                        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_config.ResultContainer);

                        // Create the container if it does not already exist (asynchronously)
                        await containerClient.CreateIfNotExistsAsync();

                        //Uploading all intermediate processed files
                        string virtualFolderName = $"{result.ExperimentId}-{Guid.NewGuid()}-{Path.GetFileNameWithoutExtension(result.InputFileUrl)}";
                        
                        await UploadFolderAsync(UtilityClass.OutputDirectory(), virtualFolderName, containerClient );

                        string uniqueBlobName = $"{virtualFolderName}/ImageBestTechniquePlot-{Path.GetFileName(result.InputFileUrl)}";

                        // Initialize the BlobClient for the specific blob using the output file's name
                        BlobClient blobClient = containerClient.GetBlobClient(uniqueBlobName);

                        // Upload the file to the blob storage, overwriting it if it already exists (asynchronously)
                        await blobClient.UploadAsync(outputFile, overwrite: true);

                        logger?.LogInformation("Successfully uploaded all processed files into Blob Storage.");

                        // Delete file after upload
                        if (File.Exists(outputFile))
                        {
                            File.Delete(outputFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur
                    logger?.LogError($"{DateTime.Now} - An error occurred while uploading the Results to Storage. Exception: {ex.Message}");
                }
                finally
                {
                    logger?.LogInformation("Deleting temporary files!");
                    UtilityClass.DeleteAllFiles();
                }
            });
        }

        /// <summary>
        /// Storing all intermediate folder with processed files in blob storage
        /// </summary>
        /// <param name="localFolderPath">local folder path</param>
        /// <param name="virtualFolderName">blob storage folder path</param>
        /// <param name="containerClient">blob containe client to handle blobs</param>
        /// <returns></returns>
        public static async Task UploadFolderAsync(string localFolderPath, string virtualFolderName, BlobContainerClient containerClient)
        {
            foreach (string filePath in Directory.GetFiles(localFolderPath, "*", SearchOption.AllDirectories))
            {
                // Get relative path inside the folder
                string relativePath = Path.GetRelativePath(localFolderPath, filePath);

                // Build blob name (with optional virtual folder prefix)
                string blobName = string.IsNullOrEmpty(virtualFolderName)
                    ? relativePath.Replace("\\", "/")   
                    : $"{virtualFolderName}/{relativePath.Replace("\\", "/")}";

                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Upload blob to storage
                using (FileStream fs = File.OpenRead(filePath))
                {
                    await blobClient.UploadAsync(fs, overwrite: true);
                }
            }
        }


        /// <summary>
        /// Uploading graph plotted (Technique vs no of Images processed) for final result
        /// </summary>
        /// <param name="ExperimentId">Unique Id set by user for experiment</param>
        /// <param name="result">Experiment output details</param>
        public async Task UploadResultAsync(string ExperimentId, IExperimentResult result)
        {
            try
            {

                logger?.LogInformation($"{DateTime.Now} - Uploading the combined plotted graph to blob storage.");

                TableServiceClient tableServiceClient = new(_connectionString);
                TableClient tableClient = tableServiceClient.GetTableClient(tableName: _config.ResultTable);
                var queryResults = tableClient.QueryAsync<TableEntity>();

                //Uploading combined graph plotted for the complete dataset in blob storage
                var data = new Dictionary<string, int>();

                await foreach (var entity in queryResults)
                {

                    string fieldValue = entity["ExperimentId"]?.ToString();
                    if (string.Equals(fieldValue, ExperimentId, StringComparison.OrdinalIgnoreCase))
                    {
                        string technique = entity.GetString("Technique") ?? "Not Processed";
                        technique = string.IsNullOrWhiteSpace(technique) ? "Not Processed" : technique.Trim();

                        if (data.TryGetValue(technique, out int value))
                        {
                            data[technique] = ++value;
                        }
                        else
                        {
                            data[technique] = 1;
                        }
                    }
                }

                string outputFile = MLBarChart.GenerateTechniqueVsImagesBarChart(data);

                // Generate a unique blob name using the experiment Id, GUID and outputfile name
                string uniqueBlobName = $"{ExperimentId}-{Guid.NewGuid()}-{outputFile}";

                // Initialize the BlobServiceClient with the storage connection string from the configuration
                BlobServiceClient blobServiceClient = new(_connectionString);

                // Initialize the BlobContainerClient for the specified result container from the configuration
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_config.ResultContainer);

                // Create the container if it does not already exist (asynchronously)
                await containerClient.CreateIfNotExistsAsync();

                // Initialize the BlobClient for the specific blob using the output file's name
                BlobClient blobClient = containerClient.GetBlobClient(uniqueBlobName);

                // Upload the file to the blob storage, overwriting it if it already exists (asynchronously)
                await blobClient.UploadAsync(outputFile, overwrite: true);

                logger?.LogInformation("Successfully uploaded the combined plotted graph into Blob Storage.");

                // Delete file after upload
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                    logger?.LogInformation($"Deleted temp file: {outputFile}");
                }
            }
            catch (Exception ex)
            {
                // Log an error for any other exceptions during uploading Results to blob
                logger?.LogError($"{DateTime.Now} - An error occurred while uploading the combined plotted graph to blob. Exception: {ex.Message}");
            }
        }
    }
}
