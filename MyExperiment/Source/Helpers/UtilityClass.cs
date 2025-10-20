using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyExperiment.Source.Helpers
{

    /// <summary>
    /// Utility class providing helper methods for directory and file management within the OCR application.
    /// </summary>
    public class UtilityClass
    {
        // Path to input image directory
        static readonly string InputImageFolder = "Input_Images";

        //Path to output directory
        static readonly string OutputFolder = "Output";

        //Path to input directory
        static readonly string InputFolder = "Input";

        // Path to preprocessed image directory
        static readonly string OutputImageFolder = "Preprocessed_Image_Output";

        // Path to the trained tessdata folder
        static readonly string TrainedDataDirectory = "Trained_Data_Input";

        // Path to extracted text after preprocessing
        static readonly string TesseractOutputFolder = "Tesseract_Output";

        static readonly string CosineSimilarityFolder = "Cosine_Similarity_Output";

        /// <summary>
        /// Retrieves the root directory of the solution. 
        /// </summary>
        /// <returns>string</returns>
        public static string SolutionDirectory()
        {
            return AppContext.BaseDirectory; 

        }

        /// <summary>
        /// Local folder Input path
        /// </summary>
        /// <returns>Returns Input folder path</returns>
        public static string InputDirectory()
        {
            string directoryPath = Path.Combine(SolutionDirectory(), InputFolder);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        /// <summary>
        /// Retrieves or creates the input image directory. 
        /// </summary>
        /// <returns>string</returns>
        public static string InputImageDirectory()
        {
            string directoryPath = Path.Combine(InputDirectory(), InputImageFolder);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        /// <summary>
        /// Local folder Output path
        /// </summary>
        /// <returns>Returns Output folder with processed data path</returns>
        public static string OutputDirectory()
        {
            string directoryPath = Path.Combine(SolutionDirectory(), OutputFolder);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        /// <summary>
        /// Retrieves or creates the output image directory. 
        /// </summary>
        /// <returns>string</returns>
        public static string OutputImageDirectory()
        {
            string directoryPath = Path.Combine(OutputDirectory(), OutputImageFolder);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        /// <summary>
        /// Retrieves or creates the Tesseract OCR output directory 
        /// </summary>
        /// <returns>string</returns>
        public static string TesseractOutputDirectory()
        {
            string directoryPath = Path.Combine(OutputDirectory(), TesseractOutputFolder);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        public static string CosineSimilarityDirectory()
        {
            string directoryPath = Path.Combine(OutputDirectory(), CosineSimilarityFolder);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        /// <summary>
        /// Constructs the full path of an input image.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>string</returns>
        public static string InputImagePath(string filename)
        {
            string imagePath = Path.Combine(InputImageDirectory(), filename);

            return imagePath;
        }

        // Constructs and ensures the existence of an output image file.
        public static string OutputImagePath(string filename)
        {
            string imagePath = Path.Combine(InputImageDirectory(), filename);

            if (!File.Exists(imagePath))
            {
                File.Create(imagePath);
            }

            return imagePath;
        }

        // Retrieves the full path for storing Tesseract OCR output files.
        public static string TesseractOutputPath(string filename)
        {

            string textFilePath = Path.Combine(TesseractOutputDirectory(), filename);

            return textFilePath;
        }

        // Retrieves the trained data path for OCR processing.
        public static string TrainedDataPath()
        {
            string directoryPath = Path.Combine(InputDirectory(), TrainedDataDirectory);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        public static string DictionaryPath()
        {
            return Path.Combine(TrainedDataPath(), "dictionary.txt");
        }

        // Constructs the path for storing cosine similarity results.
        public static string CosineSimilarityOutputPath(string filename)
        {
            string filePath = Path.Combine(CosineSimilarityDirectory(), filename);
            return filePath;
        }

        // Saves content to a specified file.
        public static void SaveToFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        // Deletes all files from specified output directories.
        public static void DeleteAllFiles()
        {
            // List of folders to empty (change these paths)
            string[] folders = [
                TesseractOutputDirectory(),
                OutputImageDirectory(),
                CosineSimilarityDirectory(),
                InputImageDirectory()
            ];

            try
            {
                foreach (string folderPath in folders)
                {

                    // Check if the folder exists
                    if (Directory.Exists(folderPath))
                    {
                        // Delete all files in the folder
                        foreach (string file in Directory.GetFiles(folderPath))
                        {
                            File.Delete(file);
                        }

                        Console.WriteLine("Folder emptied successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to delete files in Folder: {ex.Message}");
            }
        }

        // Helper function to safely get configuration arrays
        public static T[] GetConfigurationArray<T>(IConfiguration config, string key)
        {
            var array = config.GetSection(key).Get<T[]>();
            return array ?? [];  // Return an empty array if the section is missing or null
        }

        public static List<T> GetConfigurationList<T>(IConfiguration config, string key)
        {
            var array = config.GetSection(key).Get<List<T>>();
            return array ?? [];  // Return an empty List if the section is missing or null
        }

        public static string GetRequiredConfigValue(IConfiguration config, string key)
        {
            string value = config[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new KeyNotFoundException($"'{key}' key is missing or contains an empty value in the configuration.");
            }

            return value;
        }
    }
}
