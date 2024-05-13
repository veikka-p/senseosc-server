using System.Text.Json.Serialization;

namespace SenseOSC;

public sealed class OSCData {
    [JsonPropertyName("address")]
    public string Address { get; set; } = "";
    [JsonPropertyName("value")]
    public float Value { get; set; }
}
