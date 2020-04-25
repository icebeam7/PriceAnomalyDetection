namespace PriceAnomalyDetection.Models
{
    public class PriceStatus
    {
        public bool IsAnomaly { get; set; }
        public bool IsNegativeAnomaly { get; set; }
        public bool IsPositiveAnomaly { get; set; }
        public int Period { get; set; }
        public float ExpectedValue { get; set; }
        public float LowerMargin { get; set; }
        public float UpperMargin { get; set; }
        public int SuggestedWindow { get; set; }
    }
}
