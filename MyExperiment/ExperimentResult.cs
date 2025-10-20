using Azure;
using Azure.Data.Tables;
using MyCloudProject.Common;
using MyExperiment.Source.Helpers;
using System;

namespace MyExperiment
{
    /// <summary>
    /// Final result fields that is stored in Table
    /// </summary>
    /// <param name="partitionKey">Unique for the table dataset</param>
    /// <param name="rowKey">Unique for each row in the table</param>
    public class ExperimentResult(string partitionKey, string rowKey) : ITableEntity, IExperimentResult
    {
        public string PartitionKey { get; set; } = partitionKey;

        public string RowKey { get; set; } = rowKey;

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public string ExperimentId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime? StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        public long DurationSec { get; set; }

        public string InputFileUrl { get; set; }

        public string[] OutputFiles { get; set; }

        public BestTechnique BestTechniqueResult { get; set; }

        public float Accuracy { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
