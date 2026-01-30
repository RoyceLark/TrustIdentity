# TrustIdentity Admin API

The Admin API provides programmatic access to manage the TrustIdentity server. It is secured using OAuth2 Client Credentials flow.

## Authentication

All requests to the Admin API require a valid JWT bearer token with the `admin_api` scope.

**Token Endpoint:** `POST /connect/token`
**Required Scope:** `admin_api`

## Endpoints

### Clients

*   `GET /api/v1/admin/clients`: List all clients
*   `GET /api/v1/admin/clients/{id}`: Get client details
*   `POST /api/v1/admin/clients`: Create a new client
*   `PUT /api/v1/admin/clients/{id}`: Update a client
*   `DELETE /api/v1/admin/clients/{id}`: Delete a client

### Users

*   `GET /api/v1/admin/users`: List all users (supports pagination)
*   `GET /api/v1/admin/users/{id}`: Get user details
*   `POST /api/v1/admin/users`: Create a new user
*   `PUT /api/v1/admin/users/{id}`: Update user profile
*   `DELETE /api/v1/admin/users/{id}`: Delete a user
*   `POST /api/v1/admin/users/{id}/reset-password`: Reset user password
*   `POST /api/v1/admin/users/{id}/lock`: Lock user account
*   `POST /api/v1/admin/users/{id}/unlock`: Unlock user account

### Resources

*   `GET /api/v1/admin/resources/identity`: List identity resources
*   `GET /api/v1/admin/resources/api-resources`: List API resources
*   `GET /api/v1/admin/resources/api-scopes`: List API scopes

### Stats & Auditing

*   `GET /api/v1/admin/stats`: Get dashboard statistics
*   `GET /api/v1/admin/audit`: Get recent audit events
