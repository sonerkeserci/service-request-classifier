namespace RequestClassifier.ML.Models;

// Represents the simplified prediction result returned to the application layer.
public class ServiceRequestPredictionResult
{
    public string PredictedCategory { get; set; } = string.Empty;

    public float Confidence { get; set; }
}