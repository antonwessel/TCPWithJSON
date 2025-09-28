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

        Request? req;
        try
        {
            req = JsonSerializer.Deserialize<Request>(line);
        }
        catch (Exception)
        {
            await Send(writer, new Response(false, error: "Invalid JSON"));
            return;
        }

        // Validates request data
        if (req is null)
        {
            await Send(writer, new Response(false, error: "Invalid request object"));
            return;
        }
        if (string.IsNullOrWhiteSpace(req.method))
        {
            await Send(writer, new Response(false, error: "Field 'method' is missing"));
            return;
        }
        if (req.a is null || req.b is null)
        {
            await Send(writer, new Response(false, error: "Fields 'a' and 'b' must be valid integers"));
            return;
        }

        var method = req.method.Trim().ToLower();
        var a = req.a.Value; // I have to use .Value here 🤔
        var b = req.b.Value;

        // Runs method
        switch (method)
        {
            case "add":
                await Send(writer, new Response(true, result: a + b));
                break;
            case "subtract":
                await Send(writer, new Response(true, result: a - b));
                break;
            case "random":
                if (a > b)
                {
                    await Send(writer, new Response(false, error: "For 'Random', 'a' must be less than or equal to 'b'"));
                    break;
                }
                var value = (a == b) ? a : Random.Shared.Next(a, b + 1); // Shorthand for if-else. 'Shared' makes it thread safe.
                await Send(writer, new Response(true, result: value));
                break;
            default:
                await Send(writer, new Response(false, error: "Unknown method, please use: 'Add', 'Subtract', or 'Random'"));
                break;
        }

    }
    catch (Exception)
    {
        // Just ignore errors and close nicely 😎
    }
    finally
    {
        client.Close();
        Console.WriteLine("Client disconnected!");
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