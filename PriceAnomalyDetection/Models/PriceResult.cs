namespace PriceAnomalyDetection.Models
{
    public class PriceResult
    {
        public float[] ExpectedValues { get; set; }
        public bool[] IsAnomaly { get; set; }
        public bool[] IsNegativeAnomaly { get; set; }
        public bool[] IsPositiveAnomaly { get; set; }
        public float[] LowerMargins { get; set; }
        public int Period { get; set; }
        public float[] UpperMargins { get; set; }
    }
}
