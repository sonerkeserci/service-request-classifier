using RequestClassifier.ML.Models;

namespace RequestClassifier.ML.Services;

// Defines the prediction operation that must be implemented by prediction services.
public interface IServiceRequestPredictor
{
    // Receives a service request title and description,
    // sends the combined text to the trained model,
    // and returns the predicted category and highest score.
    ServiceRequestPredictionResult PredictCategory(string? title, string description);
}