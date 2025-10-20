namespace MyExperiment.Source.Helpers
{
    /// <summary>
    /// Capture final result of the ML experiment
    /// </summary>
    public class BestTechnique
    {
        public string Technique { get; set; }
        public double Mean { get; set; }
        public double DictionaryAccuracy { get; set; }
        public double MeanConfidence { get; set; }
        public string Output { get; set; }
        public string FileName { get; set; }
    }
}
