using System.Net.Sockets;

const string Host = "127.0.0.1"; // Localhost (IP address)
const int Port = 5001; // Same as server

using var client = new TcpClient();
await client.ConnectAsync(Host, Port); // Connect to server

await using var stream = client.GetStream();
using var reader = new StreamReader(stream);
using var writer = new StreamWriter(stream);







public record Request(string? method, int? a, int? b);
public record Response(bool ok, int? result = null, string? error = null);