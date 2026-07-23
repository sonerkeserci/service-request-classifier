using Microsoft.ML.Data;

namespace RequestClassifier.ML.Models;

public class ServiceRequesTrainingData      // Represents the input text and its correct category used to train the model.
{
    [LoadColumn(0)]
    public string Text { get; set; } = string.Empty;
    [LoadColumn(1)]
    public string Category { get; set; } = string.Empty;
}