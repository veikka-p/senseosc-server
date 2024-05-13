using System.Text.Json.Serialization;

public class SettingsData
{
    [JsonPropertyName("WebSocketURL")]
    public required string? WebSocketURL { get; set; }

    [JsonPropertyName("WebSocketPort")]
    public int WebSocketPort { get; set; }
}