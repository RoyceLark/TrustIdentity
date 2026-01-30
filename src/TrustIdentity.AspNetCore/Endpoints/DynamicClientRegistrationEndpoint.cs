using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.AspNetCore.Endpoints
{
    /// <summary>
    /// Endpoint for Dynamic Client Registration (RFC 7591)
    /// </summary>
    public class DynamicClientRegistrationEndpoint
    {
        /// <summary>
        /// Handles the dynamic client registration request
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <param name="service">The registration service</param>
        /// <returns>A task representing the operation</returns>
        public static async Task Handle(HttpContext context, IDynamicClientRegistrationService service)
        {
            if (!HttpMethods.IsPost(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return;
            }

            // Check Content-Type
            if (!context.Request.HasJsonContentType())
            {
               context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
               return;
            }

            try 
            {
                var request = await JsonSerializer.DeserializeAsync<DynamicClientRegistrationRequest>(context.Request.Body);
                if (request == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "invalid_request" });
                    return;
                }

                var response = await service.RegisterClientAsync(request);
                
                context.Response.StatusCode = StatusCodes.Status201Created;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "invalid_client_metadata", error_description = ex.Message });
            }
            catch (Exception ex)
            {
                // Log exception here if logger available
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "server_error", error_description = ex.Message });
            }
        }
    }
}
