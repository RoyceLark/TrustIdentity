# Admin API Reference

TrustIdentity provides a comprehensive REST API for administrative tasks. All endpoints follow the `/api/v1/admin` prefix.

## Authentication
Admin API requires an access token with the `admin_api` scope.

## Clients
Endpoints for managing OAuth 2.0 and OpenID Connect clients.

### `GET /clients`
Returns a list of all registered clients.

### `POST /clients`
Creates a new client.
**Body:** `Client` object.

### `GET /clients/{id}`
Returns details for a specific client.

### `PUT /clients/{id}`
Updates an existing client.

### `DELETE /clients/{id}`
Deletes a client.

---

## Users
Endpoints for user management.

### `GET /users`
Search and list users with pagination.
**Query Params:** `search`, `page`, `pageSize`.

### `POST /users`
Create a new user.
**Body:** `{ username, email, password }`

### `POST /users/{id}/lock`
Locks a user account.

---

## Stats
Real-time diagnostic and usage statistics.

### `GET /stats`
Returns data for the admin dashboard, including issued tokens, fraud events, and login trends.

---

## Advanced Management

### `GET /grants/user/{subjectId}`
Lists all active grants (tokens/consent) for a specific user.

### `DELETE /grants/{key}`
Revokes a specific grant or token.

### `GET /sessions/user/{subjectId}`
Lists active server-side sessions for a user.
