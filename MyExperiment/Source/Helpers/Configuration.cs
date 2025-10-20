using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyExperiment.Source.Helpers
{
    /// <summary>
    /// Provides application configuration settings from JSON and environment variables.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Loads configuration settings from "appsettings.json" and environment variables.
        /// </summary>
        /// <returns>An IConfigurationRoot object containing application settings.</returns>
        public static IConfigurationRoot Config()
        {

            //Configuration to read from json file, and environment variables
            var builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            return configuration;
        }
    }
}
