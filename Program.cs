using System.Net;
using System.Net.Sockets;
using CoreOSC;
using CoreOSC.IO;
using SenseOSC;
using System.Text.Json;

var udpClient = new UdpClient();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();

string defaultwebSocketURL = "127.0.0.1";
int defaultWebSocketPort = 1234;

string WebSocketURL = defaultwebSocketURL;
int webSocketPort = defaultWebSocketPort;

app.MapPost("/osc", async (context) =>
{
    string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
    if (JsonSerializer.Deserialize<OSCData>(requestBody) is not OSCData data) {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return;
    }

    OscMessage oscMessage = new(new(data.Address), [data.Value]);
    try
    {
        await udpClient.SendMessageAsync(oscMessage);
    }
    catch (Exception)
    {
    }
    context.Response.StatusCode = (int)HttpStatusCode.OK;
    return;
});

app.MapPost("/settings", async (context) =>
{
    try
    {
        string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        var settings = JsonSerializer.Deserialize<SettingsData>(requestBody);
        if (settings == null || string.IsNullOrWhiteSpace(settings.WebSocketURL) || settings.WebSocketPort <= 0)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        WebSocketURL = settings.WebSocketURL;
        webSocketPort = settings.WebSocketPort;
        if (udpClient.Client.Connected)
        {
            udpClient.Close();
            udpClient = new UdpClient();
        }
        udpClient.Connect(WebSocketURL, webSocketPort);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        Console.WriteLine($"WebSocket URL: {WebSocketURL}");
        Console.WriteLine($"WebSocket Port: {webSocketPort}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while processing the request: {ex.Message}");
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    }
});

app.MapGet("/settings", async (context) =>
{
    var settingsData = new SettingsData
    {
        WebSocketURL = WebSocketURL,
        WebSocketPort = webSocketPort
    };

    var responseJson = JsonSerializer.Serialize(settingsData);
    await context.Response.WriteAsync(responseJson);

    Console.WriteLine($"WebSocket URL: {WebSocketURL}");
    Console.WriteLine($"WebSocket Port: {webSocketPort}");
});


app.Run();
