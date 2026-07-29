namespace RequestClassifier.Application.Settings;

// Contains configuration values used by the automatic assignment logic.
public class MachineLearningSettings
{
    // Minimum highest score required for automatic assignment.
    public float AutoAssignmentScoreThreshold { get; set; }

    // Minimum difference required between the first and second scores.
    public float AutoAssignmentMarginThreshold { get; set; }
}