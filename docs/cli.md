# TrustIdentity CLI Tool

The `trust-id` CLI tool allows you to manage your TrustIdentity server from the command line.

## Installation

The CLI is available as a .NET tool or can be built from source.

```bash
dotnet run --project src/TrustIdentity.Cli -- [commands]
```

## Global Options

*   `--server <url>`: Specify the TrustIdentity server URL (default: `https://localhost:5001`)

## Commands

### Clients

List all registered clients:
```bash
trust-id client list
```

Create a new client:
```bash
trust-id client create <client_id> <client_name>
```

### Users

List all users:
```bash
trust-id user list
```

## Example Usage

```bash
# Register a new web portal client
trust-id client create web-portal "Customer Web Portal" --server https://identity.mycompany.com
```
