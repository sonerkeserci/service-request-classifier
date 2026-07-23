using Microsoft.ML;
using Microsoft.ML.Data;
using RequestClassifier.ML.Models;

namespace RequestClassifier.ML.Services;

public class ServiceRequestModelTrainer
{
    private readonly MLContext _mlContext;

    public ServiceRequestModelTrainer()
    {
        // Create the main ML.NET environment with a fixed seed for repeatable results.
        _mlContext = new MLContext(seed: 42);
    }

    public MulticlassClassificationMetrics Train(string dataPath, string modelPath)
    {
        // Load the labeled service request data from a tab-separated file.
        var data = _mlContext.Data
            .LoadFromTextFile<ServiceRequestTrainingData>(
                path: dataPath,
                hasHeader: true,    // Prevent the header row from being read as training data.
                separatorChar: '\t',
                allowQuoting: true,
                trimWhitespace: true);

        // Use 80% of the data for training and 20% for testing.
        var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.20, seed: 42);

        // Convert category names into numeric labels.
        var pipeline = _mlContext.Transforms.Conversion
            .MapValueToKey(
                outputColumnName: "Label",
                inputColumnName: nameof(ServiceRequestTrainingData.Category))

            // Convert the request text into numeric features.
            .Append(
                _mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "Features",
                    inputColumnName: nameof(ServiceRequestTrainingData.Text)))

            // Train a multiclass classification model.
            .Append(
                _mlContext.MulticlassClassification.Trainers
                    .SdcaMaximumEntropy(
                        labelColumnName: "Label",           // Column including correct answer
                        featureColumnName: "Features"));    // Column incluing featurizedtext   

        // Train the model using the training data.
        var model = pipeline.Fit(split.TrainSet); // Pipeline is the recipe. Fit applies the recipe to the data and creates the real model.

        // Generate predictions for the test data.
        var predictions = model.Transform(split.TestSet); // Transform applies the trained model to test data. Returns 3 result(Label,Score,PredictedLabel).

        // Measure the performance of the trained model.
        var metrics = _mlContext.MulticlassClassification.Evaluate(
            predictions,
            labelColumnName: "Label",
            scoreColumnName: "Score",
            predictedLabelColumnName: "PredictedLabel");

        var modelDirectory = Path.GetDirectoryName(modelPath);

        if (!string.IsNullOrWhiteSpace(modelDirectory))
        {
            Directory.CreateDirectory(modelDirectory);
        }

        // Define the transformation that converts the predicted numeric key
        // back to the original category name.
        var keyToValueEstimator =
            _mlContext.Transforms.Conversion.MapKeyToValue(
                outputColumnName: "PredictedLabel",
                inputColumnName: "PredictedLabel");

        // Fit the transformation to the prediction schema.
        var keyToValueTransformer = keyToValueEstimator.Fit(predictions);

        // Append the fitted transformation to the trained model.
        var finalModel = model.Append(keyToValueTransformer);

        // Save the final model so it can be loaded by the API later.
        _mlContext.Model.Save(
            finalModel,
            split.TrainSet.Schema,
            modelPath);

        return metrics;
    }
}