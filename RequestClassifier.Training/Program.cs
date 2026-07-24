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
Console.WriteLine($"Micro Accuracy: {metrics.MicroAccuracy:P2}");
Console.WriteLine($"Macro Accuracy: {metrics.MacroAccuracy:P2}");
Console.WriteLine($"Log Loss: {metrics.LogLoss:F4}");

Console.WriteLine();
Console.WriteLine($"Model saved to: {modelPath}");