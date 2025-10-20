
# Project Implementation  

This section explains how the OCR pipeline is built, containerized, and integrated with Azure services to enable automated and scalable text extraction.  


### 1. Starting and Stopping the Azure Container Instance  

Use the Azure Portal or CLI to start the `techrookiescontainerinstance` before execution. Once processing is complete and results are retrieved, stop the container to save costs and free resources.  

<p align="center">
  <img src="https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2024-2025/blob/TechRookies/Source/MyCloudProjectSample/Documentation/Images/Monitoring.png">
  <br>
  <em>Figure 1: <i>Managing the Azure Container Instance (Start/Stop)</i></em>
</p>

 ### 2.  User Uploads Setup and Training Files**  

Instead of uploading a single image, the user uploads the setup files (e.g., dictionary.txt, eng.traineddata) and training files (datasets or input files required for the experiment) into Azure Blob Storage within the project’s Azure Storage Account.  

This upload acts as the triggering event for the pipeline. Once these files are detected in Blob Storage, the Azure system notifies the Azure Queue, which in turn signals the Azure Container Instance (ACI) to begin execution.  

By structuring the workflow this way, all required resources (setup + training) are provided by the user upfront in Blob Storage, ensuring the containerized application has everything it needs to run the experiment automatically.  

<p align="center">
  <img src="https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2024-2025/blob/TechRookies/Source/MyCloudProjectSample/Documentation/Images/setup-files.png"
  <em> Figure 2 : <i>setup-files </i></em>
</p>

<p align="center">
  <img src="https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2024-2025/blob/TechRookies/Source/MyCloudProjectSample/Documentation/Images/training-files.png"
  <em> Figure 3 : <i>Training-files </i></em>
</p>

### 3. Initializing the Project with Queue Message  

Upload the input image and any setup/training files to Azure Blob Storage. The process is triggered via an Azure Queue message containing file locations and other metadata. Messages can be sent using Azure Portal or Storage Explorer. On receiving a message, the ACI pulls the Docker image from Azure Container Registry, retrieves files from Blob Storage, and runs OCR with preprocessing.  
<p align="center">
  <img src="https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2024-2025/blob/TechRookies/Source/MyCloudProjectSample/Documentation/Images/trigger-queue.png"
  <em> Figure 4 : <i>Initializing the project with trigger-queue Message </i></em>
</p>

**Experiment Input Parameters**  

Provided in JSON format via the queue:  

```json
{
  "ExperimentId": "Exp-41",
  "InputFile": "image6.png",
  "Name": "Dataset",
  "Description": "Running OCR Test on Dataset for image6.png"
}
```
```json
{
  "ExperimentId": "Exp-33",
  "InputFile": "Input_Images/",
  "Name": "Dataset",
  "Description": "Running OCR Test on Dataset"
}
```

#### Below is a description of each input parameter:

| **Parameter**   | **Description** |
|-----------------|--------------------------------------------------------------------------------------------------------------------------------|
| **ExperimentId** | A unique identifier for the OCR experiment. This is used to link the input data, processing workflow, and results together. |
| **InputFile**    | The file name or path of the image(s) in Azure Blob Storage to be processed. Can be a single file or a directory (e.g., `Input_Images/`). |
| **Name**         | A label or category name for the dataset being processed. Helps in organizing and identifying experiments. |
| **Description**  | A short explanation of the experiment, including context about the input file(s) or purpose of the run. |


## Experiment Output Parameters

The Experiment Output properties are defined in ExperimentResult.cs as shown below:
<p align="center">
  <img src="https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2024-2025/blob/TechRookies/Source/MyCloudProjectSample/Documentation/Images/results.png"
  <em> Figure 5 : <i>results </i></em>
</p>


#### Columns of the Table

Each key-value pair added to the TableEntity corresponds to a column in the Azure Table Storage. Here's a breakdown of the columns:

#### Below is a description of each output parameter:  

| **Parameter**       | **Description** |
|---------------------|------------------------------------------------------------------------------------------------------------------------------------|
| **PartitionKey**    | The primary key used in Azure Table Storage to logically group related records for efficient queries. |
| **RowKey**          | The unique identifier for a specific record within a PartitionKey in Azure Table Storage. |
| **Timestamp**       | The date and time (in UTC) when the record was last modified in Azure Table Storage. |
| **ETag**            | The entity tag used for concurrency control in Azure Table Storage, helping to prevent conflicts during updates. |
| **ExperimentId**    | The unique identifier for the experiment, matching the input parameter to link results to its corresponding run. |
| **Name**            | The name of the experiment, carried forward from the input metadata. |
| **Description**     | A brief description of the experiment or the dataset processed. |
| **StartTimeUtc**    | The date and time (in UTC) when the experiment began execution. |
| **EndTimeUtc**      | The date and time (in UTC) when the experiment finished execution. |
| **DurationSec**     | The total execution time of the experiment in seconds. |
| **InputFileUrl**    | The URL in Azure Blob Storage where the input image or dataset used in the experiment is stored. |
| **OutputFiles**     | An array of file paths or URLs pointing to the generated output files (e.g., processed images, extracted text files) stored in Blob Storage. |
| **BestTechniqueResult** | The preprocessing method that achieved the highest accuracy score during OCR evaluation. |
| **Accuracy**        | The final accuracy score achieved by the best preprocessing technique, expressed as a percentage or float value. |
| **Duration**        | The total execution time represented as a TimeSpan object for more detailed time analysis. |

These output parameters provide a complete record of the experiment’s execution and results. They are essential for tracking performance, linking results to specific runs, and enabling efficient storage, retrieval, and further analysis within Azure Table Storage.

For visual reference, table can typically viewed in the Azure Storage Explorer or the Azure portal. The structure includes the PartitionKey, RowKey, Timestamp and all input parameters (prefixed with "Ip_") as well as output parameters (prefixed with "Op_").

| PartitionKey   | RowKey                               | Timestamp                    | ExperimentId   | InputImage                         | Name    | Description                                | StartTimeUtc                | EndTimeUtc                   | DurationInSec | Technique | ExtractedText | CosineSimilarityMean | DictionaryAccuracy | MeanConfidence |
|:---------------|:-------------------------------------|:-----------------------------|:---------------|:-----------------------------------|:--------|:-------------------------------------------|:----------------------------|:-----------------------------|--------------:|----------:|--------------:|---------------------:|-------------------:|---------------:|
| TechRookies    | 42154021-82c7-49e2-b42a-13fc0244ce85 | 2025-08-13T06:27:09.8083794Z | Exp-40         | /app/Input/Input_Images/image6.png | Dataset | Running OCR Test on Dataset for image6.png | 2025-08-13T06:26:02.642108Z | 2025-08-13T06:27:09.6022094Z |            66 |       nan |           nan |                    0 |                  0 |              0 |



### 4. Image Processing and Text Extraction

The input files are uploaded to Azure, the containerized application is triggered to start processing. Inside the container, the workflow follows the same logic as our Software Engineering project: the image is first enhanced using preprocessing techniques (e.g., rotation, resizing, grayscale, binarization, HSI adjustments, or mirroring), after which Tesseract OCR extracts the text along with confidence scores. The extracted text is then converted into OpenAI embeddings, which are compared using cosine similarity, combined with dictionary accuracy and mean confidence, to evaluate the quality of recognition. This ensures that the cloud pipeline not only automates the experiment execution but also maintains the same accuracy-driven approach used in our original project.

Azure Container Instance (ACI) logs capture key events such as message receipt, file retrieval, preprocessing, OCR execution, and result storage. These logs can be reviewed in the Azure Portal to track progress, troubleshoot issues, and verify successful processing.  

<p align="center">
  <img src="https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2024-2025/blob/TechRookies/Source/MyCloudProjectSample/Documentation/Images/Logs.png">
  <br>
  <em>Figure 6: <i>ACI Execution Logs</i></em>
</p>  

### 5. Processed Results Stored in Table  
 
After execution, results are available in the result-files Blob container and in the results Table Storage. These can be accessed via Azure Portal or Storage Explorer for analysis or integration.
  
<p align="center">
  <img src="https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2024-2025/blob/TechRookies/Source/MyCloudProjectSample/Documentation/Images/BlobStorage.png">
  <br>
  <em>Figure 7: <i>Output files in Blob Storage</i></em>
</p>  









