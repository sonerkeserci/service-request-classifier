namespace RequestClassifier.ML.Models;

// Contains metadata extracted from the trained ML.NET model.
public class ServiceRequestModelMetadata
{
    // Contains category names in the same order as the model's Score vector.
    public IReadOnlyList<string> CategoryNames { get; init; } = [];
}