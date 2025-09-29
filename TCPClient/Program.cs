using System.Net.Sockets;

const string Host = "127.0.0.1"; // Localhost (IP address)
const int Port = 5001; // Same as server

using var client = new TcpClient();
await client.ConnectAsync(Host, Port); // Connect to server

await using var stream = client.GetStream();
using var reader = new StreamReader(stream);
using var writer = new StreamWriter(stream);

var method = ReadMethod();
var (a, b) = ReadNumbers(method);

static (int a, int b) ReadNumbers(string method)
{
    while (true)
    {
        Console.Write("Write two integers (a b): ");
        var parts = (Console.ReadLine() ?? "").Split(" ", StringSplitOptions.RemoveEmptyEntries); // Return an array containg the two numbers



    }
}

static string ReadMethod()
{
    // Only these methods are valid. Casing doesn't matter.
    var valid = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Add", "Subtract", "Random" };

    while (true)
    {
        Console.WriteLine("Method? (Add / Subtract / Random): ");
        var input = Console.ReadLine() ?? "".Trim(); // Null-coalescing operator ☺️

        if (valid.Contains(input))
            return input;

        Console.WriteLine("Invalid method, please try again. Use 'Add', 'Subtract', or 'Random'.");
    }
}



public record Request(string? method, int? a, int? b);
public record Response(bool ok, int? result = null, string? error = null);