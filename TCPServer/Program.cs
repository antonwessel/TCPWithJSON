using System.Net.Sockets;
using System.Net;
using System.Text.Json;

const int Port = 5001;

var listener = new TcpListener(IPAddress.Any, Port); // Listen for any IP addresses on this port
listener.Start();
Console.WriteLine($"TCP Server is running on port {Port}...");

while (true)
{
    var client = await listener.AcceptTcpClientAsync(); // Wait for client before moving on
    var remote = client.Client.RemoteEndPoint?.ToString() ?? "unknown"; // Gets the endpoint for client if not null, otherwise return 'unknown'
    Console.WriteLine($"Client connected! {remote}");
    _ = Task.Run(() => HandleClientAsync(client, remote)); // Handle multiple clients (concurrent), discard return value
}

async Task HandleClientAsync(TcpClient client, string remote)
{
    await using var stream = client.GetStream(); // Data back and forth
    using var reader = new StreamReader(stream); // Reads from stream
    using var writer = new StreamWriter(stream); // Writes to the stream

    try
    {
        var line = await reader.ReadLineAsync();
        if (string.IsNullOrEmpty(line))
        {
            await Send(writer, new Response(false, error: "Empty request"));
            return;
        }
        else
        {
            Request? req;
            try
            {
                req = JsonSerializer.Deserialize<Request>(line);
            }
            catch (Exception)
            {
                await Send(writer, new Response(false, error: "Invalid JSON");
                return;
            }
        }


    }
    catch (Exception)
    {

        throw;
    }
}

// Send as JSON to client
static Task Send(StreamWriter writer, Response res)
{
    return writer.WriteLineAsync(JsonSerializer.Serialize(res));
}

// Good for sending data (I could also use classes here)
public record Request(string? method, int? a, int? b);
public record Response(bool ok, int? result = null, string? error = null);