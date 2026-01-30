# TrustIdentity Testing Guide

This guide covers how to run the various tests available in the TrustIdentity solution to ensure system stability and performance.

## 🧪 Test Suites

### 1. Unit Tests (`TrustIdentity.UnitTests`)
Focuses on individual components like logic services, validators, and models.
- **Coverage:** Core logic, Crypto helpers, Validation rules.
- **Command:**
  ```bash
  dotnet test tests/TrustIdentity.UnitTests
  ```

### 2. Integration Tests (`TrustIdentity.IntegrationTests`)
Tests the entire pipeline using `TestServer`. Verified actual HTTP requests and responses against a running (in-memory) instance of TrustIdentity.
- **Coverage:** Discovery Endpoint, Token Issuance, Flow validation.
- **Command:**
  ```bash
  dotnet test tests/TrustIdentity.IntegrationTests
  ```

## 🏎️ Performance Benchmarks

### Benchmark Project (`TrustIdentity.Benchmarks`)
We use [BenchmarkDotNet](https://benchmarkdotnet.org/) to measure the performance of cryptographic operations and hot-paths (like token signing).

- **Location:** `src/TrustIdentity.Benchmarks`
- **Run Command:**
  **NOTE:** Benchmarks must be run in `Release` configuration.
  ```bash
  cd src/TrustIdentity.Benchmarks
  dotnet run -c Release
  ```

### Key Scenarios Profiled
- **Password Hashing:** Verifies the speed (and cost) of PBKDF2/Argon2 hashing.
- **JWT Signing:** Measures the throughput of signing tokens with RSA/ECDSA keys.

## 🛡️ Security Audit

To perform a static security analysis of the codebase:

1. **Vulnerability Scan:**
   Check for vulnerable NuGet packages:
   ```bash
   dotnet list package --vulnerable
   ```

2. **Code Audit:**
   We recommend running a static analyzer like SonarQube or Roslyn Security Guard during CI builds.

## 🐛 Troubleshooting Tests

- **"Address in use"**: Integration tests run on `TestServer` and do not bind to real ports, so port conflicts should not occur.
- **Slow Tests**: The cryptographic tests (password hashing) are intentionally slow. Do not reduce the iteration count in production code to speed up tests; mock the hasher if necessary for unit tests.
