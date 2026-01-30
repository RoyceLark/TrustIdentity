using Microsoft.ML.Data;

namespace TrustIdentity.ML.FraudDetection;

/// <summary>
/// Data model representing a login transaction for ML analysis
/// </summary>
public class LoginTransaction
{
    /// <summary>
    /// The hour of the request (0-23)
    /// </summary>
    [LoadColumn(0)]
    public float RequestTimeHour { get; set; }

    /// <summary>
    /// Whether the request is from a foreign country (0 = No, 1 = Yes)
    /// </summary>
    [LoadColumn(1)]
    public float IsForeignCountry { get; set; }

    /// <summary>
    /// Number of previous failed attempts
    /// </summary>
    [LoadColumn(2)]
    public float PreviousFailureCount { get; set; }

    /// <summary>
    /// The label for training (True = Fraud, False = Legit)
    /// </summary>
    [LoadColumn(3)]
    public bool Label { get; set; }
}

/// <summary>
/// The prediction output from the fraud model
/// </summary>
public class FraudPrediction
{
    /// <summary>
    /// The predicted label (True/False)
    /// </summary>
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    /// <summary>
    /// The probability/confidence of the prediction (0.0 - 1.0)
    /// </summary>
    public float Probability { get; set; }

    /// <summary>
    /// The raw score
    /// </summary>
    public float Score { get; set; }
}
