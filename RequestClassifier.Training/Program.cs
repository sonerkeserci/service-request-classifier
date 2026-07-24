using RequestClassifier.ML.Services;

var dataPath = Path.Combine(
    AppContext.BaseDirectory,
    "Data",
    "service-requests.tsv");

var modelPath = Path.Combine(
    AppContext.BaseDirectory,
    "TrainedModels",
    "service-request-model.zip");

// Create the trainer and start the training process.
var trainer = new ServiceRequestModelTrainer();

var metrics = trainer.Train(
    dataPath,
    modelPath);

// Display the main evaluation metrics.

// MicroAccuracy measures the overall percentage of correct predictions.
// It gives more influence to categories that contain more test samples.
// Higher values are better, and 1.00 means 100% accuracy.
Console.WriteLine($"Micro Accuracy: {metrics.MicroAccuracy:P2}");

// MacroAccuracy calculates the average accuracy across all categories.
// Each category has equal importance regardless of its number of samples.
// Higher values are better, and 1.00 means 100% accuracy.
Console.WriteLine($"Macro Accuracy: {metrics.MacroAccuracy:P2}");

// LogLoss measures how incorrect and uncertain the model's predictions are.
// It gives a larger penalty to confident but incorrect predictions.
// Lower values are better, and 0 represents the ideal result.
Console.WriteLine($"Log Loss: {metrics.LogLoss:F4}");

Console.WriteLine();
Console.WriteLine($"Model saved to: {modelPath}");