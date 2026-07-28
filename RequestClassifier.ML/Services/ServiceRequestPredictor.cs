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

    private readonly ServiceRequestModelMetadata _modelMetadata;

    public ServiceRequestPredictor(PredictionEnginePool<ServiceRequestTrainingData, ServiceRequestPrediction> predictionEnginePool, ServiceRequestModelMetadata modelMetadata)
    {
        // Store the prediction engine pool provided by dependency injection in Program.cs.
        _predictionEnginePool = predictionEnginePool;
        _modelMetadata = modelMetadata;
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

        // Ensure that every model score has a matching category name.
        // A mismatch could cause scores to be displayed under incorrect categories.
        if (prediction.Score.Length != _modelMetadata.CategoryNames.Count)
        {
            throw new InvalidOperationException(
                $"The model returned {prediction.Score.Length} scores, " +
                $"but {_modelMetadata.CategoryNames.Count} category names were found.");
        }

        // Match every score with the category name stored at the same index,
        // sort the candidates from highest score to lowest score,
        // and keep only the five strongest category suggestions.
        var topCandidates = prediction.Score
            .Select(
                (score, index) => new CategoryPredictionCandidate
                {
                    CategoryName = _modelMetadata.CategoryNames[index],
                    Score = score
                })
            .OrderByDescending(candidate => candidate.Score)
            .Take(5)
            .ToList();

        // Use the first sorted candidate as the maximum model score.
        var maxScore = topCandidates.Count > 0
            ? topCandidates[0].Score
            : 0f;

        var secondScore = topCandidates.Count > 1
            ? topCandidates[1].Score
            : 0f;

        var scoreMargin = maxScore- secondScore;

        return new ServiceRequestPredictionResult
        {
            PredictedCategory = prediction.PredictedCategory,
            MaxScore = maxScore,
            ScoreMargin = scoreMargin,
            TopCandidates = topCandidates
        };
    }
}