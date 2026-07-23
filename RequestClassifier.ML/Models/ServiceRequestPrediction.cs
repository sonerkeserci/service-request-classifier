using Microsoft.ML.Data;

namespace RequestClassifier.ML.Models
{
    public class ServiceRequestPrediction       // Represents the category and scores predicted by the trained model.
    {
        [ColumnName("PredictedLabel")]
        public string PredictedCategory { get; set; } = string.Empty;
        public float[] Score { get; set; } = [];
    }
}
