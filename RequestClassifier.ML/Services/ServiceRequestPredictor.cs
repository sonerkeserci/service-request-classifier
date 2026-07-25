using Microsoft.Extensions.ML;
using RequestClassifier.ML.Models;

namespace RequestClassifier.ML.Services;

// Uses the trained ML.NET model to predict a category for a service request.
public class ServiceRequestPredictor : IServiceRequestPredictor
{
    // Defines the unique name used to identify the registered model.
    public const string ModelName = "ServiceRequestModel";

    private readonly PredictionEnginePool<ServiceRequestTrainingData, ServiceRequestPrediction> _predictionEnginePool;
    // PredictionEnginePool is a generic class requiring two generic type arguments because prediction transforms one object type into another object type.
    // ServiceRequestTrainingData specifies the model input type.
    // ServiceRequestPrediction specifies the model output type.

    public ServiceRequestPredictor(PredictionEnginePool<ServiceRequestTrainingData, ServiceRequestPrediction> predictionEnginePool)
    {
        // Store the prediction engine pool provided by dependency injection in Program.cs.
        _predictionEnginePool = predictionEnginePool;
    }

    public ServiceRequestPredictionResult PredictCategory(string? title, string description)
    {

        // Combine the title and description because the model was trained using a single Text input column.
        var text = string.Join(" ", new[] { title?.Trim(), description?.Trim() }
                .Where(value => !string.IsNullOrWhiteSpace(value)));

        // Prevent an empty request from being sent to the trained model.
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "The service request text cannot be empty.");
        }

        // Send the prepared text to the registered trained model and receive the raw ML.NET prediction output.
        var prediction = _predictionEnginePool.Predict(ModelName, new ServiceRequestTrainingData { Text = text });

        // Find the highest score produced among all category scores.
        var maxScore = prediction.Score.Length > 0
            ? prediction.Score.Max()
            : 0f;   // float zero

        // Convert the raw ML.NET prediction into a simplified application result.
        return new ServiceRequestPredictionResult
        {
            PredictedCategory = prediction.PredictedCategory,
            MaxScore = maxScore
        };
    }
}