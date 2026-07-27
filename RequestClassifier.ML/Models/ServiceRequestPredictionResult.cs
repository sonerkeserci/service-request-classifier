namespace RequestClassifier.ML.Models;

// Represents the category prediction result produced by the ML service.
public class ServiceRequestPredictionResult
{
    // Contains the category with the highest model score.
    public string PredictedCategory { get; set; } = string.Empty;

    // Contains the highest score produced among all categories.
    public float MaxScore { get; set; }

    // Contains the categories with the highest model scores.
    public List<CategoryPredictionCandidate> TopCandidates { get; set; } = [];
}