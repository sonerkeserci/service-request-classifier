namespace RequestClassifier.ML.Models;

public class ServiceRequesTrainingData      // Represents the input text and its correct category used to train the model.
{
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}