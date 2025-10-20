using MyCloudProject.Common;

namespace MyExperiment
{
    /// <summary>
    /// Request message json inserted in the Queue
    /// </summary>
    public class ExerimentRequestMessage : IExerimentRequest
    {
        /// <summary>
        /// Unique Experiment Id set by user
        /// </summary>
        public string ExperimentId { get; set; }
        /// <summary>
        /// Name of file/folder in blob storage to be processed
        /// </summary>
        public string InputFile { get; set; }
        /// <summary>
        /// Name of Experiment
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description for the experiment
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Unique message Id for Queue processing 
        /// </summary>
        public string MessageId { get; set; }
        /// <summary>
        /// Unique MessageReceipt for Queue processing 
        /// </summary>
        public string MessageReceipt { get; set; }
    }
}
