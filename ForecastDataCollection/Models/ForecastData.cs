namespace ForecastDataCollection.Models
{
    public class ForecastData
    {
        public class ForecastGeneration
        {
            public int Id { get; set; }
            public DateTime GeneratedDate { get; set; }
            public string SourceId { get; set; }
        }

        public class Source
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime InitializedDate { get; set; }
            public DateTime LastRunDate { get; set; }
            public int TotalRuns { get; set; }
        }
        public class Layers
        {

        }
    }
}
