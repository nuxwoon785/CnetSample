namespace CNetTest.Vision
{
    public class PredictionResult
    {
        public RoiDefinition Roi { get; set; }
        public float OkScore { get; set; }
        public float NgScore { get; set; }

        public string Label => OkScore >= NgScore ? "OK" : "NG";
    }
}
