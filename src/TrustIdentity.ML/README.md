# TrustIdentity.ML

**Machine Learning integration using ML.NET**

---

## 📦 Overview

`TrustIdentity.ML` provides ML.NET integration for machine learning capabilities in TrustIdentity, powering the AI fraud detection features.

---

## ✨ Features

- ✅ **ML.NET Integration** - Microsoft ML.NET framework
- ✅ **Fraud Detection Models** - Pre-trained models
- ✅ **Anomaly Detection** - Isolation Forest algorithm
- ✅ **Behavioral Analysis** - LSTM models
- ✅ **Model Training** - Train custom models
- ✅ **Model Deployment** - Deploy trained models

---

## 🚀 Installation

```bash
dotnet add package TrustIdentity.ML
dotnet add package Microsoft.ML
```

---

## 🔧 Usage

### Basic Setup

```csharp
using TrustIdentity.ML.Extensions;

builder.Services.AddTrustIdentity(options => { ... })
    .AddMLFraudDetection()
    .AddMLBehaviorAnalysis();
```

### Train Custom Model

```csharp
var mlContext = new MLContext();

// Load training data
var data = mlContext.Data.LoadFromTextFile<LoginAttempt>("training-data.csv", separatorChar: ',');

// Define pipeline
var pipeline = mlContext.Transforms.Concatenate("Features", "IpAddress", "Location", "TimeOfDay")
    .Append(mlContext.BinaryClassification.Trainers.FastTree());

// Train model
var model = pipeline.Fit(data);

// Save model
mlContext.Model.Save(model, data.Schema, "fraud-detection-model.zip");
```

---

## 📄 License

Apache 2.0 - See [LICENSE](../../../LICENSE)
