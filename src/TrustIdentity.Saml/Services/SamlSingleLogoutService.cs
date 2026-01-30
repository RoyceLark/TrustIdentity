using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using TrustIdentity.Saml.Models;
using TrustIdentity.Saml.Serialization;
using TrustIdentity.Saml.Security;
using Microsoft.Extensions.Logging;

namespace TrustIdentity.Saml.Services;

/// <summary>
/// Complete SAML Single Logout Implementation
/// </summary>
public class SamlSingleLogoutService
{
    private readonly SamlSerializer _serializer;
    private readonly SamlSigningService _signingService;
    private readonly ILogger<SamlSingleLogoutService> _logger;
    private readonly ISamlSessionStore _sessionStore;

    /// <summary>
    /// Initializes a new instance of the SamlSingleLogoutService
    /// </summary>
    /// <param name="serializer">The SAML serializer</param>
    /// <param name="signingService">The SAML signing service</param>
    /// <param name="sessionStore">The SAML session store</param>
    /// <param name="logger">The logger instance</param>
    public SamlSingleLogoutService(
        SamlSerializer serializer,
        SamlSigningService signingService,
        ISamlSessionStore sessionStore,
        ILogger<SamlSingleLogoutService> logger)
    {
        _serializer = serializer;
        _signingService = signingService;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    /// <summary>
    /// Create Logout Request (SP-initiated or IdP-initiated)
    /// </summary>
    /// <param name="nameId">The NameID of the session to log out</param>
    /// <param name="nameIdFormat">The format of the NameID</param>
    /// <param name="sessionIndex">The optional session index</param>
    /// <param name="issuer">The issuer entity ID</param>
    /// <param name="destination">The destination URL</param>
    /// <param name="signingCertificate">Optional certificate for signing the request</param>
    /// <returns>The serialized SAML LogoutRequest XML</returns>
    public string CreateLogoutRequest(
        string nameId,
        string nameIdFormat,
        string? sessionIndex,
        string issuer,
        string destination,
        X509Certificate2? signingCertificate = null)
    {
        try
        {
            var logoutRequest = new SamlLogoutRequest
            {
                Issuer = issuer,
                NameId = new SamlNameId
                {
                    Value = nameId,
                    Format = nameIdFormat
                },
                SessionIndex = sessionIndex
            };

            var xml = _serializer.SerializeLogoutRequest(logoutRequest);

            if (signingCertificate != null)
            {
                xml = _signingService.SignXml(xml, signingCertificate);
            }

            _logger.LogInformation("Created SAML Logout Request for NameID: {NameId}", nameId);
            return xml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create SAML Logout Request");
            throw;
        }
    }

    /// <summary>
    /// Process Logout Request from SP or IdP
    /// </summary>
    /// <param name="logoutRequestXml">The logout request XML</param>
    /// <param name="validationCertificate">Optional certificate for signature validation</param>
    /// <returns>A result object containing processing details</returns>
    public async Task<SamlLogoutRequestResult> ProcessLogoutRequestAsync(
        string logoutRequestXml,
        X509Certificate2? validationCertificate = null)
    {
        try
        {
            // Validate signature if certificate provided
            if (validationCertificate != null)
            {
                if (!_signingService.ValidateSignature(logoutRequestXml, validationCertificate))
                {
                    return new SamlLogoutRequestResult
                    {
                        Success = false,
                        ErrorStatus = SamlConstants.StatusCodes.Requester,
                        ErrorMessage = "Invalid signature on Logout Request"
                    };
                }
            }

            var logoutRequest = _serializer.DeserializeLogoutRequest(logoutRequestXml);
            if (logoutRequest == null)
            {
                return new SamlLogoutRequestResult
                {
                    Success = false,
                    ErrorStatus = SamlConstants.StatusCodes.Requester,
                    ErrorMessage = "Failed to deserialize Logout Request"
                };
            }

            _logger.LogInformation("Processing Logout Request for NameID: {NameId}", 
                logoutRequest.NameId.Value);

            // Find and terminate session
            var session = await _sessionStore.GetSessionByNameIdAsync(
                logoutRequest.NameId.Value, 
                logoutRequest.SessionIndex);

            if (session != null)
            {
                await _sessionStore.RemoveSessionAsync(session.SessionId);
                _logger.LogInformation("Terminated session: {SessionId}", session.SessionId);
            }

            return new SamlLogoutRequestResult
            {
                Success = true,
                LogoutRequest = logoutRequest,
                SessionTerminated = session != null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Logout Request");
            return new SamlLogoutRequestResult
            {
                Success = false,
                ErrorStatus = SamlConstants.StatusCodes.Responder,
                ErrorMessage = "Internal error processing Logout Request"
            };
        }
    }

    /// <summary>
    /// Create Logout Response
    /// </summary>
    /// <param name="inResponseTo">The ID of the original logout request</param>
    /// <param name="issuer">The issuer entity ID</param>
    /// <param name="destination">The destination URL</param>
    /// <param name="statusCode">The SAML status code</param>
    /// <param name="statusMessage">Optional status message</param>
    /// <param name="signingCertificate">Optional certificate for signing the response</param>
    /// <returns>The serialized SAML LogoutResponse XML</returns>
    public string CreateLogoutResponse(
        string inResponseTo,
        string issuer,
        string destination,
        string statusCode,
        string? statusMessage = null,
        X509Certificate2? signingCertificate = null)
    {
        try
        {
            var logoutResponse = new SamlLogoutResponse
            {
                Issuer = issuer,
                InResponseTo = inResponseTo,
                Status = new SamlStatus
                {
                    StatusCode = statusCode,
                    StatusMessage = statusMessage
                }
            };

            var xml = _serializer.SerializeLogoutResponse(logoutResponse);

            if (signingCertificate != null)
            {
                xml = _signingService.SignXml(xml, signingCertificate);
            }

            _logger.LogInformation("Created Logout Response with status: {Status}", statusCode);
            return xml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Logout Response");
            throw;
        }
    }

    /// <summary>
    /// Process Logout Response
    /// </summary>
    /// <param name="logoutResponseXml">The logout response XML</param>
    /// <param name="validationCertificate">Optional certificate for signature validation</param>
    /// <returns>A result object containing processing details</returns>
    public SamlLogoutResponseResult ProcessLogoutResponse(
        string logoutResponseXml,
        X509Certificate2? validationCertificate = null)
    {
        try
        {
            // Validate signature
            if (validationCertificate != null)
            {
                if (!_signingService.ValidateSignature(logoutResponseXml, validationCertificate))
                {
                    return new SamlLogoutResponseResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid signature on Logout Response"
                    };
                }
            }

            var logoutResponse = _serializer.DeserializeLogoutResponse(logoutResponseXml);
            if (logoutResponse == null)
            {
                return new SamlLogoutResponseResult
                {
                    Success = false,
                    ErrorMessage = "Failed to deserialize Logout Response"
                };
            }

            var success = logoutResponse.Status.StatusCode == SamlConstants.StatusCodes.Success;

            _logger.LogInformation("Processed Logout Response. Success: {Success}, Status: {Status}", 
                success, logoutResponse.Status.StatusCode);

            return new SamlLogoutResponseResult
            {
                Success = success,
                LogoutResponse = logoutResponse,
                StatusCode = logoutResponse.Status.StatusCode,
                StatusMessage = logoutResponse.Status.StatusMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Logout Response");
            return new SamlLogoutResponseResult
            {
                Success = false,
                ErrorMessage = "Internal error processing Logout Response"
            };
        }
    }

    /// <summary>
    /// Perform global logout across all service providers
    /// </summary>
    /// <param name="sessionId">The session ID to log out</param>
    /// <param name="serviceProviders">The list of service providers to notify</param>
    /// <param name="signingCertificate">The certificate to sign logout requests</param>
    /// <returns>A global result object</returns>
    public async Task<GlobalLogoutResult> PerformGlobalLogoutAsync(
        string sessionId,
        List<ServiceProviderEndpoint> serviceProviders,
        X509Certificate2 signingCertificate)
    {
        var result = new GlobalLogoutResult();

        try
        {
            var session = await _sessionStore.GetSessionAsync(sessionId);
            if (session == null)
            {
                result.Success = false;
                result.ErrorMessage = "Session not found";
                return result;
            }

            _logger.LogInformation("Performing global logout for session: {SessionId} across {Count} SPs", 
                sessionId, serviceProviders.Count);

            // Send logout requests to all SPs
            foreach (var sp in serviceProviders)
            {
                try
                {
                    var logoutRequest = CreateLogoutRequest(
                        session.NameId,
                        session.NameIdFormat,
                        session.SessionIndex,
                        session.IdpEntityId,
                        sp.LogoutEndpoint,
                        signingCertificate);

                    var spResult = new ServiceProviderLogoutResult
                    {
                        ServiceProvider = sp.EntityId,
                        LogoutRequest = logoutRequest,
                        Success = true
                    };

                    result.ServiceProviderResults.Add(spResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create logout request for SP: {EntityId}", sp.EntityId);
                    result.ServiceProviderResults.Add(new ServiceProviderLogoutResult
                    {
                        ServiceProvider = sp.EntityId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            // Terminate the session
            await _sessionStore.RemoveSessionAsync(sessionId);

            result.Success = true;
            result.SessionsTerminated = 1;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during global logout");
            result.Success = false;
            result.ErrorMessage = $"Global logout failed: {ex.Message}";
            return result;
        }
    }
}

/// <summary>
/// SAML Session Store Interface
/// </summary>
public interface ISamlSessionStore
{
    /// <summary>
    /// Retrieves a session by its ID
    /// </summary>
    Task<SamlSession?> GetSessionAsync(string sessionId);
    
    /// <summary>
    /// Retrieves a session by NameID and session index
    /// </summary>
    Task<SamlSession?> GetSessionByNameIdAsync(string nameId, string? sessionIndex);
    
    /// <summary>
    /// Creates a new SAML session
    /// </summary>
    Task CreateSessionAsync(SamlSession session);
    
    /// <summary>
    /// Removes a session by its ID
    /// </summary>
    Task RemoveSessionAsync(string sessionId);
    
    /// <summary>
    /// Retrieves all sessions for a specific NameID
    /// </summary>
    Task<List<SamlSession>> GetSessionsByNameIdAsync(string nameId);
}

/// <summary>
/// Represents a SAML session
/// </summary>
public class SamlSession
{
    /// <summary>
    /// Unique session ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Subject NameID
    /// </summary>
    public string NameId { get; set; } = string.Empty;
    
    /// <summary>
    /// Format of the NameID
    /// </summary>
    public string NameIdFormat { get; set; } = string.Empty;
    
    /// <summary>
    /// Session Index from IdP
    /// </summary>
    public string? SessionIndex { get; set; }
    
    /// <summary>
    /// Entity ID of the Identity Provider
    /// </summary>
    public string IdpEntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// When the session was created
    /// </summary>
    public DateTime Created { get; set; }
    
    /// <summary>
    /// When the session expires
    /// </summary>
    public DateTime Expires { get; set; }
    
    /// <summary>
    /// List of Service Providers in this session
    /// </summary>
    public List<string> ServiceProviders { get; set; } = new();
}

/// <summary>
/// Service Provider endpoint configuration for logout
/// </summary>
public class ServiceProviderEndpoint
{
    /// <summary>
    /// Entity ID of the Service Provider
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Logout endpoint URL
    /// </summary>
    public string LogoutEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Signing certificate for the Service Provider
    /// </summary>
    public X509Certificate2? Certificate { get; set; }
}

/// <summary>
/// Result of processing a logout request
/// </summary>
public class SamlLogoutRequestResult
{
    /// <summary>
    /// Valid request flag
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The processed logout request
    /// </summary>
    public SamlLogoutRequest? LogoutRequest { get; set; }
    
    /// <summary>
    /// Whether the session was terminated
    /// </summary>
    public bool SessionTerminated { get; set; }
    
    /// <summary>
    /// Error status code if failed
    /// </summary>
    public string? ErrorStatus { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of processing a logout response
/// </summary>
public class SamlLogoutResponseResult
{
    /// <summary>
    /// Valid response flag
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The processed logout response
    /// </summary>
    public SamlLogoutResponse? LogoutResponse { get; set; }
    
    /// <summary>
    /// Status code from response
    /// </summary>
    public string? StatusCode { get; set; }
    
    /// <summary>
    /// Status message from response
    /// </summary>
    public string? StatusMessage { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of a global logout operation
/// </summary>
public class GlobalLogoutResult
{
    /// <summary>
    /// Global success flag
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Number of sessions terminated
    /// </summary>
    public int SessionsTerminated { get; set; }
    
    /// <summary>
    /// Results for each service provider
    /// </summary>
    public List<ServiceProviderLogoutResult> ServiceProviderResults { get; set; } = new();
    
    /// <summary>
    /// Error message if global failure
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of logout for a single service provider
/// </summary>
public class ServiceProviderLogoutResult
{
    /// <summary>
    /// Service Provider Entity ID
    /// </summary>
    public string ServiceProvider { get; set; } = string.Empty;
    
    /// <summary>
    /// Success flag
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The logout request sent
    /// </summary>
    public string? LogoutRequest { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}