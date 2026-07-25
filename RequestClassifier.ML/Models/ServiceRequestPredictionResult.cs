namespace RequestClassifier.ML.Models;

// Represents the simplified prediction result returned by the prediction service.
public class ServiceRequestPredictionResult
{
    // Contains the category name predicted by the trained model.
    public string PredictedCategory { get; set; } = string.Empty;

    // Contains the highest score produced among all available categories.
    public float MaxScore { get; set; }
}