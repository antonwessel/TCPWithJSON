using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;

const string Host = "127.0.0.1"; // Localhost (IP address)
const int Port = 5001; // Same as server

using var client = new TcpClient();
await client.ConnectAsync(Host, Port); // Connect to server

await using var stream = client.GetStream();
using var reader = new StreamReader(stream);
using var writer = new StreamWriter(stream);

// Validate and check that the method and the numbers are correct
var method = ReadMethod();
var (a, b) = ReadNumbers(method);

// Use the method and numbers to create a new request and turn it into JSON
var request = new Request(method, a, b);
var json = JsonSerializer.Serialize(request);

await writer.WriteAsync(json); // Write data to server
var reply = await reader.ReadLineAsync(); // Get data from server
if (string.IsNullOrEmpty(reply))
{
    Console.WriteLine("No reply from server.");
    return;
}

Response? res;
try
{
    res = JsonSerializer.Deserialize<Response>(reply);
}
catch (Exception)
{
    Console.WriteLine("Couldn't read JSON data.");
    return;
}

if (res is null)
{
    Console.WriteLine("Invalid answer.");
    return;
}

Console.WriteLine(res.ok ? $"Result: {res.result}" : $"Error: {res.error}");

static (int a, int b) ReadNumbers(string method)
{
    while (true)
    {
        Console.Write("Write two integers (a b): ");
        var parts = (Console.ReadLine() ?? "").Split(" ", StringSplitOptions.RemoveEmptyEntries); // Return an array containg the two numbers

        // Check to see if there's only 2 numbers and if they are both of type integer
        if (parts.Length == 2 && int.TryParse(parts[0], out var a) && int.TryParse(parts[1], out var b))
        {
            if (method.Equals("Random", StringComparison.CurrentCultureIgnoreCase) && a > b)
            {
                Console.WriteLine("Wrong inputs, for 'Random', 'b' has to be greater than or equal to 'a'");
                continue;
            }
            return (a, b);
        }
        Console.WriteLine("Invalid input. Example: 5 12");
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