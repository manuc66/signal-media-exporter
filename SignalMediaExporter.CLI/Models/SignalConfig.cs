using System.Text.Json.Serialization;

public class SignalConfig
{
    [JsonPropertyName("key")] public string? Key { get; set; }
}