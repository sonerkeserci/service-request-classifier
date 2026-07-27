namespace RequestClassifier.ML.Models;

// Represents one category candidate produced by the trained ML model.
public class CategoryPredictionCandidate
{
    // Contains the category name associated with the model score.
    public string CategoryName { get; set; } = string.Empty;

    // Contains the score produced by the model for this category.
    public float Score { get; set; }
}