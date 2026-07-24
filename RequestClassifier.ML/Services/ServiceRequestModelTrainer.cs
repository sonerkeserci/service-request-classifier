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
        var split = _mlContext.Data.TrainTestSplit(
            data, 
            testFraction: 0.20, 
            seed: 42);

        // Count the rows in the training and test datasets.
        var trainRowCount = _mlContext.Data
            .CreateEnumerable<ServiceRequestTrainingData>(
                split.TrainSet,
                reuseRowObject: false)
            .Count();

        var testRowCount = _mlContext.Data
            .CreateEnumerable<ServiceRequestTrainingData>(
                split.TestSet,
                reuseRowObject: false)
            .Count();

        Console.WriteLine($"Training rows: {trainRowCount}");
        Console.WriteLine($"Test rows: {testRowCount}");

        // Define the training pipeline.
        // This block only describes the sequence of transformations and the trainer.
        // No training happens until Fit is called.
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
        var model = pipeline.
            Fit(split.TrainSet); // "Pipeline" is the recipe. "Fit" method applies the recipe to the data and creates the real model.

        // Apply the trained model to the test data. Returns a IDataView
        // The result keeps the existing columns and adds prediction columns such as Score and PredictedLabel. 
        var predictions = model.
            Transform(split.TestSet);

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

        // Define the key-to-value conversion.
        // "Estimator" only describes how PredictedLabel will be converted
        // from a numeric key back to the original category name.
        var keyToValueEstimator =
            _mlContext.Transforms.Conversion.MapKeyToValue(
                outputColumnName: "PredictedLabel",
                inputColumnName: "PredictedLabel");

        // Fit the estimator to the prediction schema and create a usable transformer.
        // This does not retrain the classification model.
        var keyToValueTransformer =
            keyToValueEstimator.
            Fit(predictions);

        // Append: Add the fitted key-to-value transformer to the trained classification model.
        var finalModel = model.
            Append(keyToValueTransformer);

        // Save the final model so it can be loaded by the API later.
        _mlContext.Model.Save(
            finalModel,
            split.TrainSet.Schema,
            modelPath);

        return metrics;
    }
}