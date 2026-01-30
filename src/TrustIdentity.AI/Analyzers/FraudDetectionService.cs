using TrustIdentity.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TrustIdentity.AI.Analyzers;

/// <summary>
/// Service for detecting fraud and suspicious activities using rule-based analysis and ML hooks
/// </summary>
public class FraudDetectionService : IFraudDetectionService
{
    private readonly ILogger<FraudDetectionService> _logger;
    private readonly TrustIdentity.Abstractions.Stores.IUserStore _userStore;
    private readonly MLContext _mlContext;
    private PredictionEngine<TrustIdentity.ML.FraudDetection.LoginTransaction, TrustIdentity.ML.FraudDetection.FraudPrediction>? _predictionEngine;
    private readonly object _lock = new object();
    private bool _modelLoaded = false;
    private const string ModelPath = "fraud_model.zip";

    /// <summary>
    /// Initializes a new instance of the FraudDetectionService
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="userStore">The user store instance</param>
    public FraudDetectionService(
        ILogger<FraudDetectionService> logger,
        TrustIdentity.Abstractions.Stores.IUserStore userStore)
    {
        _logger = logger;
        _userStore = userStore;
        _mlContext = new MLContext();
        LoadOrTrainModel();
    }

    private void LoadOrTrainModel()
    {
        try
        {
            if (!System.IO.File.Exists(ModelPath))
            {
                _logger.LogWarning("Fraud model not found. Training new model...");
                var trainer = new TrustIdentity.ML.FraudDetection.FraudModelTrainer(ModelPath);
                trainer.TrainAndSaveModel();
            }

            lock (_lock)
            {
                ITransformer model = _mlContext.Model.Load(ModelPath, out var schema);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<TrustIdentity.ML.FraudDetection.LoginTransaction, TrustIdentity.ML.FraudDetection.FraudPrediction>(model);
                _modelLoaded = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load fraud detection model. Falling back to rules.");
        }
    }

    /// <summary>
    /// Analyzes a login attempt for potential fraud
    /// </summary>
    public async Task<double> AnalyzeLoginAttemptAsync(string userId, string ipAddress, string userAgent)
    {
        var user = await _userStore.FindBySubjectIdAsync(userId);
        var failedAttempts = user?.FailedLoginAttempts ?? 0;

        // 1. Feature Engineering (Convert raw inputs to model features)
        // Note: maximizing global compatibility by disabling Geo-restrictions.
        // Future capabilities could implement "Impossible Travel" velocity checks instead.
        var transaction = new TrustIdentity.ML.FraudDetection.LoginTransaction
        {
            RequestTimeHour = DateTime.UtcNow.Hour,
            IsForeignCountry = 0f, 
            PreviousFailureCount = (float)failedAttempts
        };

        if (_modelLoaded)
        {
            lock (_lock)
            {
                var prediction = _predictionEngine!.Predict(transaction);
                _logger.LogInformation("ML Prediction for {UserId}: Probability {Prob}", userId, prediction.Probability);
                return prediction.Probability;
            }
        }
        else
        {
            // Fallback to simple rules
            return await Task.FromResult(0.0);
        }
    }

    /// <summary>
    /// Determines if a behavior pattern represents suspicious activity
    /// </summary>
    /// <param name="pattern">The behavior pattern to analyze</param>
    /// <returns>True if suspicious; otherwise false</returns>
    public async Task<bool> IsSuspiciousActivityAsync(Abstractions.Models.BehaviorPattern pattern)
    {
        _logger.LogInformation("Checking suspicious activity for pattern: {PatternId}", pattern.PatternId);

        // Check if the behavior pattern indicates suspicious activity
        var suspiciousScore = 0.0;

        if (pattern.FailedAttempts > 5)
        {
            suspiciousScore += 0.4;
        }

        if (pattern.LocationChanges > 3)
        {
            suspiciousScore += 0.3;
        }

        if (pattern.DeviceChanges > 2)
        {
            suspiciousScore += 0.3;
        }

        return await Task.FromResult(suspiciousScore >= 0.7);
    }


}

/// <summary>
/// Repesents fraud data for training or analysis
/// </summary>
public class FraudData
{
    /// <summary>The user ID</summary>
    public string UserId { get; set; } = string.Empty;
    /// <summary>The IP address</summary>
    public string IpAddress { get; set; } = string.Empty;
    /// <summary>Whether it's fraud</summary>
    public bool IsFraud { get; set; }
}