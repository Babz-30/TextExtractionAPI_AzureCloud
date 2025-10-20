# Azure Cloud Implementation : ML 24/25-10 Extracting text from images using tesseract-ocr api_TechRookies
<a id= "top"></a>

## Table of contents
1. [Introduction](#introduction)
2. [Project Architecture](#project-architecture)
3. [Project Implementation](/Documentation/Document-md/Project%20Implementation.md)
4. [Project Execution](/Documentation/Document-md/Project%20%20Execution.md)
5. [Results](/Documentation/Document-md/Result.md)
6. [Conclusion](#conclusion)
7. [References](#references)

## Introduction 

This OCR-based text extraction project focuses on enhancing the accuracy of Optical Character Recognition by applying and evaluating a variety of image preprocessing techniques. The system addresses common challenges such as noise, distortion, and poor image quality, which often reduce OCR effectiveness. Techniques like grayscale conversion, binarization, histogram equalization, and rotation are applied to optimize text visibility. OpenAI embeddings are then used to compare the extracted outputs using cosine similarity, dictionary-based accuracy, and confidence scores to determine the most effective preprocessing method.

The project has been redesigned for scalable deployment in the cloud using Microsoft Azure. A Docker image of the application is built and pushed to Azure Container Registry, then deployed using Azure Container Instances. Image uploads are stored in Azure Blob Storage, with message-based processing triggered via Azure Queue Storage. Final results and metadata are stored in Azure Table Storage, enabling efficient retrieval and analysis. This architecture ensures a streamlined, automated, and high-accuracy OCR workflow.

## Project Architecture 

In this section, Figure 1 illustrates the project's architectural diagram, showcasing the integration of various components that constitute the project's architecture and workflow. This diagram highlights the interactions and interdependencies among the different modules, providing a detailed overview of the entire project structure.
<p align="center">
  <img src="/Documentation/Images/Architecture.png">
  <br>
  <em>Figure 1: <i>Project Architecture - Azure Cloud Implementation: ML24/25-10 Extracting text from images using tesseract-ocr api Experiment</i></em>
</p>

### Architecture Steps Explanation

- **Step 1**: Developer Builds and Pushes Docker Image
Developer writes and tests the application code in Visual Studio.
Source code versioning managed via GitHub.
Application is containerized into a Docker image.
Docker image pushed to Azure Container Registry (ACR).

- **Step 2**: User Uploads Image with Text
User uploads an image containing text to Azure Blob Storage within an Azure Storage Account.

- **Step 3**: Message Sent to Queue
User uploads a message to Azure Queue with metadata or a reference to the image stored in Blob Storage.
The message acts as a trigger for the next processing step.

- **Step 4**: Message Triggers Azure Container Instance
Azure Container Instance (ACI) pulls the Docker image from Azure Container Registry and, based on the queue message, retrieves the uploaded image along with required setup/training files from Blob Storage for processing, then performs tasks such as text extraction or data analysis.

- **Step 5**: Processed Results Stored in Table
Results from the containerized processing (extracted text, status, metadata) are stored in Azure Table Storage for further use or analysis.


### Features

- Receives OCR processing requests from an Azure Storage Queue, triggered by user-uploaded messages containing file references and metadata.  
- Retrieves input images and required setup/training files from Azure Blob Storage for preprocessing and text extraction.  
- Executes OCR within an Azure Container Instance using container images stored in Azure Container Registry.  
- Stores extracted text and metadata in Azure Table Storage, and saves processed output files to Azure Blob Storage for easy access and analysis.  
- Maintains detailed execution logs for monitoring, debugging, and performance tracking via Azure Container Instance logs.  

## Tools and Technologies Used

| Tools/Technology                | Description                                                                                                                                                                      |
|---------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Visual Studio**         | A comprehensive Integrated Development Environment (IDE) from Microsoft, utilized for developing and debugging applications across multiple platforms, including Windows, macOS, and Linux. |
| **GitHub**                | A web-based platform offering version control and collaborative development, utilizing Git for managing source code across various programming languages and facilitating team collaboration. |
| **Docker**                | A containerization platform that streamlines the deployment, scaling, and management of applications by encapsulating them within lightweight, portable containers, ideal for cloud environments like Azure. |
| **Docker Image**          | A self-contained, lightweight snapshot of an application and its dependencies, designed to be deployed in Docker containers, ensuring consistency and portability across different environments. |
| **Azure Container Instances** | A serverless compute service allowing the execution of Docker containers in the Azure cloud without the need to manage underlying virtual machines, simplifying container deployment and scaling. |
| **Azure Container Registry** | A managed container registry service in Azure for storing, managing, and securing Docker container images, facilitating streamlined deployment and version control. |
| **Azure Storage**         | A cloud-based storage solution by Microsoft providing scalable and secure storage options, including blob, file, queue, and table storage, to handle diverse data storage needs. |
| **Azure Blob Storage**    | An Azure service designed for storing vast amounts of unstructured data, such as documents and media files. In this project, it is used to manage input and output data containers for storing experiment data and results. |
| **Azure Queue**           | A messaging service that facilitates asynchronous communication by queuing messages, which trigger application processes or workflows, ensuring efficient task management and execution. |

[Move to Top](#top)

### Benefits of Cloud Integration

- **Scalability** – Can process thousands of images (e.g., Kaggle dataset) instead of being limited to a few hundred locally.

- **Automation** – Uploading files to Blob Storage automatically triggers preprocessing, OCR, and result storage with no manual steps.

- **Parallel Processing** – Multiple containers can run in parallel, allowing faster handling of large datasets.

- **Centralized Results** – Outputs and metrics are stored in Table Storage, making querying, monitoring, and analysis much easier.

- **Reliability** – Azure services handle retries, logging, and resource scaling, reducing risk of failure in large-scale runs.

- **Reusability** – The same pipeline can be reused for different datasets (bills, IDs, license plates) without changing the core logic.

## Conclusion

The project demonstrates how an OCR workflow can be transformed from a locally developed prototype into a fully automated, cloud-deployed pipeline. By leveraging Azure’s containerized architecture and storage services, the solution achieves real-time processing, scalability, and cost efficiency. The integration of preprocessing techniques with accuracy evaluation ensures high-quality text extraction, even from challenging images.

With its modular design, the system can be easily extended to support additional preprocessing methods, multi-language OCR, or integration with downstream analytics and visualization platforms. This makes it adaptable for diverse use cases ranging from document digitization to large-scale automated data extraction.

## References

[SE project - Creating Text from Images with OCR API ](https://github.com/Babz-30/TextExtractionAPI_Team_TechRookies)<br/>
[Microsoft Azure Documentation](https://learn.microsoft.com/en-us/azure/?product=popular)<br/>
[SE Project Documentation](https://github.com/Babz-30/TextExtractionAPI_Team_TechRookies/tree/main/Documentation)<br/>
[Kaggel Data Set ](https://www.kaggle.com/datasets/robikscube/textocr-text-extraction-from-images-dataset)









