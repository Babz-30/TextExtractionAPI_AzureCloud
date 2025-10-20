using MyExperiment.Source.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyExperiment.Source.Services
{
    /// <summary>
    /// OpenAI used to calculate text embeddings.
    /// </summary>
    internal class TextEmbedding
    {
        /// <summary>
        /// Call OpenAI API to get text embedding of the input text.
        /// </summary>
        /// <param name="text">Text extracted from processed image.</param>
        /// <returns>List of type double of the received text embeddings.</returns>
        public static async Task<List<double>> ComputeEmbedding(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return [.. new double[1536]];
            }

            var config = Configuration.Config();
            string url = config["API_URL"]; // API URL
            var mediaType = config[key: "MEDIA_TYPE"];

            if (string.IsNullOrEmpty(mediaType))
            {
                throw new KeyNotFoundException("MEDIA_TYPE key is missing or contains an empty value in the configuration.");
            }

            var requestBody = new
            {
                model = config["EMBEDDING_MODEL"],
                input = text,
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", config["authtoken"]);

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, mediaType);

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                if (jsonResponse == null)
                {
                    return [.. new double[1536]];
                }
                var embeddingArray = jsonResponse["data"][0]["embedding"].ToObject<List<double>>();

                return embeddingArray;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching embedding: {ex.Message}");
                return [.. new double[1536]]; // Return a zero-vector in case of failure
            }
        }

        /// <summary>
        /// Generates Dictionary of all the techniques as key and its embeddings as value
        /// </summary>
        /// <param name="ocrTexts"></param>
        /// <returns>Dictionary of technique and it corressponding embeddings</returns>
        public static async Task<Dictionary<string, List<double>>> GetTextEmbeddingsAsync(Dictionary<string, string> ocrTexts)
        {
            Dictionary<string, List<double>> textEmbeddings = [];
            foreach (var item in ocrTexts)
            {
                textEmbeddings[item.Key] = await ComputeEmbedding(item.Value);
            }
            return textEmbeddings;
        }
    }
}
