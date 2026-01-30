using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML;

namespace TrustIdentity.ML.FraudDetection;

/// <summary>
/// Handles the training and saving of the fraud detection machine learning model.
/// </summary>
public class FraudModelTrainer
{
    private readonly MLContext _mlContext;
    private readonly string _modelPath;

    /// <summary>
    /// Initializes a new instance of the FraudModelTrainer.
    /// </summary>
    /// <param name="modelPath">The path where the trained model should be saved.</param>
    public FraudModelTrainer(string modelPath)
    {
        _mlContext = new MLContext(seed: 0);
        _modelPath = modelPath;
    }

    /// <summary>
    /// Generates synthetic data, trains the model, and saves it to the specified path.
    /// </summary>
    public void TrainAndSaveModel()
    {
        // 1. Create synthetic training data (robust databases would pull this from logs)
        var data = GenerateSyntheticData();
        var trainingData = _mlContext.Data.LoadFromEnumerable(data);

        // 2. Define training pipeline
        var pipeline = _mlContext.Transforms.Concatenate("Features", 
                nameof(LoginTransaction.RequestTimeHour), 
                nameof(LoginTransaction.IsForeignCountry), 
                nameof(LoginTransaction.PreviousFailureCount))
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

        // 3. Train model
        var model = pipeline.Fit(trainingData);

        // 4. Save model
        var directory = Path.GetDirectoryName(_modelPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        _mlContext.Model.Save(model, trainingData.Schema, _modelPath);
    }

    private IEnumerable<LoginTransaction> GenerateSyntheticData()
    {
        var data = new List<LoginTransaction>();
        var rand = new Random(0);

        // Generate 1000 legit transactions (Daytime, home country, low failures)
        for (int i = 0; i < 1000; i++)
        {
            data.Add(new LoginTransaction
            {
                RequestTimeHour = rand.Next(8, 22),         // 8 AM - 10 PM
                IsForeignCountry = 0f,                      // Home
                PreviousFailureCount = rand.Next(0, 2),     // Low failures
                Label = false                               // Legit
            });
        }

        // Generate 100 fraud transactions (Night time OR Foreign OR High failures)
        for (int i = 0; i < 100; i++)
        {
            data.Add(new LoginTransaction
            {
                RequestTimeHour = rand.Next(0, 5),          // 12 AM - 5 AM
                IsForeignCountry = 1f,                      // Foreign
                PreviousFailureCount = rand.Next(3, 10),    // High failures
                Label = true                                // Fraud
            });
        }

        return data;
    }
}
