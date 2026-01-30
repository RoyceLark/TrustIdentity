using System.CommandLine;
using System.CommandLine.Invocation;
using TrustIdentity.Abstractions.Models;
using System.Net.Http.Json;
using Newtonsoft.Json;

var rootCommand = new RootCommand("TrustIdentity CLI Tool");

var serverUrlOption = new Option<string>("--server", () => "https://localhost:5001", "The URL of the TrustIdentity server");
rootCommand.AddGlobalOption(serverUrlOption);

var clientCommand = new Command("client", "Manage OAuth2 clients");
var userCommand = new Command("user", "Manage users");

// Client Commands
var listClients = new Command("list", "List all clients");
listClients.SetHandler(async (server) =>
{
    var client = new HttpClient { BaseAddress = new Uri(server) };
    try 
    {
        var response = await client.GetAsync("/api/v1/admin/clients");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}, serverUrlOption);

var createClient = new Command("create", "Create a new client");
var clientIdArg = new Argument<string>("id", "The client ID");
var clientNameArg = new Argument<string>("name", "The client name");
createClient.AddArgument(clientIdArg);
createClient.AddArgument(clientNameArg);
createClient.SetHandler(async (server, id, name) =>
{
    var httpClient = new HttpClient { BaseAddress = new Uri(server) };
    var client = new Client { ClientId = id, ClientName = name, Enabled = true };
    try 
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/admin/clients", client);
        response.EnsureSuccessStatusCode();
        Console.WriteLine($"Client '{id}' created successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}, serverUrlOption, clientIdArg, clientNameArg);

clientCommand.AddCommand(listClients);
clientCommand.AddCommand(createClient);

// User Commands
var listUsers = new Command("list", "List all users");
listUsers.SetHandler(async (server) =>
{
    var client = new HttpClient { BaseAddress = new Uri(server) };
    try 
    {
        var response = await client.GetAsync("/api/v1/admin/users");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}, serverUrlOption);

userCommand.AddCommand(listUsers);

rootCommand.AddCommand(clientCommand);
rootCommand.AddCommand(userCommand);

return await rootCommand.InvokeAsync(args);
